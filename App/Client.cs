using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace App
{
    class StateObject
    {
        private readonly byte[] sentData;

        public readonly FunctionCode SentCode;
        public readonly FunctionCode ExpectedResponse;

        public byte[] SentData { get => sentData; }

        public StateObject(FunctionCode sentCode, byte[] sentData, FunctionCode expectedResponse)
        {
            SentCode = sentCode;
            this.sentData = sentData;
            ExpectedResponse = expectedResponse;
        }
    }

    class Client
    {
        private const int bufferSize = 256;

        private string address;
        private int port;
        private Socket socket;
        private byte[] response;
        private byte[] buffer;

        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);

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

                socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), socket);
                connectDone.WaitOne();
                connectDone.Reset();
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
                socket.EndConnect(ar);
                Console.WriteLine("Socket connected to {0}", socket.RemoteEndPoint.ToString());
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void SendFrame(FunctionCode code, byte[] data = null)
        {
            var frame = new Frame(code, data);

            socket.BeginSend(frame.Bytes, 0, frame.Bytes.Length, 0, new AsyncCallback(SendCallback), socket);
            sendDone.WaitOne();
            sendDone.Reset();
        }

        private void SendCallback(IAsyncResult ar)
        {
            var bytesSent = socket.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to server.", bytesSent);
            sendDone.Set();
        }

        private void ReceiveFrame(StateObject state)
        {
            buffer = new byte[bufferSize];
            socket.BeginReceive(buffer, 0, bufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            receiveDone.WaitOne();
            receiveDone.Reset();
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var state = (StateObject)ar.AsyncState;
                int bytesRead = socket.EndReceive(ar);
                Console.WriteLine("Received {0} bytes from server.", bytesRead);

                var frame = Frame.Parse(buffer, bytesRead);
                if (frame.IsError)
                {
                    // Resend last frame
                    Console.WriteLine("Received Error Frame. Resending last frame...");
                    SendFrame(state.SentCode, state.SentData);
                }
                else if (frame.Code != state.ExpectedResponse)
                {
                    Console.WriteLine("Wrong Function Code. Sending error frame...");
                    SendFrame(FunctionCode.Error);
                }
                else
                {
                    response = frame.Data;
                }
            }
            catch (InvalidFrameException e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("Sending error frame...");
                SendFrame(FunctionCode.Error);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                receiveDone.Set();
            }
        }

        public string ReadSerialNumber()
        {
            SendFrame(FunctionCode.ReadSerial);

            response = null;
            while (response == null)
            {
                ReceiveFrame(new StateObject(FunctionCode.ReadSerial, null, FunctionCode.ReadSerialResponse));
            }

            return Encoding.ASCII.GetString(response);
        }
    }
}
