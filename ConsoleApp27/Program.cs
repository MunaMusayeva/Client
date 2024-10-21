using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

var listener = new Socket(AddressFamily.InterNetwork,
                          SocketType.Dgram,
                          ProtocolType.Udp);

var ip = IPAddress.Parse("127.0.0.1");
var port = 45678;
var listenerEP = new IPEndPoint(ip, port);

listener.Bind(listenerEP);

var buffer = new byte[ushort.MaxValue - 28];
EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

var clients = new List<EndPoint>();

_ = Task.Run(async () =>
{
    while (true)
    {
        var result = await listener.ReceiveFromAsync(new ArraySegment<byte>(buffer),
                                                     SocketFlags.None,
                                                     remoteEP);

        var count = result.ReceivedBytes;
        var msg = Encoding.Default.GetString(buffer, 0, count);
        Console.WriteLine($"Client {result.RemoteEndPoint}: {msg}");

        if (!clients.Contains(result.RemoteEndPoint))
        {
            clients.Add(result.RemoteEndPoint);
        }
        var serverMsg = $"Server received from {result.RemoteEndPoint}: {msg}";
        var sendBuffer = Encoding.Default.GetBytes(serverMsg);

        foreach (var client in clients)
        {
            await listener.SendToAsync(new ArraySegment<byte>(sendBuffer),
                                       SocketFlags.None,
                                       client);
        }
    }
});

while (true)
{
    var serverMsg = Console.ReadLine(); 
    if (string.IsNullOrWhiteSpace(serverMsg))
        continue;

    var sendBuffer = Encoding.Default.GetBytes("Server says: " + serverMsg);
    foreach (var client in clients)
    {
        await listener.SendToAsync(new ArraySegment<byte>(sendBuffer),
                                   SocketFlags.None,
                                   client); 
    }
}
