using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace TableExplorerNF
{
    class Program
    {
        static void Main(string[] args)
        {
            CloudStorageAccount cuentaAlmacenamiento = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("TableConnectionString"));
            CloudTableClient clienteTablas = cuentaAlmacenamiento.CreateCloudTableClient();
            CloudTable tabla = clienteTablas.GetTableReference("documento");
            tabla.CreateIfNotExists();

            var tablas = clienteTablas.ListTables();

            foreach (CloudTable item in tablas)
            {
                Console.WriteLine(item.Name);
            }
            Console.ReadLine();
        }
    }
}
