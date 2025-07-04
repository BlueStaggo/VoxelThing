namespace VoxelThing.Server;

public record ServerConfig
(
    int Port = 8577,
    int HorizontalChunkDistance = 10,
    int VerticalChunkDistance = 5
);