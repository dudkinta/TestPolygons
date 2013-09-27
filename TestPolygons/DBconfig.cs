using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace TestPolygons
{
    static class DBconfig  // Класс конфигурации подключения к БД
    {
        public static string dbConnect;

        static DBconfig()  // Конструктор
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
