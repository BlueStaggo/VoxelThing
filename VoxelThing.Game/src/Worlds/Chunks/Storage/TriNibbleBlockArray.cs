using PDS;
using VoxelThing.Game.Maths;

namespace VoxelThing.Game.Worlds.Chunks.Storage;

public class TriNibbleBlockArray : BlockArray
{
    protected Array3D<byte> ByteData = new(Chunk.Length);
    protected NibbleArray3D NibbleData = new(Chunk.Length);

    protected override CompoundItem SerializedData => new() 
    {
        ["ByteData"] = new ArrayItem<byte>(ByteData.Data),
        ["NibbleData"] = new ArrayItem<byte>(NibbleData.Data),
    };

    protected override int MaxPaletteSize => 4095;

    protected internal override int GetBlockId(int x, int y, int z) => ByteData[x, y, z] | NibbleData[x, y, z] << 8;

    protected internal override void SetBlockId(int x, int y, int z, int id)
    {
        ByteData[x, y, z] = (byte)id;
        NibbleData[x, y, z] = (byte)((id >>> 8) & 0xF);
    }

    public override void Deserialize(CompoundItem compoundItem)
    {
        if (compoundItem["ByteData"] is not ArrayItem<byte> newByteDataItem
            || compoundItem["NibbleData"] is not ArrayItem<byte> newNibbleDataItem)
        {
            Array.Clear(ByteData.Data);
            Array.Clear(NibbleData.Data);
            return;
        }

        byte[] oldByteData = ByteData.Data;
        byte[] oldNibbleData = NibbleData.Data;
        byte[] newByteData = newByteDataItem.Value;
        byte[] newNibbleData = newNibbleDataItem.Value;

        if (newByteData.Length < oldByteData.Length)
            Array.Clear(oldByteData);
        Buffer.BlockCopy(newByteData, 0, oldByteData, 0, Math.Min(newByteData.Length, oldByteData.Length));

        if (newNibbleData.Length < oldNibbleData.Length)
            Array.Clear(oldNibbleData);
        Buffer.BlockCopy(newNibbleData, 0, oldNibbleData, 0, Math.Min(newNibbleData.Length, oldNibbleData.Length));
    }

    protected override BlockArray GetExpandedBlockArray() => new ShortBlockArray()
    {
        Palette = Palette
    };
}