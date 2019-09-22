using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;

namespace QueueHandlerNF
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string colaNombre = "mifiladeprocesos";
            string contenedorNombre = "contenedorarchivos";
            int registrosprocesar = 20;

            CloudStorageAccount miCuenta = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("QueueConnectionString"));

            CloudQueueClient clienteColas = miCuenta.CreateCloudQueueClient();
            CloudQueue queue = clienteColas.GetQueueReference(colaNombre);
            bool existeCola = await queue.ExistsAsync();

            if (existeCola)
            {
                CloudBlobClient clienteBlob = miCuenta.CreateCloudBlobClient();
                CloudBlobContainer contenedor = clienteBlob.GetContainerReference(contenedorNombre);
                bool existeContenedor = await contenedor.CreateIfNotExistsAsync();
                existeContenedor = await contenedor.ExistsAsync();

                if (existeContenedor)
                {
                    Console.WriteLine($"Procesando la cola: {colaNombre}");
                    foreach (CloudQueueMessage item in await queue.GetMessagesAsync(registrosprocesar))
                    {
                        Create_Directory("files");
                        string rutaArchivo = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "files"), $"Log{item.Id}.txt");
                        using (TextWriter archivoTemp = File.CreateText(rutaArchivo))
                        {
                            string mensaje = item.AsString;
                            archivoTemp.WriteLine(mensaje);
                        }
                        Console.WriteLine($"Archivo creado: {rutaArchivo}");
                        Upload_File(contenedorNombre, rutaArchivo);
                        await queue.DeleteMessageAsync(item);
                    }
                    Console.WriteLine($"Se finalizo de procesar la cola: {colaNombre}");
                }
            }
            else
            {
                Console.WriteLine($"No existe la cola: {colaNombre}");
            }
            Console.ReadLine();
        }

        private static string Create_Directory(string name)
        {
            // Specify the directory you want to manipulate.
            string path = Path.Combine(Directory.GetCurrentDirectory(), name);

            try
            {
                // Determine whether the directory exists.
                if (Directory.Exists(path))
                {
                    Console.WriteLine("That path exists already.");
                    return null;
                }

                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(path);
                Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(path));

                return path;
            }
            catch (Exception ex)
            {
                Console.WriteLine("The process failed: {0}", ex.ToString());
                return null;
            }
            finally
            {
            }
        }

        private static void Upload_File(string containerName, string file)
        {
            if (File.Exists(file))
            {
                System.Diagnostics.Stopwatch sw;

                sw = Stopwatch.StartNew();

                long length = new System.IO.FileInfo(file).Length;

                if (length > 1024000)
                {
                    Upload_File_Block(containerName, file);
                }
                else
                {
                    Upload_File_Stream(containerName, file);
                }

                sw.Stop();

                Console.WriteLine("Runtime in upload process: {0} seconds", sw.Elapsed.TotalMilliseconds / 1000);

            }
            else
            {
                Console.WriteLine("The file does not exists: {0}", Path.GetFileName(file));
                Console.WriteLine("Path: {0}", file);
            }
        }

        private static void Upload_File_Stream(string containerName, string origin_file, string folder = "")
        {
            try
            {
                Console.WriteLine(File.Exists(origin_file) ? "File exists." : "File does not exist.");

                if (File.Exists(origin_file))
                {
                    Console.WriteLine(origin_file);

                    CloudStorageAccount cuentaAlmacenamiento = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                    CloudBlobClient clienteBlob = cuentaAlmacenamiento.CreateCloudBlobClient();
                    CloudBlobContainer contenedor = clienteBlob.GetContainerReference(containerName);
                    contenedor.CreateIfNotExists();
                    contenedor.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

                    CloudBlockBlob miBlob = contenedor.GetBlockBlobReference((folder.Length == 0 ? "" : folder + "/") + Path.GetFileName(origin_file));
                    Console.WriteLine("Uploading file...");

                    using (var fileStream = System.IO.File.OpenRead(origin_file))
                    {
                        miBlob.UploadFromStream(fileStream);
                    }

                    Console.WriteLine("The file was uploaded successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The process failed: {0}", ex.ToString());
            }
            finally
            {
            }
        }

        public static string GetMD5HashFromStream(byte[] bytes)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private static void Upload_File_Block(string containerName, string origin_file, string folder = "")
        {
            CloudStorageAccount cuentaAlmacenamiento = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudBlobClient clienteBlob = cuentaAlmacenamiento.CreateCloudBlobClient();
            CloudBlobContainer contenedor = clienteBlob.GetContainerReference(containerName);
            contenedor.CreateIfNotExists();
            contenedor.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            CloudBlockBlob blob = contenedor.GetBlockBlobReference((folder.Length == 0 ? "" : folder + "/") + Path.GetFileName(origin_file));
            Console.WriteLine("Uploading file...");

            int blockSize = 256 * 1024; //256 kb

            using (FileStream fileStream = new FileStream(origin_file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                long fileSize = fileStream.Length;

                //block count is the number of blocks + 1 for the last one
                int blockCount = (int)((float)fileSize / (float)blockSize) + 1;

                //List of block ids; the blocks will be committed in the order of this list 
                List<string> blockIDs = new List<string>();

                //starting block number - 1
                int blockNumber = 0;

                try
                {
                    int bytesRead = 0; //number of bytes read so far
                    long bytesLeft = fileSize; //number of bytes left to read and upload

                    //do until all of the bytes are uploaded
                    while (bytesLeft > 0)
                    {
                        Console.WriteLine("blockNumber: {0}/{1}", blockNumber, blockCount);

                        blockNumber++;
                        int bytesToRead;
                        if (bytesLeft >= blockSize)
                        {
                            //more than one block left, so put up another whole block
                            bytesToRead = blockSize;
                        }
                        else
                        {
                            //less than one block left, read the rest of it
                            bytesToRead = (int)bytesLeft;
                        }

                        //create a blockID from the block number, add it to the block ID list
                        //the block ID is a base64 string
                        string blockId =
                          Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(string.Format("BlockId{0}",
                            blockNumber.ToString("0000000"))));
                        blockIDs.Add(blockId);
                        //set up new buffer with the right size, and read that many bytes into it 
                        byte[] bytes = new byte[bytesToRead];
                        fileStream.Read(bytes, 0, bytesToRead);

                        //calculate the MD5 hash of the byte array
                        string blockHash = GetMD5HashFromStream(bytes);

                        //upload the block, provide the hash so Azure can verify it
                        blob.PutBlock(blockId, new MemoryStream(bytes), blockHash);

                        //increment/decrement counters
                        bytesRead += bytesToRead;
                        bytesLeft -= bytesToRead;
                    }

                    //commit the blocks
                    blob.PutBlockList(blockIDs);

                    Console.WriteLine("The file was uploaded successfully.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Print("Exception thrown = {0}", ex);
                }
            }
        }
    }


}
