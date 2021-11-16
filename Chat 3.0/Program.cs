using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chat_3._0
{
    internal class Program
    {
        static object _lock = new object();
        static Dictionary<int, TcpClient> list_client = new Dictionary<int, TcpClient>();
        static void Main(string[] args)
        {
            try
            {

                TcpListener server = new TcpListener(IPAddress.Any, 7000);
                server.Start();
                int count = 1;
                Console.WriteLine("Server started!");

                while(true)
                {
                    TcpClient client = server.AcceptTcpClient();

                    lock(_lock) list_client.Add(count, client);

                    Thread thread = new Thread(handle_clients);
                    thread.Start(count);
                    Console.WriteLine($"We have new user! id#{count++}");
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        private static void handle_clients(object o)
        {
            int id = (int)o;

            var client = new TcpClient();

            lock (_lock)  client = list_client[id];

            NetworkStream stream = client.GetStream();  
            while(true)
            {
                byte[] buffer = new byte[client.ReceiveBufferSize];

                int bytesCount = 0;
                try
                {
                    bytesCount = stream.Read(buffer, 0, client.ReceiveBufferSize);
                }
                catch (Exception)
                {
                    Console.WriteLine($"user#{id} left");

                    lock (_lock) list_client.Remove(id);
                    client.Client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    return;
                }

                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesCount);

                Console.WriteLine($"Data from user#{id}: {dataReceived}");

            }
        }
    }
}
