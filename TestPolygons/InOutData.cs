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

namespace TestPolygons
{
    class InOutData
    {
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
        public static bool saveToDB()
        {

            string pointsData = prepareData();
            polygonDataSet polygonDS = new polygonDataSet();
            polygonDataSet.CollectionsDataTable collectionsDT = polygonDS.Collections;
            polygonDataSet.CollectionsRow collectionRow = collectionsDT.NewCollectionsRow();

            collectionRow["name"] = "data1";
            collectionRow["data"] = pointsData;
            collectionsDT.AddCollectionsRow(collectionRow);

            //this.personTableTableAdapter1.Update(collectionsDT);

            return true;
        }
        public static bool loadFromDB()
        {
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
    }
}
