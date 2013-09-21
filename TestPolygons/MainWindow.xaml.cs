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
        private int indexMovePoint;
        private Ellipse currentPoint = new Ellipse();

        public fmMain()
        {
            InitializeComponent();
            prepareCanvas();
            prepareToolPanel();
        }

        private void prepareToolPanel() // подготовка панели инструментов
        {
            Tools.type = Tools.ToolType.arrow;
            btnToolArrow.IsChecked = true;
            btnToolPoly.IsChecked = false;
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

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e) // нажатие по канве
        {
            canvas.Focus();
            Vector p = new Vector(Mouse.GetPosition(canvas));
            if (Tools.type == Tools.ToolType.polygon)
            {
                List<UIElement> elements = Elements.AddPoint(p);
                if (elements.Count != 0)
                {
                    for (int i = 0; i < elements.Count; i++) { canvas.Children.Add(elements[i]); }
                }
            }
            refreshCanvas();
        }

        private void refreshCanvas() // обновление канвы
        {
            canvas.Children.Clear();
            prepareCanvas();
            for (int i = 0; i < Elements.polygons.Count; i++) //перерисовка всех полигонов
            {
                canvas.Children.Add(Elements.polygons[i]);
            }
            for (int i = 0; i < Elements.currentPolygon.Count; i++) //перерисовка линий незаконченного полигона
            {
                canvas.Children.Add(Elements.currentPolygon.ElementAt(i).Value);
            }
            canvas.Children.Add(Elements.singlePoint);
        } 

        private void fmWPFMain_SizeChanged(object sender, SizeChangedEventArgs e) // изменение размеров окна 
        {
            refreshCanvas();
        }
        
        private void btnToolArrow_Click(object sender, RoutedEventArgs e) // выбор инструмента для перетаскивания точек
        {
            Tools.type = Tools.ToolType.arrow;
            btnToolPoly.IsChecked = false;
        }

        private void btnToolPoly_Click(object sender, RoutedEventArgs e) // выбор инструмента для рисования полигонов
        {
            Tools.type = Tools.ToolType.polygon;
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
            if (e.Key == Key.Enter)
            {
                if (Elements.currentPolygon.Count >= 2)
                {
                    Line line = Elements.getLine(Elements.points[Elements.lastPoint], Elements.points[Elements.firstPoint]);
                    Elements.lastPoint = -1;
                    Elements.currentPolygon.Add(Elements.points.Count, line);
                    Polygon polygon = Elements.AddPolygon(Elements.currentPolygon);
                    Elements.polygons.Add(polygon);
                    Elements.currentPolygon.Clear();
                    canvas.Children.Add(line);
                    canvas.Children.Add(polygon);
                    refreshCanvas();
                }
            }
            if ((e.Key == Key.Delete) || (e.Key == Key.Back))
            {
                if (Elements.currentPolygon.Count == 0)  // при недостроенном полигоне нельзя удалять точки
                {
                    if (indexMovePoint != -1)
                    {
                        Elements.RemovePoint(indexMovePoint);
                        refreshCanvas();
                    }
                }  // надо сообщение о невозможности удалить пока полигон строится
            }
        }

        private void canvas_PreviewMouseMove(object sender, MouseEventArgs e)  // обработка передвижения точек
        {
            Vector p = new Vector(Mouse.GetPosition(canvas));
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                if (indexMovePoint != -1)
                {
                    canvas.Children.Remove(currentPoint);
                }
                indexMovePoint = Elements.getIndexPoint(p);
                if (indexMovePoint != -1)
                {
                    currentPoint = Elements.getEllipse(Elements.points[indexMovePoint], 5);
                    canvas.Children.Add(currentPoint);
                    btnToolArrow.IsChecked = true;
                    btnToolArrow_Click(null, null);
                }
                else
                {
                    canvas.Children.Remove(currentPoint);
                    btnToolPoly.IsChecked = true;
                    btnToolPoly_Click(null, null);
                }

            }
            else
            {
                if (indexMovePoint != -1)
                {
                    // проверка от совпадения точек
                    for (int i = 0; i < Elements.points.Count; i++)
                    {
                        if ((p.x == Elements.points[i].x) && (p.y == Elements.points[i].y) && (i != indexMovePoint))
                        {
                            p.x = p.x + 2;
                        }
                    }

                    // передвижение отдельной точки
                    currentPoint.Margin = new Thickness(p.x - (currentPoint.StrokeThickness + 0.5), p.y - (currentPoint.StrokeThickness + 0.5), 0, 0);
                    Elements.singlePoint.Margin = new Thickness(p.x - (Elements.singlePoint.StrokeThickness + 0.5), p.y - (Elements.singlePoint.StrokeThickness + 0.5), 0, 0);
                    // передвижение точки недостроенного полигона
                    foreach (KeyValuePair<int, Line> line in Elements.currentPolygon)
                    {
                        if (line.Key == indexMovePoint)
                        {
                            line.Value.X2 = p.x;
                            line.Value.Y2 = p.y;
                        }
                        int tagPoint = (int)line.Value.Tag;
                        if (tagPoint == indexMovePoint)
                        {
                            line.Value.X1 = p.x;
                            line.Value.Y1 = p.y;
                        }
                    }

                    // передвижение точки полигона
                    for (int i = 0; i < Elements.polygons.Count; i++)
                    {
                        for (int j = 0; j < Elements.polygons[i].Points.Count; j++)
                        {
                            if ((Elements.polygons[i].Points[j].X == Elements.points[indexMovePoint].x) && (Elements.polygons[i].Points[j].Y == Elements.points[indexMovePoint].y))
                            {
                                Elements.polygons[i].Points[j] = p.getPoint();
                            }
                        }
                    }
                    Elements.points[indexMovePoint] = p;
                }
            }
        } 
    }
}
