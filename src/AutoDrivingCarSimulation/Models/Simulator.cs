using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDrivingCarSimulation.Models;

public class Simulator
{
    private Map _map { get; set; }

    private List<Car> _cars { get; set; } = new List<Car>();

    public void InitializeMap(int width, int height)
    {
        _map = new Map(width, height);
    }

    public EnumAddCarError TryAddCar(Car car)
    {
        if (_cars.Count >= _map.CoordinateCapacity()) return EnumAddCarError.CarsOverload;

        if (_cars?.Where(x => x.Name.Equals(car.Name, StringComparison.OrdinalIgnoreCase))?.Any() is true) return EnumAddCarError.CarNameExisted;

        if (_cars?.Where(x => x.InitialPosition == car.InitialPosition)?.Any() is true) return EnumAddCarError.CarIsSamePosition;

        _cars.Add(car);

        return EnumAddCarError.Success;
    }

    public void Simulate()
    {
        int stepIndex = 0;
        while (IsAnyCarMoveable())
        {
            stepIndex += 1;
            foreach (var car in _cars)
            {
                car.Move(_map, stepIndex);
            }
        }
    }

    public void ShowResult()
    {
        Console.WriteLine("\n\nAfter simulation, the result is:");
        _map.ShowResult();
        foreach (var car in _cars)
        {
            car.ShowSimulationResult();
        }
    }

    public void ShowCurrentListCars()
    {
        Console.WriteLine("\nYour current list of cars are:");
        foreach (var car in _cars)
        {
            car.ShowInitialinfo();
        }
        Console.WriteLine();
    }

    public int NumberOfCars() => _cars?.Count ?? 0;

    private bool IsAnyCarMoveable()
    {
        return _cars.Where(x => x.IsMoveable()).Any();
    }
}
