using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace TestPolygons
{
    static class DBconfig
    {
        public static string dbConnect;

        static DBconfig()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder["Server"] = "localhost";
            builder["Integrated Security"] = "true";
            builder["User Instance"] = "false";
            builder["Database"] = "polyDB";
            builder["Pooling"] = "false";
            dbConnect = builder.ConnectionString;
        }
    }
}
