using OpenTK.Graphics.OpenGL;

namespace VoxelThing.Client.Rendering.Shaders;

public class Shader : IDisposable
{
    private static int shaderInUse;

    protected readonly int Handle;
    private bool disposed;

    protected Shader(string path)
    {
		string vertexPath = path + ".vsh";
		string fragmentPath = path + ".fsh";
		string vertexSource = ReadShaderSource(vertexPath);
		string fragmentSource = ReadShaderSource(fragmentPath);

        // Load vertex shader
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexSource);
		GL.CompileShader(vertexShader);
        GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int compileStatus);
		if (compileStatus == 0) 
        {
			string log = GL.GetShaderInfoLog(vertexShader);
			throw new FormatException("Failed to compile shader \"" + vertexPath + "\"!\n" + log);
		}

		// Load fragment shader
		int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
		GL.ShaderSource(fragmentShader, fragmentSource);
		GL.CompileShader(fragmentShader);
        GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out compileStatus);
		if (compileStatus == 0)
        {
			string log = GL.GetShaderInfoLog(fragmentShader);
			throw new FormatException("Failed to compile fragment shader \"" + fragmentPath + "\"!\n" + log);
		}

		// Load program
		Handle = GL.CreateProgram();
		GL.AttachShader(Handle, vertexShader);
		GL.AttachShader(Handle, fragmentShader);
		GL.LinkProgram(Handle);
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int linkStatus);
		if (linkStatus == 0)
        {
			string log = GL.GetProgramInfoLog(Handle);
			throw new FormatException("Failed to link program!\n" + log);
		}

		// Delete shaders
		GL.DeleteShader(vertexShader);
		GL.DeleteShader(fragmentShader);
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