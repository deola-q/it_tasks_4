using Avalonia.Controls;
using Avalonia.Interactivity;
using Geometric.Models;
using Geometric.ViewModels;
using System;
using System.Linq;

namespace Geometric.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        // Обработка изменения выбора типа фигуры
        private void ShapeTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedIndex = ShapeTypeComboBox.SelectedIndex;

            // Показываем/скрываем соответствующие поля ввода
            PointInput.IsVisible = selectedIndex == 0;
            LineInput.IsVisible = selectedIndex == 1;
            EllipseInput.IsVisible = selectedIndex == 2;
            PolygonInput.IsVisible = selectedIndex == 3;
        }

        // Обработка нажатия на кнопку "Создать фигуру"
        private void CreateShape_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainWindowViewModel;
            if (viewModel == null) return;

            var selectedIndex = ShapeTypeComboBox.SelectedIndex;

                switch (selectedIndex)
                {
                    case 0: // Точка
                        if (double.TryParse(PointX.Text, out var pointX) && double.TryParse(PointY.Text, out var pointY))
                        {
                            viewModel.AddShape(new Point(pointX, pointY));
                        }
                        else
                        {
                            throw new ArgumentException("Некорректные координаты точки.");
                        }
                        break;

                    case 1: // Линия
                        if (double.TryParse(LineX1.Text, out var lineX1) &&
                            double.TryParse(LineY1.Text, out var lineY1) &&
                            double.TryParse(LineX2.Text, out var lineX2) &&
                            double.TryParse(LineY2.Text, out var lineY2))
                        {
                            viewModel.AddShape(new Line(lineX1, lineY1, lineX2, lineY2));
                        }
                        else
                        {
                            throw new ArgumentException("Некорректные координаты линии.");
                        }
                        break;

                    case 2: // Эллипс
                        if (double.TryParse(EllipseCenterX.Text, out var ellipseCenterX) &&
                            double.TryParse(EllipseCenterY.Text, out var ellipseCenterY) &&
                            double.TryParse(EllipseRadiusX.Text, out var ellipseRadiusX) &&
                            double.TryParse(EllipseRadiusY.Text, out var ellipseRadiusY))
                        {
                            viewModel.AddShape(new Ellipse(ellipseCenterX, ellipseCenterY, ellipseRadiusX, ellipseRadiusY));
                        }
                        else
                        {
                            throw new ArgumentException("Некорректные параметры эллипса.");
                        }
                        break;

                    case 3: // Полигон
                        if (!string.IsNullOrWhiteSpace(PolygonVerticesX.Text) && !string.IsNullOrWhiteSpace(PolygonVerticesY.Text))
                        {
                            var verticesX = PolygonVerticesX.Text.Split(',').Select(s => double.TryParse(s, out var x) ? x : throw new ArgumentException("Некорректные вершины X.")).ToArray();
                            var verticesY = PolygonVerticesY.Text.Split(',').Select(s => double.TryParse(s, out var y) ? y : throw new ArgumentException("Некорректные вершины Y.")).ToArray();
                            viewModel.AddShape(new Polygon(verticesX, verticesY));
                        }
                        else
                        {
                            throw new ArgumentException("Вершины полигона не могут быть пустыми.");
                        }
                        break;
                }
            }

        // Обработка нажатия на кнопку "Очистить список"
        private void ClearShapes_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainWindowViewModel;
            if (viewModel == null) return;

            viewModel.ClearShapes();
        }
    }
}