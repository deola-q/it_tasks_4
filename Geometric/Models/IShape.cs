namespace Geometric.Models;

public interface IShape
{
    (double Left, double Top, double Right, double Bottom) BoundingBox { get; }
    double Area { get; }
}