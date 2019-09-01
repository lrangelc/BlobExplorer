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
            var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();


            Console.WriteLine("Hello World!");

            CloudStorageAccount cuentaAlmacenamiento = CloudStorageAccount.Parse(configuration.GetConnectionString("StorageConnectionString"));
            CloudBlobClient clienteBlob = cuentaAlmacenamiento.CreateCloudBlobClient();
            CloudBlobContainer contenedor = clienteBlob.GetContainerReference("yeap1");
            contenedor.CreateIfNotExistsAsync();
            contenedor.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            CloudBlockBlob miBlob = contenedor.GetBlockBlobReference("foto3.jpg");
            miBlob.UploadFromFileAsync(@"C:\\RPADev\\cursos\\paas\\eiab.jpg");

            Console.WriteLine("Tu contenedor esta listo y creado");
            Console.ReadLine();
        }
    }
}
