using VoxelThing.Client.Gui.Screens;

namespace VoxelThing.Client.Gui.Controls;

public class Container(Screen screen) : Control(screen)
{
    protected readonly List<Control> Controls = [];

    public virtual Control AddControl(Control control)
    {
        control.Container?.RemoveControl(control);
        Controls.Add(control);
        control.Container = this;
        return control;
    }
    
    public virtual bool RemoveControl(Control control)
    {
        bool removed = Controls.Remove(control);
        if (removed)
            control.Container = null;
        return removed;
    }

    public virtual void ClearControls() => Controls.Clear();

    public override void Draw()
    {
        foreach (Control control in Controls)
            control.Draw();
    }

    public override void CheckMouseClicked(PositionalMouseButtonEventArgs args)
    {
        foreach (Control control in Controls)
            control.CheckMouseClicked(args);
        base.CheckMouseClicked(args);
    }
}