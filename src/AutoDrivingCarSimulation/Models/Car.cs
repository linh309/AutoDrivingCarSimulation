namespace AutoDrivingCarSimulation.Models;

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
