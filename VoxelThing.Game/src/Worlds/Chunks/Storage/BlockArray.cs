using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MemoryPack;
using PDS;
using VoxelThing.Game.Blocks;
using VoxelThing.Game.Utils.Collections;

namespace VoxelThing.Game.Worlds.Chunks.Storage;

[MemoryPackable]
public partial class BlockArray(int bitSize = 0) : IStructureItemSerializable
{
    [MemoryPackIgnore] private VariableBitArray data = new(Chunk.Volume, bitSize);
    [MemoryPackIgnore] private List<Block?> palette = [];
    
    [MemoryPackConstructor]
    public BlockArray() : this(0)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ArrayIndex(int x, int y, int z) => (x << Chunk.LengthPow2 | y) << Chunk.LengthPow2 | z;
    
    public Block? GetBlock(int x, int y, int z)
    {
        int id = (int)data[ArrayIndex(x, y, z)];
        if (id <= 0 || id > palette.Count) return null;
        return palette[id - 1];
    }

    public void SetBlock(int x, int y, int z, Block? block)
    {
        int coord = ArrayIndex(x, y, z);
        
        if (block is null)
        {
            data[coord] = 0ul;
            return;
        }

        int id = palette.IndexOf(block) + 1;
        if (id == 0)
        {
            palette.Add(block);
            id = palette.Count;

            if (id > (int)data.ElementMask)
            {
                if (data.BitSize >= 32)
                    throw new OutOfMemoryException("Cannot expand to larger block storage!");
                data = data.Resize(data.BitSize + 1);
            }
        }

        data[coord] = (ulong)id;
    }

    public StructureItem Serialize()
        => new CompoundItem()
        {
            ["Palette"] = new ListItem(palette.Select(StructureItem (b) => new StringItem(b?.Id.FullName ?? Block.AirId.FullName)).ToList()),
            ["Data"] = new ArrayItem<ulong>(data.Array.ToArray()),
            ["BitSize"] = new NumericItem<int>(data.BitSize)
        };

    public static BlockArray Deserialize(StructureItem? structureItem)
    {
        if (structureItem is not CompoundItem compoundItem)
            return new BlockArray();

        BlockArray blockArray = new()
        {
            palette = compoundItem["Palette"]?.TryListValue?
                .Select(s => Block.FromId(Identifier.FromFullName(s.StringValue)))
                .ToList() ?? [],
            data = new VariableBitArray(Chunk.Volume,
                compoundItem["BitSize"]?.TryIntValue ?? 0,
                compoundItem["Data"]?.TryULongArrayValue)
        };

        return blockArray;
    }

    [MemoryPackOnSerialized]
    private static void WriteBlockArray<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref BlockArray? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        Debug.Assert(value is not null, "value is not null");
        writer.WritePackableArray(value.palette.Select(b => b?.Id ?? Block.AirId).ToArray());
        writer.WriteUnmanaged(value.data.BitSize);
        writer.WriteUnmanagedArray(value.data.Array);
    }
    
    [MemoryPackOnDeserialized]
    private static void ReadBlockArray(ref MemoryPackReader reader, ref BlockArray? value)
    {
        Debug.Assert(value is not null, "value is not null");
        value.palette = (reader.ReadPackableArray<Identifier>() ?? [])
            .Select(Block.FromId)
            .ToList();
        int bitSize = reader.ReadUnmanaged<int>();
        ulong[]? array = reader.ReadUnmanagedArray<ulong>();
        value.data = new VariableBitArray(Chunk.Volume, bitSize, array);
    }

    public override string ToString()
    {
        return $"Dynamic array of {palette.Count} unique blocks";
    }
}
