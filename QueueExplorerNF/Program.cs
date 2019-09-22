using System;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace QueueExplorerNF
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            CloudStorageAccount miCuenta = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("QueueConnectionString"));
            CloudQueueClient clienteColas = miCuenta.CreateCloudQueueClient();
            CloudQueue queue = clienteColas.GetQueueReference("mifiladeprocesos");
            bool x = await queue.CreateIfNotExistsAsync();

            for (int i = 0; i < 3; i++)
            {
                CloudQueueMessage mensaje = new CloudQueueMessage($"Operacion:{i}");
                //queue.AddMessage(mensaje);
                await queue.AddMessageAsync(mensaje);
                Console.WriteLine($"El mensaje {i} ha sido publicado");
            }
            Console.WriteLine("Fin de Carga de Mensajes");
            Console.ReadLine();
        }
    }
}
