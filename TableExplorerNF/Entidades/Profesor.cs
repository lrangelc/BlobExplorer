using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableExplorerNF.Entidades
{
    public class Profesor:TableEntity
    {
        public Profesor(string identificador,string categoria)
        {
            this.RowKey = identificador;
            this.PartitionKey = categoria;
        }

        public Profesor()
        {

        }

        public string NombreProfesor { get; set; }
        public string NombreAsignatura { get; set; }
    }
}
