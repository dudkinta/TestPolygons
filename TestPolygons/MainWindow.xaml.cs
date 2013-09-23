using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
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
            lbHint.Content = "Добро пожаловать в программу рисования полигонов";
        }  // инициализация

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
            rect.Fill = System.Windows.Media.Brushes.LightSteelBlue;
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
            IEnumerable<UIElement> elements = from childs in Elements.plgns select
                                                  (from el in (IEnumerable<UIElement>)childs.Children where (el is Polygon) select el).DefaultIfEmpty(new Polygon()).FirstOrDefault();
            foreach (UIElement element in elements)
            {
                canvas.Children.Add(element);
            }
            canvas.Children.Add(Elements.line);
            canvas.Children.Add(Elements.currentPoint);
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

        private void mnuExit_Click(object sender, RoutedEventArgs e)  // команда меню "выход"
        {
            this.Close();
        }

        private void mnuNew_Click(object sender, RoutedEventArgs e) // команда меню "новый"
        {
            canvas.Children.Clear();
            prepareCanvas();
            prepareToolPanel();
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
            if ((e.Key == Key.Delete) || (e.Key == Key.Back))  // удаляем точки
            {
                if (Tools.type == Tools.ToolType.polygon)  // если строим полигон то удаляем последнюю точку
                {
                    Elements.deleteLastPoint();
                }
                else 
                {
                    if ((polygonId != -1) && (pId!=-1))  // если выбран полигон и точка то удаляем ее
                    {
                        if (!Elements.deletePolygonPoint(Elements.polygons[polygonId], pId))
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
                    btnToolPoly_Click(null, null);
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
                    }
                }
            }
        } 
    }
}
