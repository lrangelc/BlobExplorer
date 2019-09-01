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
        static void Main(string[] args)
        {

            CloudStorageAccount cuentaAlmacenamiento = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("FileConnectionString"));
            CloudFileClient clienteArchivos = cuentaAlmacenamiento.CreateCloudFileClient();
            CloudFileShare archivoCompartido = clienteArchivos.GetShareReference("platzifile");

            if (archivoCompartido.Exists())
            {
                CloudFileDirectory carpetaRaiz = archivoCompartido.GetRootDirectoryReference();
                CloudFileDirectory directorio = carpetaRaiz.GetDirectoryReference("registros");

                if (directorio.Exists())
                {
                    CloudFile archivo = directorio.GetFileReference("logActividades.txt");
                    if (archivo.Exists())
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
