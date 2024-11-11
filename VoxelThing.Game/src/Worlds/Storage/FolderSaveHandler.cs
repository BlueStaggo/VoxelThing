using System.IO.Compression;
using PDS;

namespace VoxelThing.Game.Worlds.Storage;

public class FolderSaveHandler : ISaveHandler
{
    private readonly string root;
    private readonly string chunkFolder;

    public FolderSaveHandler(string root)
    {
        this.root = root;
        chunkFolder = Path.Combine(root, "chunks");

        Directory.CreateDirectory(root);
        Directory.CreateDirectory(chunkFolder);
    }

    private string GetChunkFilePath(int x, int y, int z)
        => Path.Combine(chunkFolder, $"{x}_{y}_{z}.dat");

    public CompoundItem? LoadData(string id, CompressionLevel compression)
    {
        if (!id.EndsWith(".dat"))
            id = id + ".dat";
        string file = Path.Combine(root, id);
        try
        {
            StructureItem? item = StructureItem.ReadFromPath(file, compression);
            return item as CompoundItem;
        }
        catch (IOException)
        {
            return null;
        }
    }

    public void SaveData(string id, CompoundItem data, CompressionLevel compression)
    {
        if (!id.EndsWith(".dat"))
            id = id + ".dat";
        string file = Path.Combine(root, id);
        try
        {
            data.WriteToPath(file, compression);
        }
        catch (IOException exception)
        {
            Console.Error.WriteLine($"Failed to write world data to {file}");
            Console.Error.WriteLine(exception);
        }
    }

    public CompoundItem? LoadChunkData(int x, int y, int z)
        => LoadData(GetChunkFilePath(x, y, z), CompressionLevel.Optimal);
    
    public void SaveChunkData(int x, int y, int z, CompoundItem data)
        => SaveData(GetChunkFilePath(x, y, z), data, CompressionLevel.Optimal);

    public void Delete() => DeleteDirectory(root);

    private static void DeleteDirectory(string dir)
    {
        if (!Directory.Exists(dir)) return;
        
        foreach (string indir in Directory.EnumerateDirectories(dir))
            DeleteDirectory(indir);
        
        foreach (string file in Directory.EnumerateFiles(dir))
        {
            File.SetAttributes(file, FileAttributes.None);
            File.Delete(file);
        }

        Directory.Delete(dir);
    }
}