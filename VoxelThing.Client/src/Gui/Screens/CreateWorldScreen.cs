using OpenTK.Graphics.OpenGL;
using VoxelThing.Client.Gui.Controls;
using VoxelThing.Client.Rendering;
using VoxelThing.Game.Worlds;

namespace VoxelThing.Client.Gui.Screens;

public class CreateWorldScreen : Screen
{
    private readonly Control createButton;
    private readonly Control cancelButton;
    private readonly TextBox nameBox;
    private readonly TextBox seedBox;
    
    public CreateWorldScreen(Game game) : base(game)
    {
        createButton = AddControl(new Label(this)
        {
            Text = "Create",
            Position = (5, -25),
            Size = (100, 20),
            AlignPosition = (0.5f, 1.0f),
            HasBackground = true
        });
        createButton.OnClick += CreateWorld;
        
        cancelButton = AddControl(new Label(this)
        {
            Text = "Cancel",
            Position = (-105, -25),
            Size = (100, 20),
            AlignPosition = (0.5f, 1.0f),
            HasBackground = true
        });
        cancelButton.OnClick += (_, _) => Game.CurrentScreen = Parent;
        
        nameBox = AddControl(new TextBox(this)
        {
            Text = "World",
            Position = (-75, 50),
            Size = (150, 20),
            AlignPosition = (0.5f, 0.0f)
        });
        
        seedBox = AddControl(new TextBox(this)
        {
            Position = (-75, 80),
            Size = (150, 20),
            AlignPosition = (0.5f, 0.0f)
        });
    }

    private void CreateWorld(object? sender, PositionalMouseButtonEventArgs e)
    {
        var worldInfo = new WorldInfo()
        {
            Name = nameBox.Text
        };
        
        if (seedBox.Text.Length > 0)
            if (ulong.TryParse(seedBox.Text, out ulong numericSeed))
                worldInfo.Seed = numericSeed;
            else
                worldInfo.Seed = (ulong)seedBox.Text.GetHashCode();

        Game.StartWorld(Guid.NewGuid().ToString(), worldInfo);
    }

    public override void Draw()
    {
        MainRenderer renderer = Game.MainRenderer;
        Font font = renderer.Fonts.Outlined;

        using (GlState state = new())
        {
            state.Enable(EnableCap.Blend);
            
            renderer.Draw2D.DrawQuad(new()
            {
                Position = (0, 30),
                Size = renderer.ScreenDimensions.IntSize + (0, -60),
                Color = (0.0f, 0.0f, 0.0f, 0.5f)
            });
        }
        
        font.Print(
            "CREATE WORLD",
            renderer.ScreenDimensions.IntWidth / 2.0f, 10.0f,
            align: 0.5f
        );
        
        base.Draw();

        font.Print(
            "Name",
            nameBox.ScaledPosition.X - 5.0f,
            nameBox.ScaledPosition.Y + (nameBox.ScaledSize.Y - font.LineHeight) / 2.0f,
            align: 1.0f
        );
        
        font.Print(
            "Seed",
            seedBox.ScaledPosition.X - 5.0f,
            seedBox.ScaledPosition.Y + (seedBox.ScaledSize.Y - font.LineHeight) / 2.0f,
            align: 1.0f
        );
    }
}