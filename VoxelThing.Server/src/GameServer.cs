using System.Collections.Concurrent;
using System.Text.Json;
using Hjson;
using System.Net;
using System.Net.Sockets;
using VoxelThing.Game.Networking;
using VoxelThing.Game.Worlds.Storage;
using VoxelThing.Server.Worlds;

namespace VoxelThing.Server;

public class GameServer : IDisposable
{
    private const int TickRate = 50; // In milliseconds
    private const int HandshakeRate = 20; // In ticks
    
    public readonly ConcurrentDictionary<IPEndPoint, Client> Clients = [];
    public readonly ServerWorld World;
    public readonly ServerConfig Config;

    private readonly TcpListener tcpListener;
    
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly CancellationToken cancellationToken;

    public GameServer()
    {
        cancellationToken = cancellationTokenSource.Token;
        
        PacketManager.Init();
        Config = LoadConfig();
        
        Console.WriteLine("Loading world");
        World = new ServerWorld(this, new FolderSaveHandler("world"));
        
        int port = Config.Port;
        Console.WriteLine($"Hosting on port {port}");
        IPEndPoint ipEndPoint = new(IPAddress.Any, port);
        
        tcpListener = new(ipEndPoint);
        tcpListener.Start();
    }

    private static ServerConfig LoadConfig()
    {
        bool saveConfig = true;

        ServerConfig? loadedConfig = null;
        try
        {
            string? configHjson = HjsonValue.Load("server-config.hjson").ToString();
            if (configHjson is not null)
            {
                ServerConfig? deserializedConfig = JsonSerializer.Deserialize<ServerConfig>(configHjson);
                if (deserializedConfig is not null)
                {
                    loadedConfig = deserializedConfig;
                    saveConfig = false;
                }
            }
        }
        catch (Exception)
        {
            // ignored
        }

        loadedConfig ??= new ServerConfig();

        if (saveConfig)
            JsonValue.Parse(JsonSerializer.Serialize(loadedConfig)).Save("server-config.hjson", Stringify.Hjson);

        return loadedConfig;
    }

    public void Run()
    {
        Task.Run(ListenForClients, cancellationToken);

        int tickTime = Environment.TickCount;
        int ticks = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            int currentTime = Environment.TickCount;
            
            // In case the tick counter overflows, skip ticking
            if (currentTime < tickTime)
            {
                tickTime = currentTime;
                continue;
            }

            while (currentTime - tickTime >= TickRate)
            {
                tickTime += TickRate;
                ticks++;

                foreach (Client client in Clients.Values)
                {
                    client.Update();
                    
                    if (ticks % HandshakeRate == 0)
                        client.SendPacket(new SHandshake());
                    
                    while (client.Connection.PendingPackets.TryDequeue(out IPacket? packet))
                        client.PacketHandler.HandlePacket(packet);
                }
                
                World.Tick();
            }

            Thread.Sleep(1);
        }
    }

    private void ListenForClients()
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            TcpClient tcpClient;
            try
            {
                tcpClient = tcpListener.AcceptTcpClient();
            }
            catch (SocketException)
            {
                continue;
            }
            
            if (tcpClient.Client.RemoteEndPoint is not IPEndPoint ipEndPoint)
                continue;
            
            Connection connection = new(tcpClient, ipEndPoint, PacketSide.Client);
            AddClient(connection);
            connection.Disconnected += (_, _) => DisconnectClient(connection);
            connection.StartListening();
        }
    }
    
    private void AddClient(Connection connection)
    {
        if (Clients.TryGetValue(connection.IpEndPoint, out Client? previousClient))
            DisconnectClient(previousClient.Connection);

        Clients[connection.IpEndPoint] = new(this, connection);
        Console.WriteLine($"{connection.IpEndPoint} connected");
    }

    public void DisconnectClient(Connection connection)
    {
        if (!Clients.Remove(connection.IpEndPoint, out Client? client))
            return;
        Console.WriteLine($"{connection.IpEndPoint} disconnected");
        client?.Dispose();
    }

    public void SendPacketToAllClients(IPacket packet)
    {
        foreach (Client client in Clients.Values)
            client.SendPacket(packet);
    }
    
    public void SendPacketToOtherClients(IPacket packet, Client exception)
    {
        foreach (Client client in Clients.Values)
            if (client != exception)
                client.SendPacket(packet);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        cancellationTokenSource.Cancel();
        tcpListener.Dispose();
    }

    public static void Main()
    {
        using GameServer gameServer = new();
        gameServer.Run();
    }
}
