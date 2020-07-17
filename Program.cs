using System;
using System.Net;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Text.Encodings;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using MyExtension;
using Adam_Network_Solution;

namespace program_start
{
    class Program
    {
        static void Main(string[] args)
        {

            MBS_AsyncServer srv = new MBS_AsyncServer();
            _ = srv.Start();
            Console.WriteLine($"[Main {Thread.CurrentThread.ManagedThreadId}] Press Any key to exit");
            Console.ReadKey();
            srv.Stop();
            Console.WriteLine($"[Main {Thread.CurrentThread.ManagedThreadId}] Server has been Stopped.");
        }
    }


}
namespace Adam_Network_Solution
{
    class MBS_AsyncServer
    {
        private Socket Server_socket { get; set; }
        private IPAddress Server_address { get; set; }
        private IPEndPoint Server_ep { get; set; }
        private ManualResetEventSlim manual_reset;
        private volatile int counterClient;
        private volatile bool isRunning = false;

        public event Action OnStartEvent;
        public event Action OnStopEvent;
        List<Socket> arr;
        ConcurrentBag<string> messages;

        public int CountClient { get => this.counterClient; }
        public bool IsRunning { get => this.isRunning; }



        public MBS_AsyncServer()
        {
            this.manual_reset = new ManualResetEventSlim(false);
            this.Server_address = IPAddress.Parse("127.0.0.1");
            this.Server_ep = new IPEndPoint(this.Server_address, 8081);
            this.Server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.Server_socket.Bind(this.Server_ep);
            this.Server_socket.Listen(200);  //TODO: after a initialization.
            this.arr = new List<Socket>();
            this.isRunning = true;
            this.messages = new ConcurrentBag<string>();
            this.OnStopEvent += OnStop;
            this.OnStartEvent += MBS_AsyncServer_OnStartEvent;
        }

        private void MBS_AsyncServer_OnStartEvent()
        {
            Console.WriteLine($"[Server {Thread.CurrentThread.ManagedThreadId}] Started");
        }

        public void Stop()
        {
            this.isRunning = false;
            this.OnStopEvent();
        }
        private void OnStop()
        {
            Console.WriteLine($"[Server {Thread.CurrentThread.ManagedThreadId}] Stopped");
        }

        public async Task Start()
        {
            this.OnStartEvent();
            while (this.IsRunning)
            {
                Socket client = await this.Server_socket.AcceptAsync();
                Console.WriteLine($"[Server {Thread.CurrentThread.ManagedThreadId}] ClientAccepted {client.GetHashCode()}");
                _ = Task.Run(async () =>  // if somethink did'nt work with async try to delete here and in line [int dataCounter]
                {
                    byte[] clientResources_1 = new byte[1024];
                    while (client.Connected)
                    {
                        int dataCounter = await client.ReceiveAsync(clientResources_1,SocketFlags.None); // let this thread be free , and comback to other when data is to recive.
                        if (dataCounter <= 0) // client disconeccted and data  counter will be == 0 
                            break;
                        Console.WriteLine($"[Server {Thread.CurrentThread.ManagedThreadId}] Client HC {client.GetHashCode()} Mess {clientResources_1[0..dataCounter].ConvToString()}");
                    }
                    Console.WriteLine($"[Server {Thread.CurrentThread.ManagedThreadId}] Client HC {client.GetHashCode()} DISCONECTED");
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    client.Dispose();
                });
                Console.WriteLine($"[Server {Thread.CurrentThread.ManagedThreadId}] AcceptLoop Running");
            }
            this.Server_socket.Shutdown(SocketShutdown.Both);
            this.Server_socket.Close();
            this.Server_socket.Dispose();
        }
        private Task processClient(Socket client)
        {


            return Task.CompletedTask;
        }

    }

}
namespace MyExtension
{
    public static class exten
    {

        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }


        public static string ConvToString(this byte[] value)
        {
            return Encoding.UTF8.GetString(value);
        }
        public static byte[] ConvToByte(this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }
    }
}
//string save = $"Disconnected -> Cli HC [{client.GetHashCode()}] Rec TH [{System.Threading.Thread.CurrentThread.ManagedThreadId}]";



