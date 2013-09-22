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
        private int indexMovePoint; // индекс выбраной точки
        private Ellipse currentPoint = new Ellipse();  // маркер выбраной точки

        public fmMain()
        {
            InitializeComponent();
            prepareCanvas();
            prepareToolPanel();
            lbHint.Content = "Добро пожаловать в программу рисования полигонов";
        }  // инициализация

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
            #region Добавление точки к полигону
            if ((e.LeftButton != MouseButtonState.Pressed) && (e.RightButton == MouseButtonState.Pressed))
            {
                if (indexMovePoint == -1)  // ненадо добавлять новую точку на существующую точку
                {

                }
            }
            #endregion
            #region Добавление точки к недостроееному полигону
            if ((e.LeftButton == MouseButtonState.Pressed) && (e.RightButton != MouseButtonState.Pressed))
            {
                if (Tools.type == Tools.ToolType.polygon)
                {
                    Elements.currentLine = Elements.decLenght(Elements.currentLine, 0.1);
                    List<UIElement> elements = Elements.addPoint(new Vector(Elements.currentLine.X2, Elements.currentLine.Y2));
                    if (elements.Count != 0)  // добавление новых элементов после установки точки
                    {
                        for (int i = 0; i < elements.Count; i++) { canvas.Children.Add(elements[i]); }
                    }
                }
            }
            #endregion
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
            canvas.Children.Add(Elements.currentLine);
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
            if (e.Key == Key.Enter) // строим полигон
            {
                if (Elements.currentPolygon.Count >= 2)
                {
                    Polygon polygon = Elements.addPolygon(Elements.currentPolygon);
                    canvas.Children.Add(polygon);
                    refreshCanvas();
                }
            }
            if ((e.Key == Key.Delete) || (e.Key == Key.Back))  // удаляем точки
            {
                if (Elements.currentPolygon.Count == 0)  // при недостроенном полигоне нельзя удалять точки
                {
                    if (indexMovePoint != -1)
                    {
                        Elements.removePoint(indexMovePoint);
                        refreshCanvas();
                    }
                }
            }
        }

        private void canvas_PreviewMouseMove(object sender, MouseEventArgs e)  // обработка команд от мыши
        {
            Vector p = new Vector(Mouse.GetPosition(canvas));
            Elements.currentLine.X2 = p.x;  // без этих двух строк курсорная линия моргает. 
            Elements.currentLine.Y2 = p.y;  // а почему непонятно :(   мне кажется логично было бы наоборот
            Vector p1 = Elements.checkCurrentPolygon(p, Elements.currentLine);  // проверка пересечения курсорной линии с недостроенным полигоном
            Elements.currentLine.X2 = p1.x;
            Elements.currentLine.Y2 = p1.y;
            #region Выбор точки
            if ((e.LeftButton != MouseButtonState.Pressed)&&(e.RightButton != MouseButtonState.Pressed))
            {
                if (Elements.firstPoint != -1)
                {
                    Elements.currentLine.StrokeThickness = 1;
                }
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
                    lbHint.Content = "Левой кнопкой мыши можно передвигать точку. Del или BackSpace удаляет точку если полигон построен и имеет более 3-х точек";
                }
                else
                {
                    canvas.Children.Remove(currentPoint);
                    btnToolPoly.IsChecked = true;
                    btnToolPoly_Click(null, null);
                    lbHint.Content = "Левой кнопкой мыши можно поставить точку. Правой кнопкой добавить точку к ближайшему полигону";
                }
            }
            #endregion
            #region Передвижение точки
            if ((e.LeftButton == MouseButtonState.Pressed) && (e.RightButton != MouseButtonState.Pressed))
            {
                Elements.currentLine.StrokeThickness = 0; // делаем курсорную линию невидимой что бы не мешалась
                if (indexMovePoint != -1)
                {
                    #region Проверка от совпадения точек а то без этого полигон может сломаться :(
                    foreach (KeyValuePair<int, Vector> point in Elements.points)
                    {
                        if ((p1.x == point.Value.x) && (p1.y == point.Value.y) && (point.Key != indexMovePoint))
                        {
                            p1.x = p1.x + 2;
                        }
                    }
                    #endregion
                    #region Передвигаем начальную точку курсорной линии
                    if (indexMovePoint == Elements.lastPoint) 
                    {
                        Elements.currentLine.X1 = p1.x;
                        Elements.currentLine.Y1 = p1.y;
                    }
                    #endregion
                    #region Двигаем отдельную точку (ее видно только когда полигон не содердит линий)
                    currentPoint.Margin = new Thickness(p1.x - (currentPoint.StrokeThickness + 0.5), p1.y - (currentPoint.StrokeThickness + 0.5), 0, 0);
                    Elements.singlePoint.Margin = new Thickness(p1.x - (Elements.singlePoint.StrokeThickness + 0.5), p1.y - (Elements.singlePoint.StrokeThickness + 0.5), 0, 0);
                    Elements.points[indexMovePoint] = p1; 
                    #endregion
                    #region Двигаем точку недостроенного полигона
                    List<int> moveLines = new List<int>();
                    List<int> notMoveLines = new List<int>();
                    foreach (KeyValuePair<int, Line> line in Elements.currentPolygon)
                    {
                        int tagPoint = (int)line.Value.Tag;
                        if ((line.Key == indexMovePoint) || (tagPoint == indexMovePoint))
                        {
                            moveLines.Add(line.Key);
                        }
                        else
                        {
                            notMoveLines.Add(line.Key);
                        }
                        if (line.Key == indexMovePoint)
                        {
                            line.Value.X2 = p.x;
                            line.Value.Y2 = p.y;
                        }
                        
                        if (tagPoint == indexMovePoint)
                        {
                            line.Value.X1 = p.x;
                            line.Value.Y1 = p.y;
                        }
                    }
                    if (Elements.currentPolygon.Count > 2)
                    {
                        for (int i = 0; i < moveLines.Count; i++)
                        {
                            for (int j = 0; j < notMoveLines.Count; j++)
                            {
                                int crossFlag = -2;
                                p1 = Elements.getCrossPoint(Elements.currentPolygon[moveLines[i]], Elements.currentPolygon[notMoveLines[j]], out crossFlag);
                                if (crossFlag == 2)
                                {
                                    int tagPoint = (int)Elements.currentPolygon[moveLines[i]].Tag;
                                    if (tagPoint == indexMovePoint)
                                    {
                                        Elements.currentPolygon[moveLines[i]].X1 = p1.x;
                                        Elements.currentPolygon[moveLines[i]].Y1 = p1.y;
                                    }
                                    else
                                    {
                                        Elements.currentPolygon[moveLines[i]].X2 = p1.x;
                                        Elements.currentPolygon[moveLines[i]].Y2 = p1.y;
                                    }
                                    Elements.points[indexMovePoint] = p1;
                                }
                            }
                        }
                    }
                    #endregion
                    #region Двигаем точку построенного полигона
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
                    #endregion
                    
                }
                refreshCanvas();
            }
            #endregion
        } 
    }
}
