﻿using System;
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
        public readonly byte ExpectedResponse;
        public readonly int? ExpectedResponseSize;

        public byte[] SentData { get => sentData; }

        public StateObject(FunctionCode sentCode, byte[] sentData, byte expectedResponse, int? expectedResponseSize)
        {
            SentCode = sentCode;
            this.sentData = sentData;
            ExpectedResponse = expectedResponse;
            ExpectedResponseSize = expectedResponseSize;
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

        private Frame SendFrame(FunctionCode code, byte[] data = null)
        {
            var frame = new Frame(code, data);

            socket.BeginSend(frame.Bytes, 0, frame.Bytes.Length, 0, new AsyncCallback(SendCallback), socket);
            sendDone.WaitOne();
            sendDone.Reset();

            return frame;
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
                else if ((byte)frame.Code != state.ExpectedResponse)
                {
                    Console.WriteLine("Wrong Function Code. Sending error frame...");
                    SendFrame(FunctionCode.Error);
                }
                else if (state.ExpectedResponseSize.HasValue && state.ExpectedResponseSize.Value != frame.Data.Length)
                {
                    Console.WriteLine("Unexpected response size. Sending error frame...");
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

        private void SendAndReceive(FunctionCode code, byte[] data = null)
        {
            var frame = SendFrame(code, data);

            response = null;
            while (response == null)
            {
                ReceiveFrame(new StateObject(code, frame.Data, ExpectedResponseCode(code), ExpectedResponseSize(code)));
            }
        }

        private static byte ExpectedResponseCode(FunctionCode code)
        {
            return (byte)(code + 0x80);
        }

        private static int? ExpectedResponseSize(FunctionCode code)
        {
            switch (code)
            {
                case FunctionCode.ReadStatus:
                case FunctionCode.ReadEnergyValue:
                    return 4;
                case FunctionCode.SetRegistry:
                    return 1;
                case FunctionCode.ReadDateTime:
                    return 5;
                default:
                    return null;
            }
        }

        public string ReadSerialNumber()
        {
            SendAndReceive(FunctionCode.ReadSerial);
            return Encoding.ASCII.GetString(response);
        }

        public ushort[] ReadRegistryStatus()
        {
            SendAndReceive(FunctionCode.ReadStatus);
            return new ushort[2] { BitConverter.ToUInt16(response, 0), BitConverter.ToUInt16(response, 2) };
        }
    }
}
