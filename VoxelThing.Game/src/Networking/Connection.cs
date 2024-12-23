using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace VoxelThing.Game.Networking;

public class Connection : IDisposable
{
    public event EventHandler? Disconnected;
    
    public readonly IPEndPoint IpEndPoint;
    public readonly ConcurrentQueue<IPacket> PendingPackets = [];

    private readonly TcpClient tcpClient;
    private readonly PacketSide side;
    
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly CancellationToken cancellationToken;
    
    private readonly NetworkStream networkStream;
    private readonly BinaryReader networkBinaryReader;
    private readonly BinaryWriter networkBinaryWriter;

    public Connection(TcpClient tcpClient, IPEndPoint ipEndPoint, PacketSide side)
    {
        this.tcpClient = tcpClient;
        IpEndPoint = ipEndPoint;
        this.side = side;
        cancellationToken = cancellationTokenSource.Token;
        
        networkStream = tcpClient.GetStream();
        networkBinaryReader = new(networkStream, Encoding.UTF8, true);
        networkBinaryWriter = new(networkStream, Encoding.UTF8, true);
    }

    public void StartListening()
    {
        Task.Run(ListenForPackets, cancellationToken);
    }

    public void SendPacket(IPacket packet)
    {
        try
        {
            packet.Write(networkBinaryWriter);
        }
        catch (IOException)
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ListenForPackets()
    {
        try
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                IPacket? packet = IPacket.Read(networkBinaryReader, side);                
                if (packet is null || packet.Side != side) continue;
                PendingPackets.Enqueue(packet);
            }
        }
        catch (IOException) { }
        catch (ObjectDisposedException) { }
        
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        cancellationTokenSource.Cancel();
        tcpClient.Dispose();
        networkBinaryReader.Dispose();
        networkBinaryWriter.Dispose();
    }
}