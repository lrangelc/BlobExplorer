using System;
using System.IO;
using Microsoft.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;

namespace FileExplorer
{
    class Program
    {
        static void Main(string[] args)
        {
            Read_FileAsync();
        }

        static async System.Threading.Tasks.Task Read_FileAsync()
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            CloudStorageAccount cuentaAlmacenamiento = CloudStorageAccount.Parse(configuration.GetConnectionString("FileConnectionString"));
            CloudFileClient clienteArchivos = cuentaAlmacenamiento.CreateCloudFileClient();
            CloudFileShare archivoCompartido = clienteArchivos.GetShareReference("platzifile");

            if (await archivoCompartido.ExistsAsync())
            {
                CloudFileDirectory carpetaRaiz = archivoCompartido.GetRootDirectoryReference();
                CloudFileDirectory directorio = carpetaRaiz.GetDirectoryReference("registros");

                if (await directorio.ExistsAsync())
                {
                    CloudFile archivo = directorio.GetFileReference("logActividades.txt");
                    if (await archivo.ExistsAsync())
                    {
                        Console.WriteLine(archivo.DownloadTextAsync().Result);
                    }
                    else
                    {
                        Console.WriteLine("no se encontro el archivo: logActividades.txt");
                    }
                }
                else
                {
                    Console.WriteLine("no se encontro la carpeta: registros");
                }
            }
            Console.ReadLine();
        }
    }
}
