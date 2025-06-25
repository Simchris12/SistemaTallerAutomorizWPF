﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace SistemaTallerAutomorizWPF.Repositories
{
    public abstract class RepositoryBase
    {
        private readonly String _connectionString;
        public RepositoryBase()
        {
            _connectionString = "Server=(Local); Database=MVVMLogindb; Integrated Security=true";
        }
        protected SqlConnection GetConnection()
        {
            return new SqlConnection("Server=localhost;Database=MVVMLogindb;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;");
        }
    }
}
