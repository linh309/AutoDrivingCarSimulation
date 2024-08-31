namespace AutoDrivingCarSimulation.Tests;

public class CarTest
{
    public static IEnumerable<object[]> TestCarForward
    {
        get
        {
            yield return new object[] { 1, 2, EnumDirection.North, 1, 3 }; //Move forward 1 point in North direction
            yield return new object[] { 1, 2, EnumDirection.South, 1, 1 }; //Move forward 1 point in South direction

            yield return new object[] { 1, 2, EnumDirection.West, 0, 2 };//Move forward 1 point in West direction
            yield return new object[] { 1, 2, EnumDirection.East, 2, 2 };//Move forward 1 point in East direction
        }
    }

    public static IEnumerable<object[]> TestCarTurnRightDirection
    {
        get
        {
            yield return new object[] { 1, 2, EnumDirection.North, EnumDirection.East }; //Rigth direction
            yield return new object[] { 1, 2, EnumDirection.East, EnumDirection.South }; //Rigth direction
            yield return new object[] { 1, 2, EnumDirection.South, EnumDirection.West }; //Rigth direction
            yield return new object[] { 1, 2, EnumDirection.West, EnumDirection.North }; //Rigth direction
        }
    }

    public static IEnumerable<object[]> TestCarTurnLeftDirection
    {
        get
        {
            yield return new object[] { 1, 2, EnumDirection.North, EnumDirection.West }; //Left direction
            yield return new object[] { 1, 2, EnumDirection.West, EnumDirection.South }; //Left direction
            yield return new object[] { 1, 2, EnumDirection.South, EnumDirection.East }; //Left direction
            yield return new object[] { 1, 2, EnumDirection.East, EnumDirection.North }; //Left direction
        }
    }

    public static IEnumerable<object[]> TestCarMoveData
    {
        get
        {
            yield return new object[] { 1, 2, EnumDirection.North, "FFRFFFRRLF", 4, 3, EnumDirection.South };
            yield return new object[] { 7, 8, EnumDirection.West, "FFLFFFFFFF", 5, 1, EnumDirection.South };
            yield return new object[] { 2, 5, EnumDirection.East, "FFFRFFLF", 6, 3, EnumDirection.East };
        }
    }

    [Theory]
    [MemberData(nameof(TestCarForward))]
    public void Car_Forward(int initialXAxis, int initialYAxis, EnumDirection direction, int expectedXAxis, int expectedYAxis)
    {
        //Arrange
        var map = new Map(10, 10);
        var car = new Car("A");
        car.Initialize(new Position(initialXAxis, initialYAxis), direction, "F");

        //Action
        car.Move(map, 1);

        Assert.Multiple(
            () => Assert.Equal(car.CurrentPosition, new Position(expectedXAxis, expectedYAxis)),
            () => Assert.Equal(car.CurrentDirection, direction)
        );
    }

    [Theory]
    [MemberData(nameof(TestCarTurnRightDirection))]
    public void Car_TurnRight(int initialXAxis, int initialYAxis, EnumDirection initDirection, EnumDirection expectedDirection)
    {
        //Arrange
        var map = new Map(10, 10);
        var car = new Car("A");
        car.Initialize(new Position(initialXAxis, initialYAxis), initDirection, "R");

        //Action
        car.Move(map, 1);

        //Assert
        Assert.Multiple(
            () => Assert.Equal(car.CurrentPosition, new Position(initialXAxis, initialYAxis)),
            () => Assert.Equal(car.CurrentDirection, expectedDirection)
        );
    }

    [Theory]
    [MemberData(nameof(TestCarTurnLeftDirection))]
    public void Car_TurnLeft(int initialXAxis, int initialYAxis, EnumDirection initDirection, EnumDirection expectedDirection)
    {
        //Arrange
        var map = new Map(10, 10);
        var car = new Car("A");
        car.Initialize(new Position(initialXAxis, initialYAxis), initDirection, "L");

        //Action
        car.Move(map, 1);

        Assert.Multiple(
            () => Assert.Equal(car.CurrentPosition, new Position(initialXAxis, initialYAxis)),
            () => Assert.Equal(car.CurrentDirection, expectedDirection)
        );
    }

    [Theory]
    [MemberData(nameof(TestCarMoveData))]
    public void Car_Move(int initialXAxis, int initialYAxis, EnumDirection initDirection, string commands, int expectedXAxis, int currentYAxis, EnumDirection expectedDirection)
    {
        //Arrange
        var map = new Map(10, 10);
        var car = new Car("A");
        car.Initialize(new Position(initialXAxis, initialYAxis), initDirection, commands);

        //Action
        for(int stepIndex = 1; stepIndex <= commands.Length; stepIndex++)
        {
            car.Move(map, stepIndex);
        }

        Assert.Multiple(
            () => Assert.Equal(car.CurrentPosition, new Position(expectedXAxis, currentYAxis)),
            () => Assert.Equal(car.CurrentDirection, expectedDirection)
        );
    }
}
