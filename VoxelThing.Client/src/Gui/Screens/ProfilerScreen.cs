using System.Collections.Immutable;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelThing.Client.Rendering;
using VoxelThing.Game.Maths;
using VoxelThing.Game.Utils;

namespace VoxelThing.Client.Gui.Screens;

public class ProfilerScreen(Game game) : Screen(game)
{
    private const float Padding = 5;
    private const float BarWidth = 10;
    private const float MaxBarHeight = 100;
    private const float BarMargin = 5;
    private const float LegendWidth = 150;
    
    private static readonly ImmutableArray<int> EntryColorsRgb =
    [
        0xff55aa,
        0xaaff55,
        0x55aaff,
        0xff5555,
        0x55ff55,
        0x5555ff,
        0xffaa55,
        0x55ffaa,
        0xaa55ff,
        0x55ffff,
        0xff55ff,
        0xffff55,
    ];

    private static readonly ImmutableArray<Vector3> EntryColorsVector3
        = [..EntryColorsRgb.Select(rgb => new Vector3(
            ((rgb >> 16) & 255) / 255.0f,
            ((rgb >> 8) & 255) / 255.0f,
            (rgb & 255) / 255.0f
        ))];

    private static readonly int SelfColorRgb = 0xaaaaaa;
    private static readonly Vector3 SelfColorVector3 = Vector3.One;
    private static readonly int RemainderColorRgb = 0x7f7f7f;
    private static readonly Vector3 RemainderColorVector3 = Vector3.One * 2.0f / 3.0f;
    
    private readonly Stack<string> entryKeyStack = [];
    private string? nextEntry;
    private ulong profilerTicks;
    
    public override void Draw()
    {
        if (Game.Profiler.DoMeasurements)
            DrawGraph();
        UpdateInput();
    }

    private void DrawGraph()
    {
        profilerTicks++;

        ProfilerEntry parentEntry = Game.Profiler.Root.Entries["game"];
        foreach (string key in entryKeyStack.Reverse())
        {
            if (!parentEntry.Entries.TryGetValue(key, out ProfilerEntry? childEntry))
            {
                while (entryKeyStack.Peek() != key)
                    entryKeyStack.Pop();
                entryKeyStack.Pop();
                break;
            }
            parentEntry = childEntry;
        }

        MainRenderer renderer = Game.MainRenderer;
        ScreenDimensions dimensions = Game.MainRenderer.ScreenDimensions;
        bool clickMouse = Game.MouseButtonsJustPressed.Any(m => m is { IsPressed: true, Button: MouseButton.Button1 });
        
        Dictionary<string, ProfilerEntry> entries = parentEntry.Entries.ToDictionary();
        
        List<string> entryKeys = [..entries.Keys.Order()];

        if (entries.Count == 0)
            entryKeys.Add(".self");
        else
            entryKeys.Add(".remainder");
        
        double maxTime = Math.Max(parentEntry.TimeElapsed / profilerTicks, entries.Values.Sum(e => e.TimeElapsed / profilerTicks));

        const float panelHeight = Padding * 2 + MaxBarHeight;
        const float legendStartX = Padding;
        const float barsStartX = Padding * 2 + LegendWidth;
        float panelStartY = dimensions.Height - panelHeight;
        float paddedPanelStartY = panelStartY + Padding;
        float panelWidth = Padding * 3 + entryKeys.Count * (BarWidth + BarMargin) - BarMargin + LegendWidth;
        Font font = renderer.Fonts.Shadowed;

        using (GlState state = new())
        {
            state.Enable(EnableCap.Blend);
            
            renderer.Draw2D.DrawQuad(new()
            {
                Position = new Vector2(0.0f, panelStartY),
                Size = new Vector2(panelWidth, panelHeight),
                Color = (0.0f, 0.0f, 0.0f, 0.5f)
            });
        }
        
        font.Print(
            string.Join('.', entryKeyStack.Reverse()),
            legendStartX,
            panelStartY - font.LineHeight
        );

        for (int i = 0; i < entryKeys.Count; i++)
        {
            string key = entryKeys[i];
            double time = key switch
            {
                ".self" => parentEntry.TimeElapsed / profilerTicks,
                ".remainder" => maxTime - entries.Values.Sum(e => e.TimeElapsed / profilerTicks),
                _ => entries[key].TimeElapsed / profilerTicks
            };
            int colorRgb = key switch
            {
                ".self" => SelfColorRgb,
                ".remainder" => RemainderColorRgb,
                _ => EntryColorsRgb[i % 12]
            };
            Vector3 colorVector3 = key switch
            {
                ".self" => SelfColorVector3,
                ".remainder" => RemainderColorVector3,
                _ => EntryColorsVector3[i % 12]
            };
            
            font.Print(
                $"§c{colorRgb:x6}{key}: §cffffff{time:F2}ms",
                legendStartX,
                paddedPanelStartY + i * font.LineHeight
            );

            if (!key.StartsWith('.') && clickMouse && nextEntry is null)
            {
                Aabb textAabb = Aabb.FromExtents(
                    legendStartX,
                    (dimensions.Height - panelHeight + Padding + i * font.LineHeight),
                    -0.5,
                    LegendWidth,
                    font.LineHeight,
                    1.0
                );
                if (textAabb.Contains(Game.ScaledMousePosition.X, Game.ScaledMousePosition.Y, 0.0))
                    nextEntry = key;
            }

            float barHeight = MaxBarHeight * (float)(time / maxTime);
            renderer.Draw2D.DrawQuad(new()
            {
                Position = ((barsStartX + i * (BarWidth + BarMargin)), paddedPanelStartY),
                Size = new Vector2(BarWidth, MaxBarHeight),
                ColorRgb = colorVector3 / 3.0f
            });
            renderer.Draw2D.DrawQuad(new()
            {
                Position = ((barsStartX + i * (BarWidth + BarMargin)), paddedPanelStartY + MaxBarHeight - barHeight),
                Size = new Vector2(BarWidth, barHeight),
                ColorRgb = colorVector3
            });
        }
        
        font.Print(
            $"back",
            legendStartX,
            dimensions.Height - (Padding + font.LineHeight)
        );

        if (clickMouse && nextEntry is null)
        {
            Aabb backTextAabb = Aabb.FromExtents(
                legendStartX,
                (dimensions.Height - Padding - font.LineHeight),
                -0.5,
                LegendWidth,
                font.LineHeight,
                1.0
            );
            if (backTextAabb.Contains(Game.ScaledMousePosition.X, Game.ScaledMousePosition.Y, 0.0))
                nextEntry = "__back__";
        }
    }

    private void UpdateInput()
    {
        if (nextEntry is not null)
        {
            if (nextEntry == "__back__")
                entryKeyStack.TryPop(out string _);
            else
                entryKeyStack.Push(nextEntry);
        }
        nextEntry = null;
        
        if (Game.KeysJustPressed.Any(e => e.Key == Keys.F4))
        {
            Game.Profiler.DoMeasurements = !Game.Profiler.DoMeasurements;
            if (!Game.Profiler.DoMeasurements)
            {
                Game.Profiler.Clear();
                profilerTicks = 0;
            }
        }
    }
}