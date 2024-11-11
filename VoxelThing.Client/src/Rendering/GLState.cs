using OpenTK.Graphics.OpenGL;

namespace VoxelThing.Client.Rendering;

public class GlState(GlState? parent = null) : IDisposable
{
    private readonly List<EnableCap> enabled = [], disabled = [];
    private BlendingFactor? sourceBlendingFactor;
    private BlendingFactor? destinationBlendingFactor;
    private CullFaceMode? cullFaceMode;
    private DepthFunction? depthFunc;

    public void Enable(EnableCap cap)
    {
        if (parent != null && parent.enabled.Contains(cap))
            return;

        if (!disabled.Remove(cap))
            enabled.Add(cap);
        GL.Enable(cap);
    }

    public void Disable(EnableCap cap)
    {
        if (parent != null && parent.disabled.Contains(cap))
            return;

        if (!enabled.Remove(cap))
            disabled.Add(cap);
        GL.Disable(cap);
    }

    public void BlendFunc(BlendingFactor sfactor, BlendingFactor dfactor)
    {
        sourceBlendingFactor = sfactor;
        destinationBlendingFactor = dfactor;
        GL.BlendFunc(sfactor, dfactor);
    }
    
    public void CullFace(CullFaceMode mode)
    {
        cullFaceMode = mode;
        GL.CullFace(mode);
    }
    
    public void DepthFunc(DepthFunction function)
    {
        depthFunc = function;
        GL.DepthFunc(function);
    }

    public void Scissor(int x, int y, int width, int height)
    {
        GL.Scissor(x, y, width, height);
        Enable(EnableCap.ScissorTest);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        
        foreach (EnableCap cap in enabled) GL.Disable(cap);
        foreach (EnableCap cap in disabled) GL.Enable(cap);

        if (sourceBlendingFactor is not null && destinationBlendingFactor is not null)
            GL.BlendFunc(
                parent?.sourceBlendingFactor ?? BlendingFactor.SrcAlpha,
                parent?.destinationBlendingFactor ?? BlendingFactor.OneMinusSrcAlpha
            );
        if (cullFaceMode is not null) GL.CullFace(parent?.cullFaceMode ?? CullFaceMode.Back);
        if (depthFunc is not null) GL.DepthFunc(parent?.depthFunc ?? DepthFunction.Less);
    }
}