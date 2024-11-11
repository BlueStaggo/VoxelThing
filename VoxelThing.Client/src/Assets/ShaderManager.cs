using VoxelThing.Client.Rendering.Shaders;

namespace VoxelThing.Client.Assets;

public class ShaderManager : IDisposable
{
    private readonly Dictionary<Type, Shader> shaders = [];

    public T Get<T>()
        where T : Shader, new()
    {
        Type type = typeof(T);
        if (shaders.TryGetValue(type, out Shader? existingShader))
            return (T)existingShader;
        
        T shader = new();
        shaders[type] = shader;
        return shader;
    }

    public void Clear()
    {
        foreach (Shader shader in shaders.Values)
            shader.Dispose();
        shaders.Clear();
    }

    public void Dispose()
    {
        Clear();
        GC.SuppressFinalize(this);
    }
}