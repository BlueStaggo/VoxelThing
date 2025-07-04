using PDS;
using VoxelThing.Client.Gui.Screens;
using VoxelThing.Game.Worlds.Storage;

namespace VoxelThing.Client.Gui.Controls;

public class WorldPanel : Container
{
    public readonly ISaveHandler SaveHandler;
    public readonly string WorldName;

    public WorldPanel(Screen screen, ISaveHandler saveHandler) : base(screen)
    {
        SaveHandler = saveHandler;

        CompoundItem? data = saveHandler.LoadData("world");
        WorldName = data?["Name"]?.TryStringValue ?? "???";
    }

    public WorldPanel AddControls()
    {
        if (Controls.Count > 0)
            return this;
        
        var label = AddControl(new Label(Screen)
        {
            Text = WorldName,
            Size = (-20.0f, 0.0f),
            AlignSize = (1.0f, 1.0f),
            HasBackground = true
        });
        label.OnClick += (_, _) => Screen.Game.StartWorld(SaveHandler, false);

        var deleteButton = AddControl(new IconButton(Screen)
        {
            Icon = Icons.Delete,
            Position = (-20.0f, 0.0f),
            Size = (20.0f, 0.0f),
            AlignSize = (0.0f, 1.0f),
            AlignPosition = (1.0f, 0.0f)
        });
        deleteButton.OnClick += (_, _) => Screen.Game.CurrentScreen
            = new DeleteWorldScreen(Screen.Game, SaveHandler, WorldName);

        return this;
    }
}