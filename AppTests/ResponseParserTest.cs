using App;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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

        }

        [TestMethod]
        public void TestEnergyValueParsing()
        {

        }
    }
}
