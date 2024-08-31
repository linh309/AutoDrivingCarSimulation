namespace AutoDrivingCarSimulation;

public class Car
{
    public string Name { get; set; }

    public Car(string name)
    {
        Name = name;
    }

    public Position InitialPosition { get; set; }
    public Position CurrentPosition { get; set; }
    public EnumDirection CurrentDirection { get; set; }

    private EnumDirection _initialDirection { get; set; }
    private List<EnumCommandType> _commands { get; set; }
    private string _originalCommandText { get; set; }

    private int _totalSteps { get; set; } = 0;
    private int _currentStep = 0;
    private bool _isStop = false;
    private bool _isCollied = false;

    public void Initialize(Position position, EnumDirection direction, string originalCommandText)
    {
        InitialPosition = position;
        CurrentPosition = position;

        CurrentDirection = direction;
        _initialDirection = direction;

        _originalCommandText = originalCommandText;
        _commands = CreateCommand();
        _totalSteps = _commands.Count;
    }

    public void Move(Map map, int stepIndex)
    {
        _currentStep = stepIndex;

        if (IsMoveable() is false) return;

        if (IsRunOfOutCommands(stepIndex))
        {
            StopMove();
            return;
        }

        var nextPosition = NextPosition();
        var nextDirection = NextDirection();

        if (map.CheckPositionCrossBorders(nextPosition))
        {
            StopMove();
            return;
        }

        CurrentPosition = nextPosition;
        CurrentDirection = nextDirection;

        if (CurrentCommand == EnumCommandType.Forward)
        {
            map.MarkOccupiedCoordinate(this);
            var colliedCoordinates = map.CheckColliedCoordinates(this);
            if (colliedCoordinates?.Any() is true)
            {
                map.ProcessCarCollision(this, stepIndex, colliedCoordinates);

                MarkCollision();
                StopMove();
                return;
            }
        }
    }

    public void ShowSimulationResult()
    {
        if (_isCollied) return;

        Console.WriteLine($"- {Name}, ({CurrentPosition.X}, {CurrentPosition.Y}) {ToDirectionText()}");
    }

    public void ShowInitialinfo()
    {
        if (_isCollied) return;

        Console.WriteLine($"{Name}, ({CurrentPosition.X}, {CurrentPosition.Y}) {ToDirectionText()}, {_originalCommandText.ToUpper()}");
    }

    public bool IsMoveable() => _isStop is false;

    public void MarkCollision()
    {
        _isCollied = true;
    }

    public void StopMove()
    {
        _isStop = true;
    }

    private Position NextPosition()
    {
        if (CurrentCommand != EnumCommandType.Forward) return CurrentPosition;

        return CurrentDirection switch
        {
            EnumDirection.North => new Position(CurrentPosition.X, CurrentPosition.Y + 1),
            EnumDirection.South => new Position(CurrentPosition.X, CurrentPosition.Y - 1),
            EnumDirection.East => new Position(CurrentPosition.X + 1, CurrentPosition.Y),
            EnumDirection.West => new Position(CurrentPosition.X - 1, CurrentPosition.Y),
        };
    }

    private EnumDirection NextDirection()
    {
        if (CurrentCommand == EnumCommandType.TurnRight) return TurnRight();

        if (CurrentCommand == EnumCommandType.TurnLeft) return TurnLeft();

        return CurrentDirection;
    }

    private EnumDirection TurnRight()
    {
        return CurrentDirection switch
        {
            EnumDirection.North => EnumDirection.East,
            EnumDirection.East => EnumDirection.South,
            EnumDirection.South => EnumDirection.West,
            EnumDirection.West => EnumDirection.North,
        };
    }

    private EnumDirection TurnLeft()
    {
        return CurrentDirection switch
        {
            EnumDirection.North => EnumDirection.West,
            EnumDirection.West => EnumDirection.South,
            EnumDirection.South => EnumDirection.East,
            EnumDirection.East => EnumDirection.North,
        };
    }

    private string ToDirectionText()
    {
        return CurrentDirection switch
        {
            EnumDirection.North => "N",
            EnumDirection.West => "W",
            EnumDirection.South => "S",
            EnumDirection.East => "E",
        };
    }

    private bool IsRunOfOutCommands(int stepIndex) => stepIndex > _totalSteps;

    private List<EnumCommandType> CreateCommand()
    {
        var carCommands = new List<EnumCommandType>();
        foreach (var cmd in _originalCommandText)
        {
            var cmdType = cmd.ToString().ToUpper() switch
            {
                "F" => EnumCommandType.Forward,
                "R" => EnumCommandType.TurnRight,
                "L" => EnumCommandType.TurnLeft,
            };
            carCommands.Add(cmdType);
        }

        return carCommands;
    }

    private EnumCommandType CurrentCommand => _commands[_currentStep - 1];
}

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

public class Coordinate
{
    public Position Position { get; set; }
    public bool IsAvailable { get; set; } = true;
    public Car OccupiedCar { get; set; }
}

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

public class CollisionCar
{
    public int StepIndex { get; set; }
    public Position Position { get; set; }
    public Car Car { get; set; }
    public Car ColliedCar { get; set; }
}

public record Position(int X, int Y);

#region Enums

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

#endregion

internal class Program
{
    #region Validatation

    private static bool ValidateMapInput(string input)
    {
        var values = input.Split(" ").Where(x => string.IsNullOrWhiteSpace(x) is false).ToList();
        if (values.Count != 2) return false;

        foreach (var value in values)
        {
            if (int.TryParse(value, out var val) is false) return false;
        }

        return true;
    }

    private static bool ValidateOption(string input, params int[] validOptions)
    {
        if (int.TryParse(input, out var val) is false) return false;

        if (validOptions.Contains(val) is false) return false;

        return true;
    }

    private static bool ValidatePositionAndDirection(string input, int width, int height)
    {
        var values = input.Split(" ").Select(x => x.Trim()).Where(x => string.IsNullOrWhiteSpace(x) is false).ToList();
        if (values.Count != 3) return false;

        if (int.TryParse(values[0], out int xAxis) is false) return false;

        if (int.TryParse(values[1], out int yAxis) is false) return false;

        if (xAxis < 0 || xAxis >= width) return false;
        if (yAxis < 0 || yAxis >= height) return false;

        var direction = values[2].ToString().ToUpper() switch
        {
            "N" => EnumDirection.North,
            "S" => EnumDirection.South,
            "W" => EnumDirection.West,
            "E" => EnumDirection.East,
            _ => EnumDirection.None
        };

        if (direction == EnumDirection.None) return false;

        return true;
    }

    private static bool ValidateCommands(string commands)
    {
        var validCommands = new HashSet<string> { "F", "L", "R" };
        foreach (var command in commands)
        {
            if (validCommands.Contains(command.ToString()) is false) return false;
        }

        return true;
    }

    #endregion

    #region Get input data

    private static string GetMapInput()
    {
        Console.WriteLine("Please enter the width and height of the simulation field in x y format:");

        var mapInput = Console.ReadLine();
        var isMapInputValid = ValidateMapInput(mapInput);

        while (isMapInputValid is false)
        {
            Console.WriteLine("Width and Height of map are invalid. Please enter the correct width and height of the simulation field in x y format:\n");
            mapInput = Console.ReadLine();
            isMapInputValid = ValidateMapInput(mapInput);
        }

        return mapInput;
    }

    private static string GetCarName()
    {
        Console.WriteLine("Please enter the name of the car:");
        var carName = Console.ReadLine(); //Validate car name is exsited

        return carName;
    }

    private static (Position Position, EnumDirection Direction) GetPositionAndDirection(string carName, int width, int height)
    {
        Console.WriteLine($"Please enter initial position of car {carName} in x y Direction format:");

        var input = Console.ReadLine();
        var isInputValid = ValidatePositionAndDirection(input, width, height);

        while (isInputValid is false)
        {
            Console.WriteLine("\nYour position or direction is invalid. Please enter valid position of car {carName} in x y Direction format:");
            input = Console.ReadLine();
            isInputValid = ValidatePositionAndDirection(input, width, height);
        }

        var positionAndDirection = input.Split(" ");
        var positionX = int.Parse(positionAndDirection[0].ToString());
        var positionY = int.Parse(positionAndDirection[1].ToString());
        var direction = positionAndDirection[2].ToString().ToUpper() switch
        {
            "N" => EnumDirection.North,
            "S" => EnumDirection.South,
            "W" => EnumDirection.West,
            "E" => EnumDirection.East
        };

        return (new Position(positionX, positionY), direction);
    }

    private static string GetCommands(string carName)
    {
        Console.WriteLine($"Please enter the commands for car {carName}:");

        var commands = Console.ReadLine();
        var isCommandValid = ValidateCommands(commands);

        while (isCommandValid is false)
        {
            Console.WriteLine($"\nYour commands are invalid. Please enter valid commands for {carName}:");
            commands = Console.ReadLine();
            isCommandValid = ValidateCommands(commands);
        }

        return commands;
    }

    private static EnumActionOption GetActionOption()
    {
        var actionOption = Console.ReadLine();
        var isOptionValid = ValidateOption(actionOption, (int)EnumActionOption.AddCar, (int)EnumActionOption.RunSimulation);

        while (isOptionValid is false)
        {
            Console.WriteLine("\nYour option is invalid. Please choose from the following options:");
            Console.WriteLine("[1] Add a car to field");
            Console.WriteLine("[2] Run simulation");
            actionOption = Console.ReadLine();
            isOptionValid = ValidateOption(actionOption, (int)EnumActionOption.AddCar, (int)EnumActionOption.RunSimulation);
        }

        return (EnumActionOption)int.Parse(actionOption);
    }

    private static EnumNextOption GetNextOption()
    {
        Console.WriteLine("\n\nPlease choose from the following options:");
        Console.WriteLine("[1] Start over");
        Console.WriteLine("[2] Exit");

        var actionOption = Console.ReadLine();
        var isOptionValid = ValidateOption(actionOption, (int)EnumNextOption.StartOver, (int)EnumNextOption.Exit);

        while (isOptionValid is false)
        {
            Console.WriteLine("\nYour option is invalid. Please choose from the following options:");
            Console.WriteLine("[1] Start over");
            Console.WriteLine("[2] Exit");
            actionOption = Console.ReadLine();
            isOptionValid = ValidateOption(actionOption, (int)EnumActionOption.AddCar, (int)EnumActionOption.RunSimulation);
        }

        return (EnumNextOption)int.Parse(actionOption);
    }

    #endregion

    private static (int Width, int Heigth) ExtracWithAndHeight(string input)
    {
        var values = input.Split(" ").Where(x => string.IsNullOrWhiteSpace(x) is false).ToList();

        return (int.Parse(values[0]), int.Parse(values[1]));
    }

    private static void ShowSelectionOption()
    {
        Console.WriteLine("Please choose from the following options:");
        Console.WriteLine("[1] Add a car to field");
        Console.WriteLine("[2] Run simulation");
    }

    private static void ShowAddCarError(string carName, EnumAddCarError error)
    {
        if (error == EnumAddCarError.CarNameExisted)
        {
            Console.WriteLine($"The car name {carName} is already existed.\n");
        }

        if (error == EnumAddCarError.CarIsSamePosition)
        {
            Console.WriteLine($"The initial car position is already occupied.\n");
        }

        if (error == EnumAddCarError.CarsOverload)
        {
            Console.WriteLine($"The number of cars in the list is overloaded.\n");
        }
    }

    static void Main(string[] args)
    {
        var isContinue = true;

        while (isContinue)
        {
            Console.WriteLine("Welcome to Auto Driving Car Simulation!\n");

            var mapInput = GetMapInput();
            (int width, int height) = ExtracWithAndHeight(mapInput);

            var simulator = new Simulator();
            simulator.InitializeMap(width, height);

            Console.WriteLine($"You have created a field of {width} x {height}\n");
            ShowSelectionOption();

            var selectedOption = GetActionOption();

            if (selectedOption == EnumActionOption.RunSimulation)
            {
                var numberOfCars = simulator.NumberOfCars();
                if (numberOfCars <= 0)
                {
                    Console.WriteLine("\nThe list of cars are empty. Please enter the car\n");
                    selectedOption = EnumActionOption.AddCar;
                }
            }

            while (selectedOption != EnumActionOption.RunSimulation)
            {
                var isAddCarOk = false;
                while (isAddCarOk is false)
                {
                    var carName = GetCarName();
                    var positionAndDirection = GetPositionAndDirection(carName, width, height);
                    var commands = GetCommands(carName);

                    var newCar = new Car(carName);
                    newCar.Initialize(positionAndDirection.Position, positionAndDirection.Direction, commands);

                    var addCarResult = simulator.TryAddCar(newCar);
                    isAddCarOk = addCarResult == EnumAddCarError.Success;

                    if (isAddCarOk is false)
                    {
                        ShowAddCarError(carName, addCarResult);
                        Console.WriteLine("\nPlease enter valid car information\n");
                    }
                }

                simulator.ShowCurrentListCars();
                ShowSelectionOption();

                selectedOption = GetActionOption();
            }

            simulator.Simulate();
            simulator.ShowResult();

            var nextAction = GetNextOption();

            if (nextAction == EnumNextOption.Exit)
            {
                isContinue = false;
            }

            if (nextAction == EnumNextOption.StartOver)
            {
                Console.Clear();
            }
        }

        Console.WriteLine("\bEND!");
    }
}
