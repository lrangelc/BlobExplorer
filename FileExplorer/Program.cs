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
        const string ARCHIVO_NOMBRE = "logActividades.txt";
        const string DIRECTORIO_NOMBRE = "registros";

        static async System.Threading.Tasks.Task Main(string[] args)
        //static void Main(string[] args)
        {
            await Read_FileAsync();
        }

        static async System.Threading.Tasks.Task Read_FileAsync()
        {
            try
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
                //                    .SetBasePath(Directory.GetCurrentDirectory())
                //                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                IConfigurationRoot configuration = builder.Build();

                CloudStorageAccount cuentaAlmacenamiento = CloudStorageAccount.Parse(configuration.GetConnectionString("FileConnectionString"));
                CloudFileClient clienteArchivos = cuentaAlmacenamiento.CreateCloudFileClient();
                CloudFileShare archivoCompartido = clienteArchivos.GetShareReference("platzifile");

                bool existeFileShare = archivoCompartido.ExistsAsync().GetAwaiter().GetResult();
                //bool existeFileShare = await archivoCompartido.ExistsAsync();
                //.Net Framework no funciona el await con el .ExistsAsync()
                //agregar .GetAwaiter().GetResult()

                if (existeFileShare)
                {
                    CloudFileDirectory carpetaRaiz = archivoCompartido.GetRootDirectoryReference();
                    CloudFileDirectory directorio = carpetaRaiz.GetDirectoryReference("registros");

                    bool existeDirectorio = directorio.ExistsAsync().GetAwaiter().GetResult();

                    if (existeDirectorio)
                    {
                        CloudFile archivo = directorio.GetFileReference(ARCHIVO_NOMBRE);

                        bool existeArchivo = archivo.ExistsAsync().GetAwaiter().GetResult();

                        if (existeArchivo)
                        {
                            Console.WriteLine($"Escribiendo archivo: {ARCHIVO_NOMBRE}");
                            string x = archivo.DownloadTextAsync().GetAwaiter().GetResult();
                            //string x = await archivo.DownloadTextAsync();
                            Console.WriteLine(x);

                            //Console.WriteLine(archivo.DownloadTextAsync().Result);
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                Console.ReadLine();
            }
        }
    }
}
