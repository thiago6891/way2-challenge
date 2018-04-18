using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace App
{
    class Client
    {
        private const int bufferSize = 256;

        private string address;
        private int port;
        private Socket socket;
        private byte[] response;
        private byte[] buffer;
        private Frame lastFrameSent;

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

            lastFrameSent = frame;
        }

        private void SendCallback(IAsyncResult ar)
        {
            socket.EndSend(ar);
            sendDone.Set();
        }

        private void ReceiveFrame()
        {
            buffer = new byte[bufferSize];
            socket.BeginReceive(buffer, 0, bufferSize, 0, new AsyncCallback(ReceiveCallback), null);
            receiveDone.WaitOne();
            receiveDone.Reset();
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            bool received = false;
            while (!received)
            {
                try
                {
                    int bytesRead = socket.EndReceive(ar);

                    var responseFrame = Frame.Parse(buffer, bytesRead);
                    if (responseFrame.IsError)
                    {
                        // Resend last frame
                        Console.WriteLine("Received Error Frame. Resending last frame...");
                        SendFrame(lastFrameSent.Code, lastFrameSent.Data);
                    }
                    else if (!FunctionCodeHelper.IsExpectedCode(lastFrameSent.Code, responseFrame.Code))
                    {
                        Console.WriteLine("Wrong Function Code. Sending error frame...");
                        SendFrame(FunctionCode.Error);
                    }
                    else if (!FunctionCodeHelper.IsExpectedSize(lastFrameSent.Code, responseFrame.Data.Length))
                    {
                        Console.WriteLine("Unexpected response size. Sending error frame...");
                        SendFrame(FunctionCode.Error);
                    }
                    else
                    {
                        response = responseFrame.Data;
                        received = true;
                        receiveDone.Set();
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
                    received = true;
                    receiveDone.Set();
                }
            }
        }

        private void SendAndReceive(FunctionCode code, byte[] data = null)
        {
            SendFrame(code, data);

            response = null;
            while (response == null)
            {
                ReceiveFrame();
            }
        }

        public string ReadSerialNumber()
        {
            SendAndReceive(FunctionCode.ReadSerial);
            return ResponseParser.ParseSerialNumber(response);
        }

        public Tuple<ushort, ushort> ReadRegistryStatus()
        {
            SendAndReceive(FunctionCode.ReadStatus);
            return ResponseParser.ParseRegistryStatus(response);
        }

        public bool SetRegistryIndexToRead(ushort index)
        {
            SendAndReceive(FunctionCode.SetRegistry, BitConverter.GetBytes(index));
            return ResponseParser.IsSetRegisterResponseSuccessful(response);
        }

        public DateTime ReadDateTime()
        {
            SendAndReceive(FunctionCode.ReadDateTime);
            return ResponseParser.ParseDateTime(response);
        }
        
        public float ReadEnergyValue()
        {
            SendAndReceive(FunctionCode.ReadEnergyValue);
            return ResponseParser.ParseEnergyValue(response);
        }
    }
}
