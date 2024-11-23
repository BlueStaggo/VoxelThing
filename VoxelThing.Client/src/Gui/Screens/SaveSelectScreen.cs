using OpenTK.Graphics.OpenGL;
using VoxelThing.Client.Gui.Controls;
using VoxelThing.Client.Rendering;
using VoxelThing.Game.Worlds.Storage;

namespace VoxelThing.Client.Gui.Screens;

public class SaveSelectScreen : Screen
{
    private readonly ScrollContainer worldContainer;
    
    public SaveSelectScreen(Game game) : base(game)
    {
        var backButton = AddControl(new Label(this)
        {
            Text = "Back",
            Position = (-105, -25),
            Size = (100, 20),
            AlignPosition = (0.5f, 1.0f),
            HasBackground = true
        });
        backButton.OnClick += (_, _) => Game.CurrentScreen = Parent;
        
        var newWorldButton = AddControl(new Label(this)
        {
            Text = "New World",
            Position = (5, -25),
            Size = (100, 20),
            AlignPosition = (0.5f, 1.0f),
            HasBackground = true
        });
        newWorldButton.OnClick += (_, _) => Game.CurrentScreen = new CreateWorldScreen(Game);
        // newWorldButton.OnClick += (_, _) => Game.StartWorld(EmptySaveHandler.Instance);

        worldContainer = AddControl(new ScrollContainer(this)
        {
            Position = (0, 30),
            Size = (0, -60),
            AlignSize = (1.0f, 1.0f)
        });

        Refresh();
    }

    public void Refresh()
    {
        worldContainer.ClearControls();

        if (!Directory.Exists(Game.WorldDirectory))
            Directory.CreateDirectory(Game.WorldDirectory);

        IEnumerable<WorldPanel> worldPanels =
            Directory.EnumerateDirectories(Game.WorldDirectory)
                .Where(Directory.Exists)
                .Select(path => new FolderSaveHandler(path))
                .Select(saveHandler => new WorldPanel(this, saveHandler)
                {
                    Position = (-75, 0),
                    Size = (150, 20),
                    AlignPosition = (0.5f, 0.0f)
                }.AddControls());

        foreach (WorldPanel worldPanel in worldPanels)
            worldContainer.AddControl(worldPanel);
    }

    public override void Draw()
    {
        MainRenderer renderer = Game.MainRenderer;

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
        
        renderer.Fonts.Outlined.Print(
            "SELECT WORLD",
            renderer.ScreenDimensions.IntWidth / 2.0f, 10.0f,
            align: 0.5f
        );
        
        base.Draw();
    }

    protected override void OnMouseScrolled(float scroll)
    {
        base.OnMouseScrolled(scroll);
        worldContainer.Scroll(scroll * -10.0f);
    }
}