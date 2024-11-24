namespace VoxelThing.Game;

public static class SharedConstants
{
#if DEBUG
    public const bool Debug = true;
#else
    public const bool Debug = false;
#endif

    public const bool AllowProfiler = Debug;
}