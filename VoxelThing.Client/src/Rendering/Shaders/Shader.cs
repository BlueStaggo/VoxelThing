using OpenTK.Graphics.OpenGL;

namespace VoxelThing.Client.Rendering.Shaders;

[Flags]
public enum ShaderTypes
{
	Fragment = 1,
	Vertex = 2,
	Geometry = 4,
	TesselationEvaluation = 8,
	TesselationControl = 16,
	Compute = 32
}

public class Shader : IDisposable
{
	private static readonly string[] ShaderFileExtensions =
	[
		".fsh",
		".vsh",
		".gsh",
		".tesh",
		".tcsh",
		".csh"
	];

	private static readonly ShaderType[] ShaderTypeFlagToGl =
	[
		ShaderType.FragmentShader,
		ShaderType.VertexShader,
		ShaderType.GeometryShader,
		ShaderType.TessEvaluationShader,
		ShaderType.TessControlShader,
		ShaderType.ComputeShader
	];
	
    private static int shaderInUse;

    protected readonly int Handle;
    private bool disposed;

    protected Shader(string path, ShaderTypes types = ShaderTypes.Fragment | ShaderTypes.Vertex)
    {
	    // Load shaders
	    List<int> shaders = [];
	    for (int i = 0; i < 6; i++)
	    {
		    if (((int)types & (1 << i)) == 0)
			    continue;

		    string shaderPath = path + ShaderFileExtensions[i];
		    string shaderSource = ReadShaderSource(shaderPath);

		    int shaderHandle = GL.CreateShader(ShaderTypeFlagToGl[i]);
		    GL.ShaderSource(shaderHandle, shaderSource);
		    GL.CompileShader(shaderHandle);
		    GL.GetShader(shaderHandle, ShaderParameter.CompileStatus, out int compileStatus);

		    if (compileStatus == 0)
		    {
			    string log = GL.GetShaderInfoLog(shaderHandle);
			    throw new FormatException("Failed to compile shader \"" + shaderPath + "\"!\n" + log);
		    }
		    
		    shaders.Add(shaderHandle);
	    }
	    
	    // Load program
		Handle = GL.CreateProgram();
		foreach (int shader in shaders)
			GL.AttachShader(Handle, shader);
		GL.LinkProgram(Handle);
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int linkStatus);
		if (linkStatus == 0)
        {
			string log = GL.GetProgramInfoLog(Handle);
			throw new FormatException("Failed to link program!\n" + log);
		}

		// Delete shaders
		foreach (int shader in shaders)
			GL.DeleteShader(shader);
    }

    ~Shader() => Dispose();

    private static string ReadShaderSource(string path)
    {
        string source = "";

        foreach (string line in File.ReadLines(Path.Combine(Game.AssetsDirectory, "shaders", path)))
        {
            if (line.StartsWith("#include"))
            {
                string includePath = line[(line.IndexOf('"') + 1)..(line.LastIndexOf('"'))];
                if (includePath.StartsWith('/'))
                    includePath = includePath[1..];
                source += ReadShaderSource(includePath);
            }
            else
            {
                source += line;
            }
            source += '\n';
        }

        return source;
    }

    public void Use()
    {
	    if (disposed || shaderInUse == Handle) return;
	    
	    GL.UseProgram(Handle);
	    shaderInUse = Handle;
    }

    public static void Stop()
    {
	    if (shaderInUse == 0) return;
	    
	    GL.UseProgram(0);
	    shaderInUse = 0;
    }

    public void Dispose()
    {
        if (!disposed)
        {
            GL.DeleteProgram(Handle);
            disposed = true;
        }

        if (shaderInUse == Handle) Stop();
        GC.SuppressFinalize(this);
    }
}