using AutoDrivingCarSimulation.Models;

namespace AutoDrivingCarSimulation;

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
        foreach (var command in commands.ToUpper())
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
