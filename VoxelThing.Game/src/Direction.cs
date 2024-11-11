namespace VoxelThing.Game;

public enum Direction
{
    North,
    South,
    West,
    East,
    Down,
    Up,
}

public static class Directions
{
    public static int GetX(this Direction direction)
        => direction switch
        {
            Direction.West => -1,
            Direction.East => 1,
            _ => 0
        };

    public static int GetY(this Direction direction)
        => direction switch
        {
            Direction.Down => -1,
            Direction.Up => 1,
            _ => 0
        };

    public static int GetZ(this Direction direction)
        => direction switch
        {
            Direction.North => -1,
            Direction.South => 1,
            _ => 0
        };

    public static Direction GetOpposite(this Direction direction)
        => direction - ((((int)direction & 1) << 1) - 1);
}