using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelThing.Game.Entities;

namespace VoxelThing.Client;

public class ClientPlayerController(Game game) : IPlayerController
{
    public double MoveForward
        => game.CurrentScreen is not null ? 0.0
            : game.IsKeyDown(Keys.W) ? 1.0
            : game.IsKeyDown(Keys.S) ? -1.0
            : 0.0;
    
    public double MoveStrafe
        => game.CurrentScreen is not null ? 0.0
            : game.IsKeyDown(Keys.D) ? 1.0
            : game.IsKeyDown(Keys.A) ? -1.0
            : 0.0;

    public double MoveYaw
        => game.CurrentScreen is not null ? 0.0
            : (game.CursorState == CursorState.Grabbed
                  ? game.MouseDelta.X * game.Settings.MouseSensitivity : 0.0f)
              + (game.IsKeyDown(Keys.Right) ? 1.0
                  : game.IsKeyDown(Keys.Left) ? -1.0
                  : 0.0) * game.Settings.MouseSensitivity * 360.0 * game.Delta;
    
    public double MovePitch
        => game.CurrentScreen is not null ? 0.0
            : (game.CursorState == CursorState.Grabbed
                  ? game.MouseDelta.Y * game.Settings.MouseSensitivity : 0.0f)
              + (game.IsKeyDown(Keys.Up) ? 1.0
                  : game.IsKeyDown(Keys.Down) ? -1.0
                  : 0.0) * game.Settings.MouseSensitivity * 360.0 * game.Delta;

    public bool DoJump => game.CurrentScreen is null && game.IsKeyDown(Keys.Space);
    
    public bool DoCrouch => game.CurrentScreen is null && game.IsKeyDown(Keys.LeftShift);
}