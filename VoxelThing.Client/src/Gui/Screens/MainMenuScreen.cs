using System.Collections.ObjectModel;
using OpenTK.Windowing.Common;
using VoxelThing.Client.Gui.Controls;
using VoxelThing.Client.Rendering;
using VoxelThing.Client.Rendering.Textures;
using VoxelThing.Game.Maths;

namespace VoxelThing.Client.Gui.Screens;

public class MainMenuScreen : Screen
{
    private static readonly ReadOnlyCollection<string> Splashes;
    private static readonly Random64 Random = new();
    
    private readonly string splash;

    static MainMenuScreen()
    {
        try
        {
            Splashes = new(File.ReadAllLines(Path.Combine(Game.AssetsDirectory, "splashes.txt")));
        }
        catch (Exception)
        {
            Console.Error.WriteLine("Splashes not found!");
            Splashes = new(["No splashes?"]);
        }
    }
    
    public MainMenuScreen(Game game) : base(game)
    {
        splash = Splashes[Random.Next(Splashes.Count)];

        var playButton = AddControl(new Label(this)
        {
            Text = "Singleplayer",
            Position = (-75, -15),
            Size = (150, 20),
            AlignPosition = (0.5f, 0.5f),
            HasBackground = true
        });
        playButton.OnClick += (_, _) => Game.CurrentScreen = new SaveSelectScreen(Game);
        
        var multiplayerButton = AddControl(new Label(this)
        {
            Text = "Multiplayer",
            Position = (-75, 10),
            Size = (150, 20),
            AlignPosition = (0.5f, 0.5f),
            HasBackground = true
        });
        multiplayerButton.OnClick += (_, _) => Game.CurrentScreen = new MultiplayerConnectionScreen(Game);
        
        var settingsButton = AddControl(new Label(this)
        {
            Text = "Settings",
            Position = (-75, 35),
            Size = (150, 20),
            AlignPosition = (0.5f, 0.5f),
            HasBackground = true
        });
        settingsButton.OnClick += (_, _) => Game.CurrentScreen = new SettingsScreen(Game);
        
        var quitButton = AddControl(new Label(this)
        {
            Text = "Exit Game",
            Position = (-75, 60),
            Size = (150, 20),
            AlignPosition = (0.5f, 0.5f),
            HasBackground = true
        });
        quitButton.OnClick += (_, _) => Game.Close();
    }

    protected override void OnKeyPressed(KeyboardKeyEventArgs args) { }

    public override void Draw()
    {
        MainRenderer renderer = Game.MainRenderer;
        
        renderer.Draw2D.DrawQuad(new()
        {
            Size = renderer.ScreenDimensions.IntSize,
            Texture = renderer.Textures.Get("gui/titlebg.png", TextureFlags.Bilinear)
        });
        
        float hover = (float)Math.Sin(Game.TimeElapsed * Math.PI);
        float hoverAlt = (float)Math.Sin((Game.TimeElapsed + 0.25f) * Math.PI);
        renderer.Fonts.Outlined.Print(
            "VOXEL THING",
            renderer.ScreenDimensions.IntWidth / 2.0f,
            35.0f + hover * 2.0f,
            0.0f, 1.0f, 1.0f, 4.0f,
            align: 0.5f
        );
        
        renderer.Fonts.Shadowed.Print(
            "§cffff55" + splash,
            renderer.ScreenDimensions.IntWidth / 2.0f,
            80.0f + hoverAlt * 2.0f,
            align: 0.5f
        );
        
        renderer.Fonts.Normal.Print(
            $"Github: §caaffbfhttps://github.com/BlueStaggo/VoxelThing",
            4,
            renderer.ScreenDimensions.IntHeight - 24
        );

        renderer.Fonts.Normal.Print(
            $"Voxel Thing §cffffaa{Game.Version}§cffffff, created by §caaffffBlueStaggo§cffffff.",
            4,
            renderer.ScreenDimensions.IntHeight - 12
        );

        base.Draw();
    }
}