using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        public fmMain()// инициализация
        {
            InitializeComponent();
            prepareCanvas();
            prepareToolPanel();
            prepareDBsubMenu();
            updateBinding();
            lbHint.Content = "Добро пожаловать в программу рисования полигонов";
        }  
        
        void mnuPreviewPrint_Click(object sender, RoutedEventArgs e)  // Команда контекстного меню для печати маленьких картинок полигонов
        {
            MenuItem mnuItem = (MenuItem)e.OriginalSource;
            ContextMenu cMenu = (ContextMenu)mnuItem.Parent;
            ContentControl cControl = (ContentControl)cMenu.PlacementTarget;
            Canvas cnv = (Canvas)cControl.Content;
            InOutData.printPolygon(cnv, "Полигон");
        }

        private void prepareDBsubMenu()// подготовка меню Базы Данных. Если БД будет недоступно отключает это меню
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

        private void addMenuCollectItem(int id, string name)// добавление в меню БД элементов на загрузку, сохранение и удаление
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

        void mnuItemDelete_Click(object sender, RoutedEventArgs e)// Команда меню на удаление набора из БД
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

        private void mnuItemLoad_Click(object sender, RoutedEventArgs e)// Команда меню на загрузку набора из БД
        {
            MenuItem mnuCollect = sender as MenuItem;
            if (mnuCollect != null)
            {
                KeyValuePair<int, string> collect = (KeyValuePair<int, string>)mnuCollect.Tag;
                if (InOutData.loadFromDB(collect.Key))
                {
                    Elements.prepareUnionPolygon();
                    refreshCanvas();
                }
                else
                {
                    MessageBox.Show("Ошибка загрузки набора полигонов из базы данных");
                }
            }
        }

        private void saveToDB(int id, string collectName)// Сохранение набора в БД
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

        private void mnuItemSave_Click(object sender, RoutedEventArgs e)//Команда меню на сохранение набора в БД
        {
            MenuItem mnuCollect = sender as MenuItem;
            if (mnuCollect != null)
            {
                KeyValuePair<int, string> collect = (KeyValuePair<int, string>)mnuCollect.Tag;
                saveToDB(collect.Key, collect.Value);
            }
        }

        private void mnuNewCollect_Click(object sender, RoutedEventArgs e)//Команда меню на создание и сохранение нового набора в БД
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
                if ((int)polygon.Tag == -1)
                {
                    polygon.Fill = Brushes.Gray;
                    polygon.Stroke = Brushes.Black;
                    polygon.StrokeThickness = 1;
                    canvas.Children.Add(polygon);
                }
            }
            foreach (List<Polygon> polygons in Elements.polyPolygons)
            {
                for (int i = 0; i < polygons.Count; i++)
                {
                    Polygon polygon = polygons[i];
                    if (i == 0)
                    {
                        polygon.Fill = Brushes.Green;
                    }
                    else
                    {
                        polygon.Fill = Brushes.White;
                    }
                    polygon.Stroke = Brushes.Black;
                    polygon.StrokeThickness = 1;
                    canvas.Children.Add(polygon);
                }
            }
            canvas.Children.Add(Elements.line);
            canvas.Children.Add(Elements.currentPoint);
        }

        private void updateBinding()  // обновление привязки коллекций полигонов к UI
        {
            lbCanvases.ItemsSource = null;
            lbCanvases.Items.Clear();
            lbCanvases.ItemsSource = Elements.plgns;
            
            lbCanvasesUnion.ItemsSource = null;
            lbCanvasesUnion.Items.Clear();
            lbCanvasesUnion.ItemsSource = Elements.unionPolygons;
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
                    lbHint.Content = "Передвижение точки полигона левой кнопкой мыши. Удаление точки клавишей Delete или Backspace";
                }
                else
                {
                    btnToolPoly_Click(null, null);
                    lbHint.Content = "Что бы поставит точку нажмите левую кнопку мыши. Что бы добавить точку к полигону наждмите правую кнопку мыши";
                }
                if (Tools.type == Tools.ToolType.polygon) // двигаем последний сегмент недостроенного полигона
                {
                    Elements.moveLastSegment(p);
                    if (Elements.line.Points.Count > 0)
                    {
                        lbHint.Content = "Что бы удалить последнюю точку нажмите Delete или Backspace. Что бы удалить всю линию нажмите Esc";
                    }
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
                    }
                }
            }
        }

        private void mnuSaveFile_Click(object sender, RoutedEventArgs e)//Команда меню на сохранение набора в файл
        {
            InOutData.saveToFile();
        }

        private void mnuLoadFile_Click(object sender, RoutedEventArgs e)//Команда меню на загрузку набора из файла
        {
            InOutData.loadFromFile();
            Elements.prepareUnionPolygon();
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
            Elements.polyPolygons = new List<List<Polygon>>();
            Elements.unionPolygons = new ObservableCollection<Canvas>();
            updateBinding();
        }

        private void mnuPrint_Click(object sender, RoutedEventArgs e)//Команда меню на печать набора
        {
            InOutData.printPolygon(canvas, "Набор полигонов");
        }

        private void ToolPanel_MouseMove(object sender, MouseEventArgs e)  // Показывает подсказку над панелью инструментов
        {
            lbHint.Content = "Инструменты переключаются автоматичеки в зависимости от того что находится под указателем мыши.";
        }

        private void PreviewMousemove(object sender, MouseEventArgs e) // Показывает подсказку над миниатюрами полигонов
        {
            lbHint.Content = "Что бы распечатать полигон нажмите правую кнопку мыши и выберите \"Печать...\"";
        }  
    }
}
