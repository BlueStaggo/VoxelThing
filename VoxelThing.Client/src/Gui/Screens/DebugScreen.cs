using System.Diagnostics;
using System.Text;
using VoxelThing.Client.Assets;
using VoxelThing.Game.Blocks;

namespace VoxelThing.Client.Gui.Screens;

public class DebugScreen(Game game) : Screen(game)
{
    public override void Draw()
    {
        FontManager fonts = Game.MainRenderer.Fonts;
        ScreenDimensions screen = Game.MainRenderer.ScreenDimensions;

        fonts.Outlined.Print("§c00ffffVOXEL THING    §c00ff00" + Game.Version, 5, 5);
        
        long totalMb = GC.GetTotalMemory(false) / 1000000L;

        Debug.Assert(Game.World != null, "Game.World != null");
        string raycastText = Game.SelectionCast.GetDebugText(Game.World);

        string[] lines = {
            "FPS", Game.Fps + " (" + (int)(Game.Delta * 1000.0D) + "ms)",
            "Memory", totalMb + " MB",
            "GUI Scale", Convert.ToString((screen.Scale <= 0.0f ? "auto" : screen.Scale))!,
            "Position", Game.Player is not null
                ? FormatDouble(Game.Player.Position.Value.X)
                  + ", " + FormatDouble(Game.Player.Position.Value.Y)
                  + ", " + FormatDouble(Game.Player.Position.Value.Z)
                : "N/A",
            "Looking At", raycastText,
            "Scroll", FormatDouble(Game.MouseScroll.X) + ", " + FormatDouble(Game.MouseScroll.Y)
        };

        var debugBuilder = new StringBuilder();

        for (int i = 0; i < lines.Length / 2; i++) {
            string label = lines[i * 2];
            string value = lines[i * 2 + 1];

            if (debugBuilder.Length > 0) {
                debugBuilder.Append('\n');
            }

            debugBuilder.Append("§cffff7f");
            debugBuilder.Append(label);
            debugBuilder.Append(": §cffffff");
            debugBuilder.Append(value);
        }

        fonts.Shadowed.Print(debugBuilder.ToString(), 5, 15);
    }
    
    private static string FormatDouble(double d) => (Math.Floor(d * 100.0) / 100.0).ToString();
}