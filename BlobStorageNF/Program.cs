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

namespace BlobStorageNF
{
    class Program
    {
        static void Main(string[] args)
        {
            string origin_file = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "files"), "eiab.jpg");
            Console.WriteLine(File.Exists(origin_file) ? "File exists." : "File does not exist.");

            if (File.Exists(origin_file))
            {
                CloudStorageAccount cuentaAlmacenamiento = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudBlobClient clienteBlob = cuentaAlmacenamiento.CreateCloudBlobClient();
                CloudBlobContainer contenedor = clienteBlob.GetContainerReference("contenedorcodigo");
                contenedor.CreateIfNotExists();
                contenedor.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

                string file_name = "foto" + Guid.NewGuid().ToString() + ".jpg";

                CloudBlockBlob miBlob = contenedor.GetBlockBlobReference(file_name);

                using (var fileStream = System.IO.File.OpenRead(origin_file))
                {
                    miBlob.UploadFromStream(fileStream);
                }

                Console.WriteLine("Tu contenedor esta listo y creado");
                Console.ReadLine();
            }
        }
    }
}
