using System.Collections.ObjectModel;
using MemoryPack;

namespace VoxelThing.Game.Utils.Collections;

[MemoryPackable]
public partial class VariableBitArray
{
    public readonly int Length;
    public readonly int BitSize;

    private readonly int elementsPerBlock;
    public ulong ElementMask { get; }

    public readonly ulong[] Array;
    
    public VariableBitArray(int length, int bitSize, ulong[]? array = null)
    {
        if (bitSize is < 0 or > 64)
            throw new ArgumentOutOfRangeException(nameof(bitSize), "Bit size must be between 0 and 64");

        int arrayLength = bitSize == 0 ? 1 : (length + bitSize - 1) / bitSize;
        if (array is null || array.Length < arrayLength)
            array = new ulong[arrayLength];

        Length = length;
        BitSize = bitSize;

        elementsPerBlock = bitSize == 0 ? int.MaxValue : 64 / bitSize;
        ElementMask = bitSize == 0 ? 0ul : (1ul << bitSize) - 1ul;

        Array = array;
    }

    public ulong this[int i]
    {
        get
        {
            if (i < 0 || i >= Length)
                throw new ArgumentOutOfRangeException(nameof(i));

            int blockIndex = i / elementsPerBlock;
            int elementIndex = i % elementsPerBlock;

            ulong value = Array[blockIndex];
            value >>= BitSize * elementIndex;
            value &= ElementMask;
            return value;
        }
        set
        {
            if (i < 0 || i >= Length)
                throw new ArgumentOutOfRangeException(nameof(i));

            if (BitSize < 64 && value > ElementMask)
                throw new ArgumentException("Value is too large", nameof(value));

            int blockIndex = i / elementsPerBlock;
            int elementIndex = i % elementsPerBlock;
            int elementShift = BitSize * elementIndex;

            ulong block = Array[blockIndex];
            block &= ~(ElementMask << elementShift);
            block |= value << elementShift;
            Array[blockIndex] = block;
        }
    }

    public VariableBitArray Resize(int newBitSize)
    {
        VariableBitArray newArray = new(Length, newBitSize);
        for (int i = 0; i < Length; i++)
            newArray[i] = this[i];
        return newArray;
    }
}