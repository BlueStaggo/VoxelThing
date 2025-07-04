using VoxelThing.Client.Gui.Controls;

namespace VoxelThing.Client.Gui.Screens;

public class MultiplayerDisconnectionScreen : Screen
{
    public MultiplayerDisconnectionScreen(Game game, string reason) : base(game)
    {
        AddControl(new Label(this)
        {
            Text = "§cff5555Disconnected!",
            Position = (0, -60),
            AlignPosition = (0.5f, 0.5f),
            Font = Game.MainRenderer.Fonts.Outlined,
            Size = (2, 2)
        });
        
        AddControl(new Label(this)
        {
            Text = reason,
            Position = (0, -20),
            AlignPosition = (0.5f, 0.5f)
        });
        
        var exitButton = AddControl(new Label(this)
        {
            Text = "Exit",
            Position = (0, 10),
            Size = (100, 20),
            AlignPosition = (0.5f, 0.5f),
            HasBackground = true
        });
        exitButton.OnClick += (_, _) => Game.CurrentScreen = null;
    }
}