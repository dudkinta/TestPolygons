using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Controls;

namespace TestPolygons
{
    class InOutData
    {
        private static DataTable dtCollectDB = new DataTable();

        public static bool saveToFile()
        {
            SaveFileDialog FDialog = new SaveFileDialog();
            FDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            FDialog.FilterIndex = 1;
            FDialog.RestoreDirectory = true;
            Nullable<bool> result = FDialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    System.IO.File.WriteAllText(FDialog.FileName, prepareData());
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            return true;
        }
        
        public static bool loadFromFile()
        {
            OpenFileDialog FDialog = new OpenFileDialog();
            FDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            FDialog.FilterIndex = 1;
            FDialog.RestoreDirectory = true;
            Nullable<bool> result = FDialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    restoreData(System.IO.File.ReadAllText(FDialog.FileName));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            return true;
        }

        public static bool saveToDB(int id, string name)
        {
            string pointsData = prepareData();
            try
            {
                if (id != -1)  // проверка существующей записи. если есть, то UPDATE если нет, то INSERT
                {
                    SqlCommand sqlCommand = new SqlCommand("SELECT id FROM [dbo].[collects] WHERE id=@id;");
                    sqlCommand.Connection = new SqlConnection(DBconfig.dbConnect);
                    sqlCommand.Parameters.Clear();
                    sqlCommand.Parameters.AddWithValue("@id", id);
                    SqlDataAdapter da = new SqlDataAdapter(sqlCommand);
                    DataTable tmpRes = new DataTable();
                    da.Fill(tmpRes);
                    if (tmpRes.Rows.Count == 0)
                    {
                        id = -1;
                    }
                }
                if (id == -1)  // INSERT в базу данных
                {
                    SqlCommand sqlCommand = new SqlCommand("INSERT INTO [dbo].[collects] ([name],[points_data]) VALUES (@name, @pointData);");
                    sqlCommand.Connection = new SqlConnection(DBconfig.dbConnect);
                    sqlCommand.Parameters.Clear();
                    sqlCommand.Parameters.AddWithValue("@name", name);
                    sqlCommand.Parameters.AddWithValue("@pointData", pointsData);
                    sqlCommand.Connection.Open();
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Connection.Close();
                }
                if (id != -1)  // UPDATE в базу данных
                {
                    SqlCommand sqlCommand = new SqlCommand("UPDATE [dbo].[collects] SET [name] = @name, [points_data] = @pointData WHERE id=@id;");
                    sqlCommand.Connection = new SqlConnection(DBconfig.dbConnect);
                    sqlCommand.Parameters.Clear();
                    sqlCommand.Parameters.AddWithValue("@id", id);
                    sqlCommand.Parameters.AddWithValue("@name", name);
                    sqlCommand.Parameters.AddWithValue("@pointData", pointsData);
                    sqlCommand.Connection.Open();
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Connection.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
            return true;
        }
        
        public static bool loadFromDB(int id)
        {
            try
            {
                SqlCommand sqlCommand = new SqlCommand("SELECT TOP 1 * FROM [dbo].[collects] WHERE id=@id;");
                sqlCommand.Connection = new SqlConnection(DBconfig.dbConnect);
                sqlCommand.Parameters.Clear();
                sqlCommand.Parameters.AddWithValue("@id", id);
                SqlDataAdapter da = new SqlDataAdapter(sqlCommand);
                DataTable tmpRes = new DataTable();
                da.Fill(tmpRes);
                if (tmpRes.Rows.Count == 0)
                {
                    return false;
                }
                else
                {
                        int pid = (int)tmpRes.Rows[0]["id"];
                        string name = tmpRes.Rows[0]["name"].ToString();
                        string points = tmpRes.Rows[0]["points_data"].ToString();
                        restoreData(points);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return true;
        }

        private static string prepareData()
        {
            List<List<Vector>> points = new List<List<Vector>>();
            for (int i = 0; i < Elements.polygons.Count; i++)
            {
                List<Vector> poly = Elements.savePolygonPoints(Elements.polygons[i]);
                points.Add(poly);
            }
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<List<Vector>>));
            ser.WriteObject(stream, points);
            return System.Text.Encoding.Default.GetString(stream.ToArray());
        }  

        private static void restoreData(string pointsStr)
        {
            Stream stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(pointsStr));
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<List<Vector>>));
            List<List<Vector>> points = (List<List<Vector>>)ser.ReadObject(stream);
            Elements.polygons.Clear();
            Elements.plgns.Clear();
            foreach (List<Vector> pgPoints in points)
            {
                Polygon polygon = new Polygon();
                SolidColorBrush sbrush = new SolidColorBrush(Color.FromArgb(200, 127, 127, 127));
                polygon.Fill = sbrush;
                polygon.StrokeThickness = 2;
                polygon.Stroke = Brushes.Blue;
                Elements.restorePolygonPoints(pgPoints, polygon);
                Elements.polygons.Add(polygon);
                Elements.addToCollect(polygon);
            }
        }

        public static Dictionary<int, string> getCollectNamesFromDB()
        {
            Dictionary<int, string> res = new Dictionary<int, string>();
            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM [dbo].[collects];");
            sqlCommand.Connection = new SqlConnection(DBconfig.dbConnect);
            sqlCommand.Parameters.Clear();
            try
            {
                SqlDataAdapter da = new SqlDataAdapter(sqlCommand);
                dtCollectDB.Clear();
                da.Fill(dtCollectDB);
                for (int i = 0; i < dtCollectDB.Rows.Count; i++)
                {
                    int id = (int)dtCollectDB.Rows[i]["id"];
                    string name = dtCollectDB.Rows[i]["name"].ToString();
                    res.Add(id, name);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
            return res;
        }

        public static void printPolygon(Canvas cnv, string description)
        {
            PrintDialog PDialog = new PrintDialog();
            Nullable<bool> result = PDialog.ShowDialog();
            if (result == true)
            {
                PDialog.PrintVisual(cnv, description);
            }
        }
    }
}
