using App;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Text;

namespace AppTests
{
    [TestClass]
    public class ResponseParserTest
    {
        [TestMethod]
        public void TestSerialNumberParsing()
        {
            var rand = new Random();

            var totalRandomTests = 1000000;
            for (int i = 0; i < totalRandomTests; i++)
            {
                byte[] data = new byte[rand.Next(1, 50)];
                rand.NextBytes(data);
                data[data.Length - 1] = 0x00;

                var testStr = Encoding.ASCII.GetString(data);
                string parsedStr = ResponseParser.ParseSerialNumber(data);

                Assert.AreEqual<string>(testStr, parsedStr);
            }
        }

        [TestMethod]
        public void TestRegistryStatusParsing()
        {
            var rand = new Random();

            var totalRandomTests = 1000000;
            for (int i = 0; i < totalRandomTests; i++)
            {
                var nOne = (ushort)rand.Next(UInt16.MaxValue);
                var nTwo = (ushort)rand.Next(UInt16.MaxValue);

                var nOneBytes = BitConverter.GetBytes(nOne);
                var nTwoBytes = BitConverter.GetBytes(nTwo);

                var bytes = new byte[4]
                {
                nOneBytes[0],
                nOneBytes[1],
                nTwoBytes[0],
                nTwoBytes[1]
                };

                var tuple = ResponseParser.ParseRegistryStatus(bytes);

                Assert.AreEqual<ushort>(nOne, tuple.Item1);
                Assert.AreEqual<ushort>(nTwo, tuple.Item2);
            }
        }

        [TestMethod]
        public void TestSetRegisterResponseSuccessParsing()
        {
            Assert.IsTrue(ResponseParser.IsSetRegisterResponseSuccessful(new byte[1] { 0x00 }));

            var rand = new Random();

            var totalRandomTests = 1000000;
            for (int i = 0; i < totalRandomTests; i++)
            {
                var data = new byte[1];
                rand.NextBytes(data);

                if (data[0] == 0x00)
                {
                    Assert.IsTrue(ResponseParser.IsSetRegisterResponseSuccessful(data));
                }
                else
                {
                    Assert.IsFalse(ResponseParser.IsSetRegisterResponseSuccessful(data));
                }
            }
        }

        [TestMethod]
        public void TestDateTimeParsing()
        {
            var rand = new Random();

            var totalRandomTests = 1000000;
            for (int i = 0; i < totalRandomTests; i++)
            {
                var year = rand.Next(1950, 2100);
                var month = rand.Next(1, 13);
                var day = rand.Next(1, DateTime.DaysInMonth(year, month) + 1);
                var hour = rand.Next(0, 24);
                var minute = rand.Next(0, 60);
                var second = rand.Next(0, 60);

                var uncompressedBits = new bool[32 * 6];
                new BitArray(new int[] { year, month, day, hour, minute, second }).CopyTo(uncompressedBits, 0);
                var compressedBits = new bool[40];

                Array.Copy(uncompressedBits, 0, compressedBits, 28, 12);
                Array.Copy(uncompressedBits, 32, compressedBits, 24, 4);
                Array.Copy(uncompressedBits, 64, compressedBits, 19, 5);
                Array.Copy(uncompressedBits, 96, compressedBits, 14, 5);
                Array.Copy(uncompressedBits, 128, compressedBits, 8, 6);
                Array.Copy(uncompressedBits, 160, compressedBits, 2, 6);

                Array.Reverse(compressedBits);

                var bytes = new byte[5];
                for (var j = 0; j < bytes.Length; j++)
                {
                    bytes[j] = 0x00;
                    for (var k = j * 8; k < j * 8 + 8; k++)
                    {
                        bytes[j] <<= 1;
                        if (compressedBits[k]) bytes[j] |= 0x01;
                    }
                }

                var dateTime = ResponseParser.ParseDateTime(bytes);

                Assert.AreEqual(year, dateTime.Year);
                Assert.AreEqual(month, dateTime.Month);
                Assert.AreEqual(day, dateTime.Day);
                Assert.AreEqual(hour, dateTime.Hour);
                Assert.AreEqual(minute, dateTime.Minute);
                Assert.AreEqual(second, dateTime.Second);
            }
        }

        [TestMethod]
        public void TestEnergyValueParsing()
        {
            var rand = new Random();

            var totalRandomTests = 1000000;
            for (int i = 0; i < totalRandomTests; i++)
            {
                float number = (float)(Int16.MinValue + (Int16.MaxValue - Int16.MinValue) * rand.NextDouble());
                var data = BitConverter.GetBytes(number);

                var result = ResponseParser.ParseEnergyValue(data);

                Assert.AreEqual(number, result);
            }
        }
    }
}
