using PDS;

namespace VoxelThing.Game.Worlds.Storage;

public interface ISaveHandler
{
    public CompoundItem? LoadData(string id, bool compressed = false);
    public void SaveData(string id, CompoundItem data, bool compressed = false);
    public CompoundItem? LoadChunkData(int x, int y, int z);
    public void SaveChunkData(int x, int y, int z, CompoundItem data);
    public void Delete();
}