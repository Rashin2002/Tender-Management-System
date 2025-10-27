using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace TMS_NET8.Database
{
    public static class DBConnection
    {
        public static SqlConnection CreateConnection()
        {
            string connectionString = "Data Source=Rashinpc;Initial Catalog=DSK;Integrated Security=True;Encrypt=False;TrustServerCertificate=True";
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            return con;
        }
    }
}

//xmlns: local = "clr-namespace:TMS_NET8"
