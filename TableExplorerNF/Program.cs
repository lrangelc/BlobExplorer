using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using TableExplorerNF.Entidades;

namespace TableExplorerNF
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("inicia proceso. Presione cualquier tecla.");
            Console.ReadLine();
            CloudStorageAccount cuentaAlmacenamiento = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("TableConnectionString"));
            CloudTableClient clienteTablas = cuentaAlmacenamiento.CreateCloudTableClient();

            CloudTable tabla = clienteTablas.GetTableReference("documentos");
            //tabla.DeleteIfExists();
            //Console.WriteLine("Tabla eliminada");


            tabla.CreateIfNotExists();

            var tablas = clienteTablas.ListTables();

            foreach (CloudTable item in tablas)
            {
                Console.WriteLine(item.Name);
            }


            Profesor profeUno = new Profesor(Guid.NewGuid().ToString(), "Profesores");
            profeUno.NombreProfesor = "Ricardo Selis";
            profeUno.NombreAsignatura = "Microcontroladores";

            Profesor profeDos = new Profesor(Guid.NewGuid().ToString(), "Profesores");
            profeDos.NombreProfesor = "Carlos Paredes";
            profeDos.NombreAsignatura = "Diseno Audiovisual";

            Profesor profeTres = new Profesor("007", "Profesores");
            profeTres.NombreProfesor = "Victoria Moreira";
            profeTres.NombreAsignatura = "Arquitectura";

            TableOperation insertarProfeUno = TableOperation.Insert(profeUno);
            TableOperation insertarProfeDos = TableOperation.Insert(profeDos);
            TableOperation insertarProfeTres = TableOperation.Insert(profeTres);

            tabla.Execute(insertarProfeUno);
            tabla.Execute(insertarProfeDos);
            tabla.Execute(insertarProfeTres);

            Console.WriteLine("Se han insertado todos los profesores creados");





            TableOperation operacionModificar = TableOperation.Retrieve<Profesor>("Profesores", "002");
            TableResult resultadoObtenido = tabla.Execute(operacionModificar);
            Profesor entidadModificada = (Profesor)resultadoObtenido.Result;
            if (entidadModificada != null)
            {
                entidadModificada.NombreProfesor = "Rocio Pinzon";
                entidadModificada.NombreAsignatura = "Baile";
                TableOperation operacionActualizar = TableOperation.Replace(entidadModificada);
                tabla.Execute(operacionActualizar);
                Console.WriteLine("Entidad Actualizada");
            }
            else
            {
                Console.WriteLine("No se encontro la entidad");
            }




            TableQuery<Profesor> consulta = new TableQuery<Profesor>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, "000"));

            foreach (Profesor profe in tabla.ExecuteQuery(consulta))
            {
                Console.WriteLine("{0}, {1}\t{2}\t{3}", profe.PartitionKey, profe.RowKey, profe.NombreProfesor, profe.NombreAsignatura);
            }



            TableOperation operacionEliminar = TableOperation.Retrieve<Profesor>("Profesores", "007");
            TableResult resultadoObtenidoEliminar = tabla.Execute(operacionEliminar);
            Profesor entidadEliminar = (Profesor)resultadoObtenidoEliminar.Result;
            if (entidadEliminar != null)
            {
                TableOperation operacionDelete = TableOperation.Delete(entidadEliminar);
                tabla.Execute(operacionDelete);
                Console.WriteLine("Entidad Actualizada");
            }
            else
            {
                Console.WriteLine("No se encontro la entidad");
            }


            Console.ReadLine();

        }
    }
}
