using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VoxelThing.Game.Maths;

// It really sucks how C#'s default Random doesn't support xoshiro256** with a custom 64 bit seed. Not cool.

public class Random64 : Random
{
    private static readonly ulong[] JumpSeed =
    [
        0x180ec6d33cfd0aba, 0xd5a61266f0c9392c, 0xa9582618e03fc9aa, 0x39abdc4529b1661c
    ];

    private static readonly ulong[] LongJumpSeed =
    [
        0x76e15d3efefdcbbf, 0xc5004e441c522fb3, 0x77710069854ee241, 0x39109bb02acbe635
    ];
    
    private ulong s0, s1, s2, s3;
    
    public Random64() : this((ulong)DateTime.Now.Ticks) { }
    
    public Random64(ulong seed)
    {
        s0 = Salt(ref seed);
        s1 = Salt(ref seed);
        s2 = Salt(ref seed);
        s3 = Salt(ref seed);
    }

    private static ulong Salt(ref ulong seed)
    {
        ulong subSeed = seed += 0x9e3779b97f4a7c15;
        subSeed = (subSeed ^ (subSeed >> 30)) * 0xbf58476d1ce4e5b9;
        subSeed = (subSeed ^ (subSeed >> 27)) * 0x94d049bb133111eb;
        return subSeed ^ subSeed >> 31;
    }
    
    public ulong NextUInt64()
    {
        ulong result = BitOperations.RotateLeft(s1 * 5, 7) * 9;
        
        ulong t = s1 << 17;

        s2 ^= s0;
        s3 ^= s1;
        s1 ^= s2;
        s0 ^= s3;
        
        s2 ^= t;

        s3 = BitOperations.RotateLeft(s3, 45);

        return result;
    }

    public void Jump(bool longJump = false)
    {
        ulong[] jumpSeed = longJump ? LongJumpSeed : JumpSeed;

        ulong ns0 = 0;
        ulong ns1 = 0;
        ulong ns2 = 0;
        ulong ns3 = 0;
        
        for (int i = 0; i < 4; i++)
        for (int b = 0; b < 64; b++)
        {
            if ((jumpSeed[i] & 1UL << b) != 0)
            {
                ns0 ^= s0;
                ns1 ^= s1;
                ns2 ^= s2;
                ns3 ^= s3;
            }
            NextUInt64();
        }

        s0 = ns0;
        s1 = ns1;
        s2 = ns2;
        s3 = ns3;
    }

    public override long NextInt64() => (long)NextUInt64();
    
    public uint NextUInt32() => (uint)(NextUInt64() >> 32);
    
    public override int Next() => (int)(NextUInt64() >> 32);

    public ulong NextUInt64(ulong max)
    {
        ulong high = Math.BigMul(max, NextUInt64(), out ulong low);
        if (low >= max) return high;
        
        ulong target = unchecked(0UL - max) % max;
        while (low < target)
            high = Math.BigMul(max, NextUInt64(), out low);
        return high;
    }
    
    public override long NextInt64(long max) => (long)NextUInt64((ulong)max);

    public uint NextUInt32(uint max)
    {
        ulong result = (ulong)max * NextUInt32();
        uint low = (uint)result;
        if (low >= max) return (uint)(result >> 32);
        
        for (uint i = (uint)(-(int)max % max); low < i; low = (uint)result)
            result = (ulong)max * NextUInt32();
        return (uint) (result >> 32);
    }
 
    public override int Next(int max) => (int)NextUInt32((uint)max);

    public ulong NextUInt64(ulong min, ulong max) => NextUInt64(max - min) + min;
    
    public override long NextInt64(long min, long max) => (long)NextUInt64((ulong)(max - min)) + min;
    
    public uint NextUInt32(uint min, uint max) => NextUInt32(max - min) + min;
    
    public override int Next(int min, int max) => (int)NextUInt32((uint)(max - min)) + min;
    
    public override double NextDouble() => (NextUInt64() >> 11) * 1.1102230246251565E-16;
    
    public override float NextSingle() => (NextUInt64() >> 40) * 5.9604645E-08f;

    public double NextDouble(double min, double max) => NextDouble() * (max - min) + min;
    
    public float NextSingle(float min, float max) => NextSingle() * (max - min) + min;

    public override void NextBytes(byte[] buffer) => throw new NotSupportedException();

    public override void NextBytes(Span<byte> buffer) => throw new NotSupportedException();
}