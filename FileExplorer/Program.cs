using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;

namespace FileExplorer
{
    class Program
    {
        static void Main(string[] args)
        //static async System.Threading.Tasks.Task Main(string[] args)
        {
            Upload_File();
        }

        static async System.Threading.Tasks.Task Upload_File()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                IConfigurationRoot configuration = builder.Build();

                Console.WriteLine("Hello World!");

                CloudStorageAccount cuentaAlmacenamiento = CloudStorageAccount.Parse(configuration.GetConnectionString("FileConnectionString"));
                CloudFileClient clienteArchivos = cuentaAlmacenamiento.CreateCloudFileClient();

                CloudFileShare archivoCompartido = clienteArchivos.GetShareReference("platzifile3");
                bool x = await archivoCompartido.CreateIfNotExistsAsync();
                //Console.ReadLine();
                //bool x = await archivoCompartido.ExistsAsync();
                if (await archivoCompartido.ExistsAsync())
                {
                    Console.WriteLine("hi");
                }
                else
                {
                    Console.WriteLine("bye");
                }
            }
            catch (StorageException exStorage)
            {
                //Common.WriteException(exStorage);
                Console.WriteLine(
                    "Please make sure your storage account has storage file endpoint enabled and specified correctly in the app.config - then restart the sample.");
                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine("    Exception thrown creating share.");
                //Common.WriteException(ex);
                throw;
            }

        }
    }
}
