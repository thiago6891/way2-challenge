using App;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AppTests
{
    [TestClass]
    public class FrameTest
    {
        private static void TestParsingValidResponseFrame(byte code, int dataLength)
        {
            var data = new byte[dataLength];
            (new Random()).NextBytes(data);

            byte[] bytes = CreateBytes(data, code);
            FillChecksumInFrame(bytes);

            var frame = Frame.Parse(bytes, bytes.Length);

            RunAssertions(data, bytes, frame);
        }

        private static void RunAssertions(byte[] data, byte[] bytes, Frame frame)
        {
            Assert.IsFalse(frame.IsError);
            Assert.AreEqual(data.Length, frame.Data.Length);
            for (int j = 0; j < data.Length; j++)
            {
                Assert.AreEqual(data[j], frame.Data[j]);
            }

            Assert.AreEqual(bytes[2], (byte)frame.Code);
            Assert.AreEqual(bytes[bytes.Length - 1], frame.Checksum);

            Assert.AreEqual(bytes.Length, frame.Bytes.Length);
            for (int j = 0; j < bytes.Length; j++)
            {
                Assert.AreEqual(bytes[j], frame.Bytes[j]);
            }
        }

        private static void FillChecksumInFrame(byte[] bytes)
        {
            byte checksum = bytes[1];
            for (int j = 2; j < bytes.Length - 1; j++)
            {
                checksum ^= bytes[j];
            }
            bytes[bytes.Length - 1] = checksum;
        }

        private static byte[] CreateBytes(byte[] data, byte code)
        {
            var bytes = new byte[4 + data.Length];
            bytes[0] = 0x7D;
            bytes[1] = (byte)data.Length;
            bytes[2] = code;
            Array.Copy(data, 0, bytes, 3, data.Length);
            return bytes;
        }

        [TestMethod]
        public void TestParsingValidResponseFrames()
        {
            var rand = new Random();

            var totalRandomTests = 1000000;
            for (int i = 0; i < totalRandomTests; i++)
            {
                TestParsingValidResponseFrame(0x81, rand.Next(1, 50));
                TestParsingValidResponseFrame(0x82, 4);
                TestParsingValidResponseFrame(0x83, 1);
                TestParsingValidResponseFrame(0x84, 5);
                TestParsingValidResponseFrame(0x85, 4);
            }
        }
        
        [TestMethod]
        public void TestParsingErrorFrame()
        {
            var bytes = new byte[4] { 0x7D, 0x00, 0xFF, 0x00 };
            FillChecksumInFrame(bytes);

            var frame = Frame.Parse(bytes, bytes.Length);

            Assert.IsTrue(frame.IsError);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidFrameException))]
        public void TestParsingTooSmallFrame()
        {
            var frame = Frame.Parse(new byte[2], 2);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidFrameException))]
        public void TestParsingInvalidHeader()
        {
            var bytes = new byte[4] { 0x7E, 0x00, 0xFF, 0x00 };
            FillChecksumInFrame(bytes);
            var frame = Frame.Parse(bytes, bytes.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidFrameException))]
        public void TestParsingInvalidDataLength()
        {
            var rand = new Random();

            var totalRandomTests = 1000000;
            for (int i = 0; i < totalRandomTests; i++)
            {
                var informedDataSize = rand.Next(1, 50);
                var realDataSize = rand.Next(1, 50);
                while (informedDataSize == realDataSize)
                {
                    realDataSize = rand.Next(1, 50);
                }

                var bytes = new byte[4 + realDataSize];
                bytes[0] = 0x7D;
                bytes[1] = (byte)informedDataSize;
                bytes[2] = 0x01;
                FillChecksumInFrame(bytes);

                var frame = Frame.Parse(bytes, bytes.Length);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidFrameException))]
        public void TestParsingInvalidChecksum()
        {
            var bytes = new byte[4] { 0x7D, 0x00, 0xFF, 0x00 };
            
            var frame = Frame.Parse(bytes, bytes.Length);
        }

        [TestMethod]
        public void TestCreateFrameWithoutData()
        {
            var frame = new Frame(FunctionCode.ReadSerial);
            Assert.AreEqual(0, frame.Data.Length);
        }

        [TestMethod]
        public void TestCreateFrameWithData()
        {
            var data = new byte[2];
            (new Random()).NextBytes(data);
            var frame = new Frame(FunctionCode.SetRegistry, data);
            Assert.AreEqual(2, frame.Data.Length);
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(data[i], frame.Data[i]);
            }
        }
    }
}
