using System.Collections.Immutable;
using System.Collections.ObjectModel;
using VoxelThing.Game.Blocks.Texture;
using VoxelThing.Game.Worlds;
using VoxelThing.Game.Maths;

namespace VoxelThing.Game.Blocks;

public class Block
{
    public const int TextureAtlasResolution = 512;
    public const int TextureResolution = 16;
    public const int TextureAtlasRows = TextureAtlasResolution / TextureResolution;
    public const float TextureSize = 1.0f / TextureResolution;

    public static readonly Identifier AirId = new("air");

    private static readonly List<Block> RegisteredBlocksOrderedMutable = [];
    private static readonly Dictionary<Identifier, Block?> RegisteredBlocksMutable
        = new() { { AirId, null } };
    public static readonly ReadOnlyCollection<Block> RegisteredBlocksOrdered
        = RegisteredBlocksOrderedMutable.AsReadOnly();
    public static readonly ReadOnlyDictionary<Identifier, Block?> RegisteredBlocks
        = RegisteredBlocksMutable.AsReadOnly();

    private static readonly string[] WoolNames =
    [
        "black",
        "dark_gray",
        "gray",
        "light_gray",
        "yellow",
        "orange",
        "green",
        "teal",
        "turquoise",
        "cyan",
        "blue",
        "navy",
        "red",
        "purple",
        "brown",
        "white",
    ];

    public readonly Identifier Id;
    public IBlockTexture Texture { get; init; } = new AllSidesTexture(0, 0);
    public BlockTransparency Transparency { get; init; } = BlockTransparency.None;
    public bool Translucent { get; init; } = false;

    public bool Transparent => (Transparency & BlockTransparency.Transparent) != 0;
    public bool DrawNeighbors => (Transparency & BlockTransparency.DrawNeighbors) != 0;

    public static readonly Block Stone = new("stone")
    {
        Texture = new AllSidesTexture(1, 0)
    };
    
    public static readonly Block Grass = new("grass")
    {
        Texture = new GrassTexture(new(0, 1), new(0, 0), new(0, 2))
    };
    
    public static readonly Block Dirt = new("dirt")
    {
        Texture = new AllSidesTexture(0, 2)
    };
    
    public static readonly Block Cobblestone = new("cobblestone")
    {
        Texture = new AllSidesTexture(1, 1)
    };
    
    public static readonly Block Bricks = new("bricks")
    {
        Texture = new AllSidesTexture(3, 2)
    };
    
    public static readonly Block Planks = new("planks")
    {
        Texture = new AllSidesTexture(3, 0)
    };
    
    public static readonly Block Log = new("log")
    {
        Texture = new ColumnTexture(new(3, 1), new(4, 1))
    };
    
    public static readonly Block Leaves = new("leaves")
    {
        Texture = new AllSidesTexture(4, 0),
        Transparency = BlockTransparency.Thick
    };
    
    public static readonly Block Glass = new("glass")
    {
        Texture = new AllSidesTexture(4, 2),
        Transparency = BlockTransparency.Thin
    };
    
    public static readonly Block Sand = new("sand")
    {
        Texture = new AllSidesTexture(2, 0)
    };
    
    public static readonly Block Gravel = new("gravel")
    {
        Texture = new AllSidesTexture(2, 1)
    };
    
    public static readonly Block StoneBricks = new("stone_bricks")
    {
        Texture = new AllSidesTexture(2, 2)
    };
    
    public static readonly Block PolishedStone = new("polished_stone")
    {
        Texture = new AllSidesTexture(1, 2)
    };
    
    public static readonly Block Water = new("water")
    {
        Texture = new AllSidesTexture(4, 3),
        Transparency = BlockTransparency.Thin,
        Translucent = true
    };
    
    public static readonly ImmutableArray<Block> Wool =
    [
        ..Enumerable.Range(0, WoolNames.Length)
            .Select(i => new Block("wool_" + WoolNames[i])
            {
                Texture = new AllSidesTexture(i % 4, i / 4 + 3)
            })
    ];

    public Block(string id) : this(new Identifier(id)) { }

    public Block(string idnamespace, string name) : this(new Identifier(idnamespace, name)) { }

    public Block(Identifier id)
    {
        if (RegisteredBlocks.ContainsKey(id))
        {
            throw new InvalidOperationException("Block \"" + id + "\" already exists");
        }

        Id = id;
        RegisteredBlocksOrderedMutable.Add(this);
        RegisteredBlocksMutable[id] = this;
    }

    public override string ToString() => Id.ToString();

    public static Block? FromId(Identifier id) => RegisteredBlocks.GetValueOrDefault(id, null);

    public virtual bool IsFaceDrawn(IBlockAccess blockAccess, int x, int y, int z, Direction face)
    {
        Block? block = blockAccess.GetBlock(x, y, z);
        if (block is null) return true;

        if (Transparent)
            return DrawNeighbors || block != this;
        
        return block.Transparent;
    }

    public virtual Aabb GetCollisionBox(IBlockAccess blockAccess, int x, int y, int z)
        => new(x, y, z, x + 1, y + 1, z + 1);
}