namespace AutoDrivingCarSimulation.Models;

public enum EnumDirection
{
    None = 0,
    North = 1,
    South = 2,
    West = 3,
    East = 4
}

public enum EnumCommandType
{
    TurnRight,
    TurnLeft,
    Forward
}

public enum EnumActionOption
{
    None = 0,
    AddCar = 1,
    RunSimulation = 2,
}

public enum EnumNextOption
{
    None = 0,
    StartOver = 1,
    Exit = 2,
}

public enum EnumAddCarError
{
    Success = 0,
    CarNameExisted = 1,
    CarsOverload = 2,
    CarIsSamePosition = 3,
}