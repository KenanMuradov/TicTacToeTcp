using System.Net;
using System.Net.Sockets;
using System.Text.Json;

var clients = new List<TcpClient>();

var listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 27001);

listener.Start(10);



while (true)
{
    var client = await listener.AcceptTcpClientAsync();

    clients.Add(client);

    if (clients.Count % 2 != 0)
        continue;


    new Task(()=>
    {

        Console.WriteLine("Game Started");
        var count = clients.Count;
        var xTurn = true;

        var client1 = clients[count-2];
        var client2 = clients[count-1];

        var stream1 = client1.GetStream();
        var stream2 = client2.GetStream();

        var bw1 =new BinaryWriter(stream1);
        var bw2 =new BinaryWriter(stream2);

        var br1 = new BinaryReader(stream1);
        var br2 = new BinaryReader(stream2);

        bw1.Write(xTurn);
        bw1.Write('X');
        bw2.Write(!xTurn);
        bw2.Write('O');

        while (true)
        {
            if (xTurn)
            {
                var chars = br1.ReadChars(9);
                bw2.Write(chars);
                bw2.Flush();
            }
            else
            {
                var chars = br2.ReadChars(9);
                bw1.Write(chars);
                bw1.Flush();
            }

            xTurn = !xTurn;
        }
    }).Start();
}