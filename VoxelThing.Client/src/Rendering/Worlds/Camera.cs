using OpenTK.Mathematics;

namespace VoxelThing.Client.Rendering.Worlds;

public class Camera
{
    private readonly Game game;

    public Vector3d Position;

    public Matrix4 Projection => projection;
    public Matrix4 View => view;
    public Matrix4 ViewProjection => viewProjection;

    public Frustum Frustum { get; private set; }

    public Vector3 Front => front;
    public Vector3 Up => up;
    public Vector3 Right => right;

    public float Yaw
    {
        get => yaw;
        set
        {
            yaw = value % 360.0f;
            UpdateVectors();
        }
    }
    
    public float Pitch
    {
        get => pitch;
        set
        {
            if (Math.Abs(Math.Abs(value) - 90.0f) < 0.1f)
                value = 90.1f * MathF.Sign(value);
            pitch = value % 360.0f;
            UpdateVectors();
        }
    }

    public (float Yaw, float Pitch) Rotation
    {
        get => (yaw, pitch);
        set
        {
            yaw = value.Yaw % 360.0f;
            pitch = value.Pitch % 360.0f;
            if (Math.Abs(Math.Abs(pitch) - 90.0f) < 0.1f)
                pitch = 90.1f * MathF.Sign(pitch);
            UpdateVectors();
        }
    }

    public float FovRadians;
    public float Near, Far;

    public float FovDegrees
    {
        get => float.RadiansToDegrees(FovRadians);
        set => FovRadians = float.DegreesToRadians(value);
    }

    public float Aspect => (float)game.ClientSize.X / (float)game.ClientSize.Y;

    private Matrix4 projection;
    private Matrix4 view;
    private Matrix4 viewProjection;

    private Vector3 front = -Vector3.UnitZ;
    private Vector3 up;
    private Vector3 right;

    private float yaw = -90.0f;
    private float pitch = 0.0f;

    public Camera(Game game, float fov = 60.0f, float near = 0.1f, float far = 256.0f)
    {
        this.game = game;

        FovRadians = float.DegreesToRadians(fov);
        Near = near;
        Far = far;

        UpdateVectors();
    }

    private void UpdateVectors()
    {
        bool flip = MathF.Abs(pitch) > 90.0f;

        front = -Vector3.UnitZ;
        front *= Matrix3.CreateRotationX(float.DegreesToRadians(pitch));
        front *= Matrix3.CreateRotationY(float.DegreesToRadians(-yaw - 90.0f));

        right = Vector3.Cross(front, Vector3.UnitY);
        if (flip) right *= Matrix3.CreateRotationY(MathF.PI);
        Vector3.Cross(right, front, out up);
        
        Matrix4.CreatePerspectiveFieldOfView(FovRadians, Aspect, Near, Far, out projection);
        view = Matrix4.LookAt(Vector3.Zero, front, up);
        Matrix4.Mult(view, projection, out viewProjection);

        Frustum = new(ViewProjection);
    }
}