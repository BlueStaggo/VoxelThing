using OpenTK.Mathematics;
using VoxelThing.Client.Rendering.Textures;
using VoxelThing.Game.Entities;

namespace VoxelThing.Client.Rendering.Worlds;

public class EntityRenderer(MainRenderer mainRenderer)
{
    public void RenderEntity(Entity entity)
    {
        double partialTick = mainRenderer.Game.PartialTick;
        Texture texture = mainRenderer.Textures.Get($"entities/{entity.Texture}.png");
        Vector3d cameraPosition = mainRenderer.Camera.Position;
        
        int rotation = entity.GetSpriteAngle(partialTick, mainRenderer.Camera.Yaw);
        Vector4 uv = texture.UvCoordsWithExtents(entity.SpriteFrame * 32, rotation * 32, 32, 32);

        mainRenderer.Draw3D.DrawBillboard(new()
        {
            Position = (Vector3)(entity.GetRenderPosition(partialTick) - cameraPosition),
            Size = entity.SpriteSize,
            Align = (0.5f, 0.0f),
            Texture = texture,
            Uv = uv
        });
    }
}