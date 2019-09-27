using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace ClienteDocumentDBNF
{
    class Program
    {
        private static string Host = ConfigurationManager.AppSettings["Host"];
        private static string User = ConfigurationManager.AppSettings["User"];
        private static string DBname = ConfigurationManager.AppSettings["DBname"];
        private static string Password = ConfigurationManager.AppSettings["Password"];
        private static string Port = ConfigurationManager.AppSettings["Port"];

        static void Main(string[] args)
        {
            string connString = String.Format("Server={0};Database={2};Port={3};User Id={1};Password={4};Ssl Mode=Require;", Host, User, DBname, Port, Password);

            var conn = new NpgsqlConnection(connString);

            Console.Out.WriteLine("Abriendo conexion");
            conn.Open();

            var command = conn.CreateCommand();
            command.CommandText = "DROP TABLE IF EXISTS inventario";
            command.ExecuteNonQuery();
            Console.Out.WriteLine("Tabla eliminada (si existia)");

            command.CommandText = "CREATE TABLE inventario (id serial PRIMARY KEY, name VARCHAR(50), quantity INTEGER,inserted_at Timestamp);";
            command.ExecuteNonQuery();
            Console.Out.WriteLine("Tabla Creada");

            command.CommandText =
                String.Format(
                    @"
                INSERT INTO inventario (name, quantity,inserted_at) VALUES ({0},{1},now());
                INSERT INTO inventario (name, quantity,inserted_at) VALUES ({2},{3},now());
                INSERT INTO inventario (name, quantity,inserted_at) VALUES ({4},{5},now());
            ",
                    "\'banana\'", 150,
                    "\'naranja\'", 154,
                    "\'manzana\'", 100
                    );
            int nRows = command.ExecuteNonQuery();
            Console.Out.WriteLine(String.Format("Numero de filas insertadas={0}", nRows));

            Console.Out.WriteLine("Cerrando conexion");
            conn.Close();
            Console.ReadLine();

        }

    }
}
