using OpenTK.Graphics.OpenGL;
using VoxelThing.Client.Gui.Controls;
using VoxelThing.Client.Rendering;

namespace VoxelThing.Client.Gui.Screens;

public class PauseScreen : Screen
{
    public override bool PausesWorld => true;

    public PauseScreen(Game game) : base(game)
    {
        var backButton = AddControl(new Label(this)
        {
            Text = "Back to Game",
            Position = (-50, -20),
            Size = (100, 20),
            AlignPosition = (0.5f, 0.5f),
            HasBackground = true
        });
        backButton.OnClick += (_, _) => game.CurrentScreen = Parent;
        
        var settingsButton = AddControl(new Label(this)
        {
            Text = "Settings",
            Position = (-50, 10),
            Size = (100, 20),
            AlignPosition = (0.5f, 0.5f),
            HasBackground = true
        });
        settingsButton.OnClick += (_, _) => game.CurrentScreen = new SettingsScreen(Game);
        
        var exitButton = AddControl(new Label(this)
        {
            Text = "Exit World",
            Position = (-50, 35),
            Size = (100, 20),
            AlignPosition = (0.5f, 0.5f),
            HasBackground = true
        });
        exitButton.OnClick += (_, _) =>
        {
            game.ExitWorld();
            game.CurrentScreen = Parent;
        };
    }

    public override void Draw()
    {
        MainRenderer renderer = Game.MainRenderer;

        using (GlState state = new())
        {
            state.Enable(EnableCap.Blend);
            renderer.Draw2D.DrawQuad(new()
            {
                Size = renderer.ScreenDimensions.IntSize,
                Color = (0.0f, 0.0f, 0.0f, 0.5f)
            });
        }
        
        renderer.Fonts.Outlined.Print(
            "PAUSE",
            renderer.ScreenDimensions.IntWidth / 2.0f,
            renderer.ScreenDimensions.IntHeight / 4.0f - 16.0f,
            1.0f, 1.0f, 1.0f, 2.0f,
            align: 0.5f
        );
        
        base.Draw();
    }
}