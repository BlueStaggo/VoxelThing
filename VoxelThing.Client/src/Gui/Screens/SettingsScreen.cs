using OpenTK.Graphics.OpenGL;
using VoxelThing.Client.Gui.Controls;
using VoxelThing.Client.Rendering;
using VoxelThing.Client.Rendering.Textures;
using VoxelThing.Client.Settings;

namespace VoxelThing.Client.Gui.Screens;

public class SettingsScreen : Screen
{
    private const float HeaderHeight = 30.0f;
    
    private readonly ScrollContainer settingList;

    public override bool PausesWorld => true;

    public SettingsScreen(Game game) : base(game)
    {
        var backButton = AddControl(new Label(this)
        {
            Text = "< Back",
            Position = (5, 5),
            Size = (100, 20),
            HasBackground = true
        });
        backButton.OnClick += (_, _) => game.CurrentScreen = Parent;

        settingList = AddControl(new ScrollContainer(this)
        {
            Position = (0, 30),
            Size = (0, -30),
            AlignSize = (1.0f, 1.0f)
        });

        foreach (string category in Game.Settings.Categories)
        {
            settingList.AddControl(new Label(this)
            {
                Text = category,
                Font = Game.MainRenderer.Fonts.Outlined,
                Position = (5, 0),
                Size = (0, 20),
                AlignSize = (1.0f, 0.0f)
            });
            settingList.AddPadding(5);

            foreach (ISetting setting in Game.Settings.GetSettingsFromCategory(category))
            {
                Container settingPanel = new(this)
                {
                    Size = (0, 20),
                    AlignPosition = (0.1f, 0.0f),
                    AlignSize = (0.8f, 0.0f)
                };

                settingPanel.AddControl(new Label(this)
                {
                    Text = setting.Name,
                    AlignText = (1.0f, 0.5f),
                    Position = (-5, 0),
                    AlignPosition = (0.5f, 0.0f),
                    AlignSize = (0.0f, 1.0f)
                });

                Control settingButton = setting.GetControl(this);
                settingButton.AlignPosition = settingButton.AlignPosition * (0.5f, 1.0f) + (0.5f, 0.0f);
                settingButton.AlignSize *= (0.5f, 1.0f);

                settingPanel.AddControl(settingButton);
                settingList.AddControl(settingPanel);
                settingList.AddPadding(5);
            }
        }
    }

    public override void Draw()
    {
        MainRenderer renderer = Game.MainRenderer;
        ScreenDimensions dimensions = renderer.ScreenDimensions;

        Texture background = renderer.Textures.Get("gui/background.png");
        renderer.Draw2D.DrawQuad(new()
        {
            Size = (dimensions.IntWidth, HeaderHeight),
            Uv = (
                0.0f,
                0.0f,
                dimensions.IntWidth / (float)background.Width,
                HeaderHeight / background.Height
            ),
            Texture = background
        });

        using (GlState state = new())
        {
            state.Enable(EnableCap.Blend);
            renderer.Draw2D.DrawQuad(new()
            {
                Position = (0, HeaderHeight),
                Size = (dimensions.IntWidth, dimensions.IntHeight - HeaderHeight),
                Color = (0.0f, 0.0f, 0.0f, 0.5f)
            });
        }
        
        renderer.Fonts.Outlined.Print("SETTINGS", dimensions.IntWidth / 2.0f, 10.0f, align: 0.5f);
        base.Draw();
    }

    protected override void OnMouseScrolled(float scroll)
    {
        base.OnMouseScrolled(scroll);
        settingList.Scroll(scroll * -10.0f);
    }

    public override void OnClosed()
    {
        base.OnClosed();
        Game.Settings.Save();
    }
}