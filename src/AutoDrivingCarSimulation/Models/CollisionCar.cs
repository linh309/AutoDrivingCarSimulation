namespace AutoDrivingCarSimulation.Models;

public class CollisionCar
{
    public int StepIndex { get; set; }
    public Position Position { get; set; }
    public Car Car { get; set; }
    public Car ColliedCar { get; set; }
}
