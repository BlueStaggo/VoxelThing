using OpenTK.Graphics.OpenGL;

namespace VoxelThing.Client.Rendering;

public class GlState(GlState? parent = null) : IDisposable
{
    private readonly List<EnableCap> enabled = [], disabled = [];
    
    private BlendingFactor? sourceBlendingFactor;
    private BlendingFactor? destinationBlendingFactor;
    private byte? colorMask;
    private CullFaceMode? cullFaceMode;
    private DepthFunction? depthFunction;
    private bool? depthMask;
    
    private BlendingFactor SourceBlendingFactorRecursive
        => sourceBlendingFactor ?? parent?.SourceBlendingFactorRecursive ?? BlendingFactor.SrcAlpha;
    private BlendingFactor DestinationBlendingFactorRecursive
        => destinationBlendingFactor ?? parent?.DestinationBlendingFactorRecursive ?? BlendingFactor.OneMinusSrcAlpha;
    private byte ColorMaskRecursive
        => colorMask ?? parent?.ColorMaskRecursive ?? 0b1111;
    private CullFaceMode CullFaceModeRecursive
        => cullFaceMode ?? parent?.CullFaceModeRecursive ?? CullFaceMode.Back;
    private DepthFunction DepthFunctionRecursive
        => depthFunction ?? parent?.DepthFunctionRecursive ?? DepthFunction.Less;
    private bool DepthMaskRecursive
        => depthMask ?? parent?.DepthMaskRecursive ?? true;

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
        
        if (sourceBlendingFactor == SourceBlendingFactorRecursive
            && destinationBlendingFactor == DestinationBlendingFactorRecursive)
        {
            sourceBlendingFactor = null;
            destinationBlendingFactor = null;
        }
        
        GL.BlendFunc(SourceBlendingFactorRecursive, DestinationBlendingFactorRecursive);
    }
    
    public void ColorMask(bool r, bool g, bool b, bool a)
    {
        byte maskByte = (byte)((r ? 1 : 0) << 3 | (g ? 1 : 0) << 2 | (b ? 1 : 0) << 1 | (a ? 1 : 0));
        colorMask = maskByte == ColorMaskRecursive ? null : maskByte;
        UpdateColorMask();
    }
    
    public void CullFace(CullFaceMode mode)
    {
        cullFaceMode = mode == CullFaceModeRecursive ? null : mode;
        GL.CullFace(CullFaceModeRecursive);
    }
    
    public void DepthFunc(DepthFunction function)
    {
        depthFunction = function == DepthFunctionRecursive ? null : function;
        GL.DepthFunc(DepthFunctionRecursive);
    }
    
    public void DepthMask(bool? mask)
    {
        depthMask = mask == DepthMaskRecursive ? null : mask;
        GL.DepthMask(DepthMaskRecursive);
    }

    public void Scissor(int x, int y, int width, int height)
    {
        GL.Scissor(x, y, width, height);
        Enable(EnableCap.ScissorTest);
    }

    private void UpdateColorMask()
    {
        byte maskByte = ColorMaskRecursive;
        bool r = (maskByte & 8) != 0;
        bool g = (maskByte & 4) != 0;
        bool b = (maskByte & 2) != 0;
        bool a = (maskByte & 1) != 0;
        GL.ColorMask(r, g, b, a);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        
        foreach (EnableCap cap in enabled) GL.Disable(cap);
        foreach (EnableCap cap in disabled) GL.Enable(cap);

        if (sourceBlendingFactor is not null && destinationBlendingFactor is not null)
        {
            sourceBlendingFactor = null;
            destinationBlendingFactor = null;
            GL.BlendFunc(SourceBlendingFactorRecursive, DestinationBlendingFactorRecursive);
        }

        if (colorMask is not null)
        {
            colorMask = null;
            UpdateColorMask();
        }
        
        if (cullFaceMode is not null)
        {
            cullFaceMode = null;
            GL.CullFace(CullFaceModeRecursive);
        }
        
        if (depthFunction is not null)
        {
            depthFunction = null;
            GL.DepthFunc(DepthFunctionRecursive);
        }
        
        if (depthMask is not null)
        {
            depthMask = null;
            DepthMask(DepthMaskRecursive);
        }
    }
}