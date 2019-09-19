using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Diagnostics;

namespace BlobStorageNF
{
    class Program
    {
        static void Main(string[] args)
        {
            //string file = BackUp_DataBase_1();
            string file = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "files"), "eiab.jpg");

            Upload_File(file);
            Console.ReadLine();
        }

        private static void Upload_File(string file)
        {
            if (File.Exists(file))
            {
                System.Diagnostics.Stopwatch sw;

                sw = Stopwatch.StartNew();

                long length = new System.IO.FileInfo(file).Length;

                if (length > 1024000)
                {
                    Upload_File_Block(file);
                }
                else
                {
                    Upload_File_Stream(file);
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

        private static string BackUp_DataBase_1()
        {
            try
            {
                DataSet dsComando_1 = new DataSet();
                DataSet dsComando_2 = new DataSet();
                SqlConnection vConn = new SqlConnection();
                SqlDataAdapter daComando_1 = new SqlDataAdapter();
                SqlDataAdapter daComando_2 = new SqlDataAdapter();
                string vConnectionString = ConfigurationManager.ConnectionStrings["RPA_LAN"].ConnectionString;
                string vSQL = "";
                string vArchivo_bak = "";
                string vArchivo_zip = "";
                string directory = "";
                string archivo_nombre = "";

                vConn.ConnectionString = vConnectionString;

                daComando_1.SelectCommand = new SqlCommand();
                daComando_2.SelectCommand = new SqlCommand();

                archivo_nombre = vConn.Database + "_" +
                    DateTime.Now.Year.ToString() +
                    DateTime.Now.Month.ToString() +
                    DateTime.Now.Day.ToString() + "_" +
                    DateTime.Now.Hour.ToString() +
                    DateTime.Now.Minute.ToString() +
                    DateTime.Now.Second.ToString();
                directory = archivo_nombre;
                vArchivo_zip = archivo_nombre + ".zip";
                vArchivo_bak = archivo_nombre + ".bak";
                Create_Directory(directory);

                directory = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "files"), directory);
                archivo_nombre = vArchivo_zip;


                vArchivo_bak = Path.Combine(directory, vArchivo_bak);
                vArchivo_zip = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "files"), vArchivo_zip);

                vSQL = "BACKUP DATABASE " + vConn.Database +
                    " TO DISK = '" + vArchivo_bak + "'" +
                    " WITH FORMAT," +
                    " NAME = 'Full Backup of " + vConn.Database + "';";

                daComando_1.SelectCommand.Connection = vConn;
                daComando_1.SelectCommand.CommandText = vSQL;
                daComando_1.SelectCommand.CommandType = CommandType.Text;

                daComando_1.Fill(dsComando_1);

                vConn.Close();

                Console.WriteLine("The bk file was created successfully at {0}.", Directory.GetCreationTime(vArchivo_bak));

                string startPath = vArchivo_bak;
                string zipPath = vArchivo_zip;

                ZipFile.CreateFromDirectory(directory, zipPath);
                Console.WriteLine("The zip file was created successfully at {0}.", Directory.GetCreationTime(vArchivo_zip));

                return vArchivo_zip;
            }
            catch (Exception ex)
            {
                //new RaygunClient().SendInBackground(ex);
                return null;
            }
        }

        private static string Create_Directory(string nombre)
        {
            // Specify the directory you want to manipulate.
            string path = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "files"), nombre);

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

        private static void Upload_File_Stream(string origin_file, string folder = "")
        {
            try
            {
                Console.WriteLine(File.Exists(origin_file) ? "File exists." : "File does not exist.");

                if (File.Exists(origin_file))
                {
                    Console.WriteLine(origin_file);

                    CloudStorageAccount cuentaAlmacenamiento = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                    CloudBlobClient clienteBlob = cuentaAlmacenamiento.CreateCloudBlobClient();
                    CloudBlobContainer contenedor = clienteBlob.GetContainerReference("rpaposlan");
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

        private static void Upload_File_Block(string origin_file, string folder = "")
        {
            CloudStorageAccount cuentaAlmacenamiento = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudBlobClient clienteBlob = cuentaAlmacenamiento.CreateCloudBlobClient();
            CloudBlobContainer contenedor = clienteBlob.GetContainerReference("rpaposlan");
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