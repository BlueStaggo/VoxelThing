using VoxelThing.Client.Gui.Controls;
using VoxelThing.Client.Rendering;
using VoxelThing.Game.Networking;
using VoxelThing.Game.Worlds.Storage;

namespace VoxelThing.Client.Gui.Screens;

public class MultiplayerConnectionScreen : Screen
{
    private readonly TextBox nameBox;
    private Task<bool>? connectionTask;
    private bool sentConnectionRequest;
    
    public MultiplayerConnectionScreen(Game game) : base(game)
    {
        AddControl(new Label(this)
        {
            Text = "Enter IP",
            Position = (0, -40),
            AlignPosition = (0.5f, 0.5f)
        });
        
        var ipBox = AddControl(new TextBox(this)
        {
            Text = "127.0.0.1",
            Position = (-100, -30),
            Size = (200, 20),
            AlignPosition = (0.5f, 0.5f)
        });
        
        nameBox = AddControl(new TextBox(this)
        {
            Text = "BlueStaggo",
            Position = (-100, -10),
            Size = (200, 20),
            AlignPosition = (0.5f, 0.5f)
        });
        
        var backButton = AddControl(new Label(this)
        {
            Text = "Back",
            Position = (-105, 20),
            Size = (100, 20),
            AlignPosition = (0.5f, 0.5f),
            HasBackground = true
        });
        backButton.OnClick += (_, _) => Game.CurrentScreen = Parent;
        
        var playButton = AddControl(new Label(this)
        {
            Text = "Play",
            Position = (5, 20),
            Size = (100, 20),
            AlignPosition = (0.5f, 0.5f),
            HasBackground = true
        });
        playButton.OnClick += (_, _) =>
        {
            if (connectionTask is null || connectionTask.IsCompleted)
                connectionTask = Game.ConnectToServer(ipBox.Text);
        };
    }

    public override void Tick()
    {
        if ((connectionTask?.IsCompletedSuccessfully ?? false) && Game.PacketHandler is not null)
        {
            if (!sentConnectionRequest)
            {
                sentConnectionRequest = true;
                Game.PacketHandler.Server.SendPacket(new CRequestConnection(PacketManager.ProtocolVersion, nameBox.Text));
            }
        }
        else
        {
            sentConnectionRequest = false;
        }
    }

    public override void Draw()
    {
        base.Draw();
        if (connectionTask is not null)
        {
            string displayString =
                connectionTask.IsCompletedSuccessfully ? "§c00ff00Success!"
                : connectionTask.IsFaulted ? "§cff0000Failure!"
                : "§cffff00Waiting...";
            
            MainRenderer renderer = Game.MainRenderer;
            ScreenDimensions dimensions = renderer.ScreenDimensions;
            renderer.Fonts.Outlined.Print(
                displayString,
                dimensions.IntWidth / 2, dimensions.IntHeight / 2 + 50,
                align: 0.5f
            );
        }
    }
}