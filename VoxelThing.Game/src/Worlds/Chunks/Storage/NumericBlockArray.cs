using System.Numerics;
using PDS;
using VoxelThing.Game.Maths;

namespace VoxelThing.Game.Worlds.Chunks.Storage;

public abstract class NumericBlockArray<T>(Array3D<T> data) : BlockArray
    where T : IBinaryInteger<T>
{
    protected readonly Array3D<T> Data = data;

    protected override CompoundItem SerializedData => new() 
    {
        ["Data"] = new ArrayItem<T>(Data.Data)
    };

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

public class NibbleBlockArray() : NumericBlockArray<byte>(new NibbleArray3D(Chunk.Length))
{
    protected override int MaxPaletteSize => 15;

    protected override BlockArray GetExpandedBlockArray() => new ByteBlockArray()
    {
        Palette = Palette
    };
}

public class ByteBlockArray() : NumericBlockArray<byte>(new(Chunk.Length))
{
    protected override int MaxPaletteSize => 255;

    protected override BlockArray GetExpandedBlockArray() => new TriNibbleBlockArray()
    {
        Palette = Palette
    };
}

public class ShortBlockArray() : NumericBlockArray<ushort>(new(Chunk.Length))
{
    protected override int MaxPaletteSize => 65535;
}