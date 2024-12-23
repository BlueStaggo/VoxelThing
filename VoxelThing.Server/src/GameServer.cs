using System.Collections.Concurrent;
using System.Text.Json;
using Hjson;
using System.Net;
using System.Net.Sockets;
using VoxelThing.Game.Networking;

namespace VoxelThing.Server;

public class GameServer : IDisposable
{
    private const int TickRate = 50; // In milliseconds
    
    private readonly TcpListener tcpListener;
    private ServerConfig config = new();
    
    private readonly ConcurrentDictionary<IPEndPoint, Connection> clients = [];
    private readonly ConcurrentDictionary<IPEndPoint, string> displayNames = [];

    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly CancellationToken cancellationToken;
    
    public GameServer()
    {
        cancellationToken = cancellationTokenSource.Token;
        
        PacketManager.Init();
        LoadConfig();
        
        int port = config.Port;
        
        Console.WriteLine($"Hosting on port {port}");
        IPEndPoint ipEndPoint = new(IPAddress.Any, port);
        
        tcpListener = new(ipEndPoint);
        tcpListener.Start();
    }

    private void LoadConfig()
    {
        bool saveConfig = true;
        
        try
        {
            string? configHjson = HjsonValue.Load("server-config.hjson")?.ToString();
            if (configHjson is not null)
            {
                ServerConfig? deserializedConfig = JsonSerializer.Deserialize<ServerConfig>(configHjson);
                if (deserializedConfig is not null)
                {
                    config = deserializedConfig;
                    saveConfig = false;
                }
            }
        }
        catch (Exception)
        {
            // ignored
        }

        if (saveConfig)
            JsonValue.Parse(JsonSerializer.Serialize(config)).Save("server-config.hjson", Stringify.Hjson);
    }

    public void Run()
    {
        Task.Run(ListenForClients, cancellationToken);

        int tickTime = Environment.TickCount;
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
                
                foreach (Connection client in clients.Values)
                    while (client.PendingPackets.TryDequeue(out IPacket? packet))
                        HandlePacket(client, packet);
            }
            
            Thread.Sleep(1);
        }
    }

    private void HandlePacket(Connection sender, IPacket packet)
    {
        switch (packet)
        {
            case CSendMessagePacket sendMessage:
                if (!displayNames.TryGetValue(sender.IpEndPoint, out string? author))
                    author = sender.IpEndPoint.ToString();
                Console.WriteLine($"{author}: {sendMessage.Message}");
                foreach (Connection client in clients.Values)
                    client.SendPacket(new SSendMessagePacket(author, sendMessage.Message));
                break;
            
            case CUpdateDisplayName updateDisplayName:
                displayNames[sender.IpEndPoint] = updateDisplayName.DisplayName;
                Console.WriteLine($"{sender.IpEndPoint} set their display name to {updateDisplayName.DisplayName}");
                break;
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
        if (clients.TryGetValue(connection.IpEndPoint, out Connection? previousClient))
            DisconnectClient(previousClient);

        clients[connection.IpEndPoint] = connection;
        Console.WriteLine($"{connection.IpEndPoint} connected");
    }

    public void DisconnectClient(Connection connection)
    {
        clients.Remove(connection.IpEndPoint, out _);
        displayNames.Remove(connection.IpEndPoint, out _);
        Console.WriteLine($"{connection.IpEndPoint} disconnected");
        connection.Dispose();
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
