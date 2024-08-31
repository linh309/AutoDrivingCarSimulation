namespace AutoDrivingCarSimulation.Models;

public class Map
{
    public int Width { get; set; } //X axis
    public int Height { get; set; } //Y axis

    private List<Coordinate> _coordinates { get; set; }
    private List<CollisionCar> _colliedCars { get; set; }

    public Map(int width, int height)
    {
        Width = width;
        Height = height;

        _coordinates = new List<Coordinate>(width * height);
        _colliedCars = new List<CollisionCar>();
    }

    public void MarkOccupiedCoordinate(Car car)
    {
        _coordinates.Add(new Coordinate { Position = car.CurrentPosition, IsAvailable = false, OccupiedCar = car });
    }

    public IEnumerable<Coordinate> CheckColliedCoordinates(Car car)
    {
        var position = car.CurrentPosition;
        var colliedCoordinates = new List<Coordinate>(0);

        var northCoordinate = _coordinates.Where(x => x.Position == new Position(position.X, position.Y + 1)).FirstOrDefault();
        var southCoordinate = _coordinates.Where(x => x.Position == new Position(position.X, position.Y - 1)).FirstOrDefault();
        var eastCoordinate = _coordinates.Where(x => x.Position == new Position(position.X + 1, position.Y)).FirstOrDefault();
        var westCoordinate = _coordinates.Where(x => x.Position == new Position(position.X - 1, position.Y)).FirstOrDefault();

        if (IsCoordinateCollied(northCoordinate, car)) colliedCoordinates.Add(northCoordinate);
        if (IsCoordinateCollied(southCoordinate, car)) colliedCoordinates.Add(southCoordinate);
        if (IsCoordinateCollied(eastCoordinate, car)) colliedCoordinates.Add(eastCoordinate);
        if (IsCoordinateCollied(westCoordinate, car)) colliedCoordinates.Add(westCoordinate);

        return colliedCoordinates;
    }

    public void ProcessCarCollision(Car car, int stepIndex, IEnumerable<Coordinate> colliedCoordinates)
    {
        foreach (var coordinate in colliedCoordinates)
        {
            coordinate.OccupiedCar.MarkCollision();
            coordinate.OccupiedCar.StopMove();

            _colliedCars.Add(new CollisionCar { Car = car, ColliedCar = coordinate.OccupiedCar, StepIndex = stepIndex, Position = car.CurrentPosition });
            _colliedCars.Add(new CollisionCar { Car = coordinate.OccupiedCar, ColliedCar = car, StepIndex = stepIndex, Position = car.CurrentPosition });
        }
    }

    public bool CheckPositionCrossBorders(Position position)
    {
        var cross_XAxis = position.X < 0 || position.X >= Width;
        var cross_YAxis = position.Y < 0 || position.Y >= Height;
        return cross_XAxis || cross_YAxis;
    }

    public void ShowResult()
    {
        if (_colliedCars?.Any() is false) return;

        foreach (var colliedCar in _colliedCars.OrderBy(x => x.Car.Name))
        {
            Console.WriteLine($"- {colliedCar.Car.Name}, collides with {colliedCar.ColliedCar.Name} at ({colliedCar.Position.X},{colliedCar.Position.Y}) at step {colliedCar.StepIndex}");
        }
    }

    public int CoordinateCapacity() => _coordinates.Capacity;

    private bool IsCoordinateCollied(Coordinate coordinate, Car car) => coordinate is not null && coordinate.IsAvailable is false && coordinate.OccupiedCar.Name != car.Name;
}