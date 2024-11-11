using System.Numerics;
using PDS;
using VoxelThing.Game.Maths;

namespace VoxelThing.Game.Worlds.Chunks.Storage;

public abstract class NumericBlockArray<T> : BlockArray
    where T : IBinaryInteger<T>
{
    protected Array3D<T> Data;

    protected override CompoundItem SerializedData => new() 
    {
        ["Data"] = new ArrayItem<T>(Data.Data)
    };

    public NumericBlockArray()
    {
        Data = GetNewArray3D();
    }

    protected virtual Array3D<T> GetNewArray3D() => new(Chunk.Length);

    protected internal override int GetBlockId(int x, int y, int z) => int.CreateTruncating(Data[x, y, z]);
    protected internal override void SetBlockId(int x, int y, int z, int id) => Data[x, y, z] = T.CreateTruncating(id);

    public override void Deserialize(CompoundItem compoundItem)
    {
        if (compoundItem["Data"] is not ArrayItem<T> newDataItem)
        {
            Array.Clear(Data.Data);
            return;
        }

        T[] oldData = Data.Data;
        T[] newData = newDataItem.Value;

        if (newData.Length < oldData.Length)
            Array.Clear(Data.Data);

        Buffer.BlockCopy(newData, 0, oldData, 0, Math.Min(newData.Length, oldData.Length));
    }
}

public class NibbleBlockArray : NumericBlockArray<byte>
{
    protected override Array3D<byte> GetNewArray3D() => new NibbleArray3D(Chunk.Length);

    protected override int MaxPaletteSize => 15;

    protected override BlockArray GetExpandedBlockArray() => new ByteBlockArray()
    {
        Palette = Palette
    };
}

public class ByteBlockArray : NumericBlockArray<byte>
{
    protected override int MaxPaletteSize => 255;

    protected override BlockArray GetExpandedBlockArray() => new TriNibbleBlockArray()
    {
        Palette = Palette
    };
}

public class ShortBlockArray : NumericBlockArray<ushort>
{
    protected override int MaxPaletteSize => 65535;
}