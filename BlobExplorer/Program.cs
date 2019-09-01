using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BlobExplorer
{
    class Program
    {
        static void Main(string[] args)
        {
            Upload_File();
        }

        static void Upload_File()
        {
            string origin_file = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "files"), "eiab.jpg");
            Console.WriteLine(File.Exists(origin_file) ? "File exists." : "File does not exist.");

            if (File.Exists(origin_file))
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                IConfigurationRoot configuration = builder.Build();

                string file_name = "foto" + Guid.NewGuid().ToString() + ".jpg";
                Console.WriteLine("Hello World!");

                CloudStorageAccount cuentaAlmacenamiento = CloudStorageAccount.Parse(configuration.GetConnectionString("StorageConnectionString"));
                CloudBlobClient clienteBlob = cuentaAlmacenamiento.CreateCloudBlobClient();
                CloudBlobContainer contenedor = clienteBlob.GetContainerReference("yeap2");
                contenedor.CreateIfNotExistsAsync();
                contenedor.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

                CloudBlockBlob miBlob = contenedor.GetBlockBlobReference(file_name);
                var y = miBlob.UploadFromFileAsync(origin_file);

                Console.WriteLine("Tu contenedor esta listo y creado");
                Console.ReadLine();
            }
        }
    }
}
