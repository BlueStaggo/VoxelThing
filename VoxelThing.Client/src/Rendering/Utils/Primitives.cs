using VoxelThing.Client.Rendering.Vertices;

namespace VoxelThing.Client.Rendering.Utils;

public static class Primitives
{
    public static readonly Vector3Primitives OfVector3 = new();
    public static readonly WorldPrimitives InWorld = new();
    public static readonly WorldFloatPrimitives InWorldFloat = new();
}

public abstract class Primitives<TB>
    where TB : Bindings
{
    protected abstract TB NewBindings();

    protected abstract void AddPosition(TB bindings, float x, float y, float z);

    protected abstract void AddColor(TB bindings, float r, float g, float b);

    protected abstract void AddUv(TB bindings, float u, float v);

    public TB GenerateSphere(TB? bindings, float radius, int rings, int sectors) {
        bindings ??= NewBindings();

        bindings.Clear();
        float sectorStep = 2.0f * MathF.PI / sectors;
        float ringStep = MathF.PI / rings;

        for (int i = 0; i <= rings; ++i)
        {
            float ringAngle = MathF.PI / 2.0f - i * ringStep;
            float xy = radius * MathF.Cos(ringAngle);
            float z = radius * MathF.Sin(ringAngle);

            for (int j = 0; j <= sectors; ++j)
            {
                double sectorAngle = j * sectorStep;
                float x = (float)(xy * Math.Cos(sectorAngle));
                float y = (float)(xy * Math.Sin(sectorAngle));
                float u = (float)j / sectors;
                float v = (float)i / sectors;
                AddPosition(bindings, x, y, z);
                AddColor(bindings, 1.0f, 1.0f, 1.0f);
                AddUv(bindings, u, v);
            }
        }

        for (int i = 0; i < rings; ++i)
        {
            int k1 = i * (sectors + 1);
            int k2 = k1 + sectors + 1;

            for (int j = 0; j < sectors; ++j, ++k1, ++k2)
            {
                if (i != 0)
                {
                    bindings.AddIndex(k1);
                    bindings.AddIndex(k2);
                    bindings.AddIndex(k1 + 1);
                }

                if (i != rings - 1)
                {
                    bindings.AddIndex(k1 + 1);
                    bindings.AddIndex(k2);
                    bindings.AddIndex(k2 + 1);
                }
            }
        }

        bindings.Upload(false);
        return bindings;
    }

    public TB GeneratePlane(TB? bindings)
        => GeneratePlane(bindings, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f);

    public TB GeneratePlane(TB? bindings, float x, float y, float z, float width, float length)
        => GeneratePlane(bindings, x, y, z, width, length, 1.0f, 1.0f, 1.0f);

    public TB GeneratePlane(TB? bindings, float x, float y, float z, float width, float length, float r, float g, float b)
    {
        bindings ??= NewBindings();

        AddPosition(bindings, width + x, y, length + z);
        AddColor(bindings, r, g, b);
        AddUv(bindings, 1.0f, 1.0f);

        AddPosition(bindings, width + x, y, z);
        AddColor(bindings, r, g, b);
        AddUv(bindings, 1.0f, 0.0f);

        AddPosition(bindings, x, y, z);
        AddColor(bindings, r, g, b);
        AddUv(bindings, 0.0f, 0.0f);

        AddPosition(bindings, x, y, length + z);
        AddColor(bindings, r, g, b);
        AddUv(bindings, 0.0f, 1.0f);

        bindings.AddIndices(0, 1, 2, 2, 3, 0);
        bindings.Upload(false);
        return bindings;
    }
}

public class Vector3Primitives : Primitives<FloatBindings>
{
    internal Vector3Primitives() { }

    protected override FloatBindings NewBindings()
        => new(new(VertexType.Vector3));

    protected override void AddPosition(FloatBindings bindings, float x, float y, float z)
        => bindings.Put(x).Put(y).Put(z);

    protected override void AddColor(FloatBindings bindings, float r, float g, float b) { }

    protected override void AddUv(FloatBindings bindings, float u, float v) { }
}

public class WorldPrimitives : Primitives<MixedBindings>
{
    internal WorldPrimitives() { }

    protected override MixedBindings NewBindings()
        => new(VertexLayout.Block);

    protected override void AddPosition(MixedBindings bindings, float x, float y, float z)
        => bindings.Put(x).Put(y).Put(z);

    protected override void AddColor(MixedBindings bindings, float r, float g, float b)
        => bindings.Put((byte)(r * 255.0f), (byte)(g * 255.0f), (byte)(b * 255.0f));

    protected override void AddUv(MixedBindings bindings, float u, float v)
        => bindings.Put(u).Put(v);
}

public class WorldFloatPrimitives : Primitives<FloatBindings>
{
    internal WorldFloatPrimitives() { }

    protected override FloatBindings NewBindings()
        => new(VertexLayout.World);

    protected override void AddPosition(FloatBindings bindings, float x, float y, float z)
        => bindings.Put(x).Put(y).Put(z);

    protected override void AddColor(FloatBindings bindings, float r, float g, float b)
        => bindings.Put(r).Put(g).Put(b);

    protected override void AddUv(FloatBindings bindings, float u, float v)
        => bindings.Put(u).Put(v);
}
