using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace TestPolygons
{
    static class DBconfig  // Класс конфигурации подключения к БД
    {
        public static string dbConnect;
        private static Dictionary<string, string> config = new Dictionary<string, string>();

        static DBconfig()  // Конструктор
        {
            loadConfig();
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder["Server"] = config["Server"];
            if (config["Login"] == "")
            {
                builder["Integrated Security"] = "true";
            }
            else
            {
                builder["Username"] = config["Login"];
                builder["Password"] = config["Passwors"];
            }
            builder["Database"] = "polyDB";
            dbConnect = builder.ConnectionString;
        }
        private static void loadConfig()
        {
            string cfgFile = "db.cfg";
            config.Add("Server", "localhost");
            config.Add("Login", "login");
            config.Add("Password", "password");
            config.Add("DataBase", "database");
            try
            {
                restoreCFG(System.IO.File.ReadAllText(cfgFile));
            }
            catch
            {
                config.Add("Server", "127.0.0.1");
                config.Add("Login", "");
                config.Add("Password", "");
                config.Add("DataBase", "");
            }
        }

        private static void restoreCFG(string cfgStr)
        {
            Stream stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(cfgStr));
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Dictionary<string, string>));
            config = (Dictionary<string, string>)ser.ReadObject(stream);
        }
    }
}
