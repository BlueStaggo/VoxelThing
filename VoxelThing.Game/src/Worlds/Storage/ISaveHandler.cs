using System.IO.Compression;
using PDS;

namespace VoxelThing.Game.Worlds.Storage;

public interface ISaveHandler
{
    public CompoundItem? LoadData(string id, CompressionLevel compression = CompressionLevel.NoCompression);
    public void SaveData(string id, CompoundItem data, CompressionLevel compression = CompressionLevel.NoCompression);
    public CompoundItem? LoadChunkData(int x, int y, int z);
    public void SaveChunkData(int x, int y, int z, CompoundItem data);
    public void Delete();
}