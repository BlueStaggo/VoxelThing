using PDS;

namespace VoxelThing.Game.Worlds.Storage;

public class EmptySaveHandler : ISaveHandler
{
    public static readonly EmptySaveHandler Instance = new();

    private EmptySaveHandler() { }

    public void Delete() { }
    public CompoundItem? LoadChunkData(int x, int y, int z) => null;
    public CompoundItem? LoadData(string id, bool compressed = false) => null;
    public void SaveChunkData(int x, int y, int z, CompoundItem data) { }
    public void SaveData(string id, CompoundItem data, bool compressed = true) { }
}