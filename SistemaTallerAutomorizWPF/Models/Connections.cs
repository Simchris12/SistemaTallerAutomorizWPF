using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace SistemaTallerAutomorizWPF.Models
{
    internal class Connections
    {
        public static class Connection
        {
            public static String ConnectionString = "Data Source=localhost;Initial Catalog=MVVMLogindb;Integrated Security=True";

            public static SqlConnection ObtainConnection()
            {
                return new SqlConnection(ConnectionString);
            }
        }
    }
}
