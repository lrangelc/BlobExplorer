﻿using System;
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
            Console.WriteLine("inicia proceso");
            Console.ReadLine();
            CloudStorageAccount cuentaAlmacenamiento = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("TableConnectionString"));
            CloudTableClient clienteTablas = cuentaAlmacenamiento.CreateCloudTableClient();

            CloudTable tabla = clienteTablas.GetTableReference("documento");
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

            TableOperation insertarProfeUno = TableOperation.Insert(profeUno);
            TableOperation insertarProfeDos = TableOperation.Insert(profeDos);

            tabla.Execute(insertarProfeUno);
            tabla.Execute(insertarProfeDos);

            Console.WriteLine("Se han insertado todos los profesores creados");


            TableQuery<Profesor> consulta = new TableQuery<Profesor>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan,"000"));

            foreach (Profesor profe in tabla.ExecuteQuery(consulta))
            {
                Console.WriteLine("{0}, {1}\t{2}\t{3}",profe.PartitionKey,profe.RowKey,profe.NombreProfesor,profe.NombreAsignatura);
            }

            Console.ReadLine();

        }
    }
}