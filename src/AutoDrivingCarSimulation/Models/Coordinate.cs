namespace AutoDrivingCarSimulation.Models;

public class Coordinate
{
    public Position Position { get; set; }
    public bool IsAvailable { get; set; } = true;
    public Car OccupiedCar { get; set; }
}

public record Position(int X, int Y);