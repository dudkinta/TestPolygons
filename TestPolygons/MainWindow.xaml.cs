using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace TestPolygons
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class fmMain : Window
    {
        private int polygonId = -1;  // для хранения номера полингона под курсором
        private int pId = -1; // для хранения номера точки под курсором

        public fmMain()
        {
            InitializeComponent();
            prepareCanvas();
            prepareToolPanel();
            prepareDBsubMenu();
            lbCanvases.ItemsSource = null;
            lbCanvases.Items.Clear();
            lbCanvases.ItemsSource = Elements.plgns;
            lbHint.Content = "Добро пожаловать в программу рисования полигонов";
        }  // инициализация

        private void prepareDBsubMenu()
        {
            mnuBD.Items.Clear();
            Dictionary<int, string> bdCollect = InOutData.getCollectNamesFromDB();
            if (bdCollect != null)
            {
                MenuItem mnuNewItem = new MenuItem();
                mnuNewItem.Header = "<Новый набор>";
                mnuNewItem.Click += mnuNewCollect_Click;
                mnuBD.Items.Add(mnuNewItem);
                mnuBD.Items.Add(new Separator());
                foreach (KeyValuePair<int, string> item in bdCollect)
                {
                    addMenuCollectItem(item.Key, item.Value);
                }
            }
            else { mnuBD.IsEnabled = false; }
        }

        private void addMenuCollectItem(int id, string name)
        {
            MenuItem mnuItem = new MenuItem();
            mnuItem.Header = name;
            MenuItem mnuItemLoad = new MenuItem();
            mnuItemLoad.Header = "_Загрузить";
            mnuItemLoad.Tag = new KeyValuePair<int, string>(id, name);
            mnuItemLoad.Click += mnuItemLoad_Click;
            MenuItem mnuItemSave = new MenuItem();
            mnuItemSave.Header = "_Сохранить";
            mnuItemSave.Tag = new KeyValuePair<int, string>(id, name);
            mnuItemSave.Click += mnuItemSave_Click;
            MenuItem mnuItemDelete = new MenuItem();
            mnuItemDelete.Header = "_Удалить";
            mnuItemDelete.Tag = new KeyValuePair<int, string>(id, name);
            mnuItemDelete.Click += mnuItemDelete_Click;
            mnuItem.Items.Add(mnuItemLoad);
            mnuItem.Items.Add(mnuItemSave);
            mnuItem.Items.Add(new Separator());
            mnuItem.Items.Add(mnuItemDelete);
            mnuBD.Items.Add(mnuItem);
        }

        void mnuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mnuCollect = sender as MenuItem;
            if (mnuCollect != null)
            {
                KeyValuePair<int, string> collect = (KeyValuePair<int, string>)mnuCollect.Tag;
                if (InOutData.deleteFromDB(collect.Key))
                {
                    MessageBox.Show("Набор полигонов удален из базы данных");
                    prepareDBsubMenu();
                }
                else
                {
                    MessageBox.Show("Ошибка удаления набора полигонов из базы данных");
                }
            }
        }

        private void mnuItemLoad_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mnuCollect = sender as MenuItem;
            if (mnuCollect != null)
            {
                KeyValuePair<int, string> collect = (KeyValuePair<int, string>)mnuCollect.Tag;
                if (InOutData.loadFromDB(collect.Key))
                {
                    refreshCanvas();
                }
                else
                {
                    MessageBox.Show("Ошибка загрузки набора полигонов из базы данных");
                }
            }
        }

        private void saveToDB(int id, string collectName)
        {
            if (InOutData.saveToDB(id, collectName))
            {
                prepareDBsubMenu();
                MessageBox.Show("Набор полигонов сохранен в базу данных");
            }
            else
            {
                MessageBox.Show("Ошибка сохранение набора полигонов в базу данных");
            }
        }
        
        private void mnuItemSave_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mnuCollect = sender as MenuItem;
            if (mnuCollect != null)
            {
                KeyValuePair<int, string> collect = (KeyValuePair<int, string>)mnuCollect.Tag;
                saveToDB(collect.Key, collect.Value);
            }
        }
        
        private void mnuNewCollect_Click(object sender, RoutedEventArgs e)
        {
            NewCollectDB fmNewCollectDBname = new NewCollectDB();
            fmNewCollectDBname.ShowDialog();
            if (fmNewCollectDBname.result)
            {
                saveToDB(-1, fmNewCollectDBname.collectName);
            }
        }

        private void prepareToolPanel() // подготовка панели инструментов
        {
            Tools.type = Tools.ToolType.polygon;
            btnToolArrow.IsChecked = false;
            btnToolPoly.IsChecked = true;
        }

        private void prepareCanvas() // подготовка канвы для присования
        {
            Polygon rect = new Polygon();
            rect.Points.Add(new Point(0, 0));
            rect.Points.Add(new Point(canvas.ActualWidth, 0));
            rect.Points.Add(new Point(canvas.ActualWidth, canvas.ActualHeight));
            rect.Points.Add(new Point(0, canvas.ActualHeight));
            rect.Fill = System.Windows.Media.Brushes.White;
            canvas.Children.Add(rect);
        }

        private void refreshCanvas() // обновление канвы
        {
            canvas.Children.Clear();
            prepareCanvas();
            foreach (Polygon polygon in Elements.polygons)
            {
                canvas.Children.Add(polygon);
            }
            canvas.Children.Add(Elements.line);
            canvas.Children.Add(Elements.currentPoint);
            foreach (Line line in Elements.unionLines)
            {
                canvas.Children.Add(line);
            }
            foreach (Polygon polygon in Elements.polyPolygon)
            {
                canvas.Children.Add(polygon);
            }
        }

        private void updateBinding()
        {
            lbCanvases.ItemsSource = null;
            lbCanvases.Items.Clear();
            lbCanvases.ItemsSource = Elements.plgns;
        }

        private void fmWPFMain_SizeChanged(object sender, SizeChangedEventArgs e) // изменение размеров окна 
        {
            refreshCanvas();
        }

        private void btnToolArrow_Click(object sender, RoutedEventArgs e) // выбор инструмента для перетаскивания точек
        {
            Tools.type = Tools.ToolType.arrow;
            btnToolArrow.IsChecked = true;
            btnToolPoly.IsChecked = false;
        }

        private void btnToolPoly_Click(object sender, RoutedEventArgs e) // выбор инструмента для рисования полигонов
        {
            Tools.type = Tools.ToolType.polygon;
            btnToolPoly.IsChecked = true;
            btnToolArrow.IsChecked = false;
        }

        private void fmWPFMain_PreviewKeyDown(object sender, KeyEventArgs e) // обработка команды от клавиатуры
        {
            if (e.Key == Key.Enter) // строим полигон
            {
                if (Tools.type == Tools.ToolType.polygon)  // если строим полигон то удаляем последнюю точку
                {
                    if (!Elements.addPolygon())
                    {
                        lbHint.Content = "Последний сегмент имеет самопересечение. Построение полигона невозможно.";
                    }
                }
            }
            if (e.Key == Key.Escape)
            {
                Elements.line.Points.Clear();
            }
            if ((e.Key == Key.Delete) || (e.Key == Key.Back))  // удаляем точки
            {
                if (Tools.type == Tools.ToolType.polygon)  // если строим полигон то удаляем последнюю точку
                {
                    Elements.deleteLastPoint();
                }
                else
                {
                    if ((polygonId != -1) && (pId != -1))  // если выбран полигон и точка то удаляем ее
                    {
                        if (!Elements.deletePolygonPoint(polygonId, pId))
                        {
                            lbHint.Content = "При попытке удалении точки возникло самопересечение. Удаление данной точки запрещено.";
                        }
                        else
                        {
                            polygonId = -1;
                            pId = -1;
                        }
                    }
                }
            }
            refreshCanvas();
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e) // нажатие по канве
        {
            canvas.Focus();
            Vector p = new Vector(Mouse.GetPosition(canvas));
            #region Добавление точки к полигону
            if ((e.LeftButton != MouseButtonState.Pressed) && (e.RightButton == MouseButtonState.Pressed))
            {
                if (!Elements.addPointPolygon(p))
                {
                    lbHint.Content = "Добавление точки приведет к самопересечению. Выберете другое место для добавления точки.";
                }
            }
            #endregion
            #region Добавление точки к недостроееному полигону
            if ((e.LeftButton == MouseButtonState.Pressed) && (e.RightButton != MouseButtonState.Pressed))
            {
                if (Tools.type == Tools.ToolType.polygon)
                {
                    Elements.addPoint(p);
                }
            }
            #endregion
            refreshCanvas();
        }

        private void canvas_PreviewMouseMove(object sender, MouseEventArgs e)  // обработка команд от мыши
        {
            Vector p = new Vector(Mouse.GetPosition(canvas));
            if (e.LeftButton == MouseButtonState.Released)
            {
                Vector cursor = Elements.getPoint(p, out polygonId, out pId, 10, (Elements.line.Points.Count == 0));
                if (polygonId != -1)   // переключатся на стрелку только если точка полигона под мышкой
                {
                    btnToolArrow_Click(null, null);
                }
                else
                {
                    if (Elements.polygons.Count < 2)
                    {
                        btnToolPoly_Click(null, null);
                    }
                }

                if (Tools.type == Tools.ToolType.polygon) // двигаем последний сегмент недостроенного полигона
                {
                    Elements.moveLastSegment(p);
                }
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Tools.type == Tools.ToolType.arrow)
                {
                    if ((polygonId != -1) && (pId != -1))  // двигаем точку полигона если только выбран конкретный полигон и конкретная точка
                    {
                        Elements.movePolygonPoint(p, polygonId, pId);
                        refreshCanvas();
                        lbHint.Content = Elements.debug;
                    }
                }
            }
        }

        private void mnuSaveFile_Click(object sender, RoutedEventArgs e)
        {
            InOutData.saveToFile();
        }

        private void mnuLoadFile_Click(object sender, RoutedEventArgs e)
        {
            InOutData.loadFromFile();
            refreshCanvas();
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)  // команда меню "выход"
        {
            this.Close();
        }

        private void mnuNew_Click(object sender, RoutedEventArgs e) // команда меню "новый"
        {
            canvas.Children.Clear();
            prepareCanvas();
            prepareToolPanel();
            Elements.line.Points.Clear();
            Elements.polygons = new List<Polygon>();
            Elements.plgns = new ObservableCollection<Canvas>();
            updateBinding();
        }

        private void mnuPrint_Click(object sender, RoutedEventArgs e)
        {
            InOutData.printPolygon(canvas, "Набор полигонов");
        }
    }
}
