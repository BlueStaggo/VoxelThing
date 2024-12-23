using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelThing.Client.Gui.Controls;
using VoxelThing.Game.Networking;

namespace VoxelThing.Client.Gui.Screens;

public class MultiplayerTestScreen : Screen
{
    private readonly List<string> messages = [];
    
    public MultiplayerTestScreen(Game game, string name) : base(game)
    {
        AddControl(new Label(this)
        {
            Text = $"You are {name}",
            Font = game.MainRenderer.Fonts.Outlined,
            Position = (2, 2),
            AlignText = (0.0f, 0.0f)
        });
        
        var messageBox = AddControl(new TextBox(this)
        {
            Position = (0, -20),
            Size = (-100, 20),
            AlignPosition = (0.0f, 1.0f),
            AlignSize = (1.0f, 0.0f)
        });
        messageBox.OnKeyPressed += (_, args) =>
        {
            if (args.Key != Keys.Enter)
                return;
            Game.Connection?.SendPacket(new CSendMessagePacket(messageBox.Text));
            messageBox.Text = string.Empty;
        };
        
        var sendButton = AddControl(new Label(this)
        {
            Text = "Send",
            Position = (-100, -20),
            Size = (100, 20),
            AlignPosition = (1.0f, 1.0f),
            HasBackground = true
        });
        sendButton.OnClick += (_, _) =>
        {
            Game.Connection?.SendPacket(new CSendMessagePacket(messageBox.Text));
            messageBox.Text = string.Empty;
        };
        
        var exitButton = AddControl(new IconButton(this)
        {
            Icon = Icons.Delete,
            Position = (-20, 0),
            Size = (20, 20),
            AlignPosition = (1.0f, 0.0f),
        });
        exitButton.OnClick += (_, _) =>
        {
            Game.DisconnectFromServer();
            Game.CurrentScreen = null;
        };
    }

    public override void Tick()
    {
        if (Game.Connection is null)
            Game.CurrentScreen = null;
    }

    public override void Draw()
    {
        ScreenDimensions dimensions = Game.MainRenderer.ScreenDimensions;
        Game.MainRenderer.Draw2D.DrawQuad(new()
        {
            Color = (Vector4)Color4.Black,
            Size = dimensions.IntSize
        });
        for (int i = 0; i < messages.Count; i++)
        {
            int y = dimensions.IntHeight - 30 - i * 10;
            Game.MainRenderer.Fonts.Normal.Print(messages[i], 2, y);
        }
        base.Draw();
    }

    public void HandlePacket(IPacket packet)
    {
        if (packet is SSendMessagePacket sendMessage)
        {
            string fullMessage = $"{sendMessage.Author}: {sendMessage.Message}";
            string fullMessageColored = $"{sendMessage.Author}: §cffffff{sendMessage.Message}";
            Console.WriteLine(fullMessage);
            messages.Insert(0, fullMessageColored);
            if (messages.Count > 50)
                messages.RemoveAt(50);
        }
    }
}