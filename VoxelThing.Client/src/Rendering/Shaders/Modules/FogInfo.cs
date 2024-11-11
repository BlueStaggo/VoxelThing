namespace VoxelThing.Client.Rendering.Shaders.Modules;

public readonly struct FogInfo
{
    public readonly IntUniform SkyTexture;
    public readonly FloatUniform SkyWidth;
    public readonly FloatUniform SkyHeight;
    // public readonly Vector3Uniform CameraPosition;
    public readonly FloatUniform HorizontalDistance;
    public readonly FloatUniform VerticalDistance;

    public FogInfo(int handle)
    {
        (SkyTexture = new IntUniform(handle, "fogInfo.skyTex", true)).Set(1);
        SkyWidth = new FloatUniform(handle, "fogInfo.skyWidth", true);
        SkyHeight = new FloatUniform(handle, "fogInfo.skyHeight", true);
        // CameraPosition = new Vector3Uniform(handle, "fogInfo.camPos", true);
        HorizontalDistance = new FloatUniform(handle, "fogInfo.distHor", true);
        VerticalDistance = new FloatUniform(handle, "fogInfo.distVer", true);
    }
}