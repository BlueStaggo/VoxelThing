using System.Collections.ObjectModel;
using OpenTK.Mathematics;
using VoxelThing.Game.Maths;
using VoxelThing.Game.Networking;
using VoxelThing.Game.Worlds.Chunks;

namespace VoxelThing.Server.Worlds;

public class PlayerIsland(Client client)
{
    private const int MaxChunksPerTick = 5;
    
    private readonly HashSet<Vector3i> sentChunks = [];

    public void Update()
    {
        if (client.Character is null) return;
        
        GameServer server = client.Server;
        Vector3i center = client.Character.ChunkPosition;
        int horizontalDist = server.Config.HorizontalChunkDistance;
        int verticalDist = server.Config.VerticalChunkDistance;
        
        ReadOnlyCollection<Vector3i> pointsToCheck
            = MathUtil.GetCuboidPoints(horizontalDist, verticalDist, horizontalDist);
        int chunksSent = 0;
        
        foreach (var point in pointsToCheck)
        {
            Vector3i offsetPoint = point + center;
            if (sentChunks.Contains(offsetPoint)) continue;
            
            Chunk? chunk = server.World.GetChunkAt(offsetPoint.X, offsetPoint.Y, offsetPoint.Z);
            if (chunk is null) continue;
            
            sentChunks.Add(offsetPoint);
            client.SendPacket(new SLoadChunk(chunk));
            if (++chunksSent >= MaxChunksPerTick) break;
        }

        sentChunks.RemoveWhere(
            point =>
                Math.Abs(point.X - center.X) > horizontalDist
                || Math.Abs(point.Y - center.Y) > verticalDist
                || Math.Abs(point.Z - center.Z) > horizontalDist
        );
    }
}