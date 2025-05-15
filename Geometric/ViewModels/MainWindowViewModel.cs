using ReactiveUI;
using Geometric.Models;
using System.Collections.ObjectModel;

namespace Geometric.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<Shape> Shapes { get; } = new ObservableCollection<Shape>();

        public void AddShape(Shape shape)
        {
            Shapes.Add(shape);
        }

        public void ClearShapes()
        {
            Shapes.Clear();
        }
    }
}