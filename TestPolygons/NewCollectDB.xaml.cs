using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TestPolygons
{
    /// <summary>
    /// Логика взаимодействия для NewCollectDB.xaml
    /// </summary>
    public partial class NewCollectDB : Window
    {
        public string collectName = "";
        public bool result = false;
        public NewCollectDB()
        {
            InitializeComponent();
        }

        private void btOk_Click_1(object sender, RoutedEventArgs e)
        {
            result = true;
            collectName = tbCollectName.Text;
            this.Close();
        }

        private void btCancel_Click_1(object sender, RoutedEventArgs e)
        {
            result = false;
            this.Close();
        }
    }
}
