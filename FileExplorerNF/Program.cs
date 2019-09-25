using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;

namespace FileExplorerNF
{
    class Program
    {
        const string ARCHIVO_NOMBRE = "logActividades.txt";
        const string DIRECTORIO_NOMBRE = "registros";

        static void Main(string[] args)
        {
            CloudStorageAccount cuentaAlmacenamiento = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("FileConnectionString"));
            CloudFileClient clienteArchivos = cuentaAlmacenamiento.CreateCloudFileClient();
            CloudFileShare archivoCompartido = clienteArchivos.GetShareReference("platzifile");

            if (archivoCompartido.Exists())
            {
                CloudFileDirectory carpetaRaiz = archivoCompartido.GetRootDirectoryReference();
                CloudFileDirectory directorio = carpetaRaiz.GetDirectoryReference(DIRECTORIO_NOMBRE);

                if (directorio.Exists())
                {
                    Console.WriteLine($"Archivo a leer: {ARCHIVO_NOMBRE}");
                    CloudFile archivo = directorio.GetFileReference(ARCHIVO_NOMBRE);
                    if (archivo.Exists())
                    {
                        Console.WriteLine(archivo.DownloadTextAsync().Result);
                    }
                    else
                    {
                        Console.WriteLine($"no se encontro el archivo: {ARCHIVO_NOMBRE}");
                    }
                }
                else
                {
                    Console.WriteLine($"no se encontro la carpeta: {DIRECTORIO_NOMBRE}");
                }
            }
            Console.ReadLine();
        }
    }
}
