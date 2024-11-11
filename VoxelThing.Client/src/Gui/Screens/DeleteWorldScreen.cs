using VoxelThing.Client.Gui.Controls;
using VoxelThing.Game.Worlds.Storage;

namespace VoxelThing.Client.Gui.Screens;

public class DeleteWorldScreen : Screen
{
    private readonly ISaveHandler saveHandler;
    
    public DeleteWorldScreen(Game game, ISaveHandler saveHandler, string worldName) : base(game)
    {
        this.saveHandler = saveHandler;
        
        AddControl(new Label(this)
        {
            Text = $"Are you sure you want to\ndelete \"{worldName}\"?",
            Position = (0, -10),
            AlignPosition = (0.5f, 0.5f)
        });

        var cancelButton = AddControl(new Label(this)
        {
            Text = "Cancel",
            Position = (-60, 10),
            Size = (50, 20),
            AlignPosition = (0.5f, 0.5f),
            HasBackground = true
        });
        cancelButton.OnClick += (_, _) => game.CurrentScreen = Parent;

        var deleteButton = AddControl(new Label(this)
        {
            Text = "Delete",
            Position = (10, 10),
            Size = (50, 20),
            AlignPosition = (0.5f, 0.5f),
            HasBackground = true
        });
        deleteButton.OnClick += DeleteWorld;
    }

    private void DeleteWorld(object? sender, PositionalMouseButtonEventArgs e)
    {
        try
        {
            saveHandler.Delete();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Failed to delete world!");
            Console.Error.WriteLine(ex);
        }

        if (Parent is SaveSelectScreen saveSelectScreen)
            saveSelectScreen.Refresh();

        Game.CurrentScreen = Parent;
    }
}