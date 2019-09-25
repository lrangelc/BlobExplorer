using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;

namespace BlobExplorer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Upload_FileAsync();
        }

        static async System.Threading.Tasks.Task Upload_FileAsync()
        {
            string origin_file = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "files"), "eiab.jpg");
            Console.WriteLine(File.Exists(origin_file) ? "File exists." : "File does not exist.");

            if (File.Exists(origin_file))
            {
#if DEBUG || DEV 
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile($"appsettings.Development.json", true, true);
#else
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile($"appsettings.json", true, true);
#endif
                //var builder = new ConfigurationBuilder()
                //    .SetBasePath(Directory.GetCurrentDirectory())
                //    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                IConfigurationRoot configuration = builder.Build();

                string file_name = "foto" + Guid.NewGuid().ToString() + ".jpg";
                Console.WriteLine("Hello World!");

                CloudStorageAccount cuentaAlmacenamiento = CloudStorageAccount.Parse(configuration.GetConnectionString("StorageConnectionString"));
                CloudBlobClient clienteBlob = cuentaAlmacenamiento.CreateCloudBlobClient();
                CloudBlobContainer contenedor = clienteBlob.GetContainerReference("yeap2");
                bool existeContenedor = contenedor.CreateIfNotExistsAsync().GetAwaiter().GetResult();
                _ = contenedor.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

                CloudBlockBlob miBlob = contenedor.GetBlockBlobReference(file_name);
                _ = miBlob.UploadFromFileAsync(origin_file);

                Console.WriteLine("Tu contenedor esta listo y creado");
                Console.ReadLine();
            }
        }
    }
}