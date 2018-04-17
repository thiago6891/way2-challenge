using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace App
{
    class Client
    {
        private string address;
        private int port;

        private ManualResetEvent connectDone = new ManualResetEvent(false);

        public Client(string address, int port)
        {
            this.address = address;
            this.port = port;
        }

        public void Connect()
        {
            try
            {
                var ipAddress = IPAddress.Parse(address);
                var remoteEP = new IPEndPoint(ipAddress, port);

                var client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket)ar.AsyncState;
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());

                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
