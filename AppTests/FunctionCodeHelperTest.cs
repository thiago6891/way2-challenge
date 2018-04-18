using App;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AppTests
{
    [TestClass]
    public class FunctionCodeHelperTest
    {
        [TestMethod]
        public void TestExpectedResponseFunctionCodes()
        {
            Assert.IsTrue(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadSerial, (FunctionCode)0x81));
            Assert.IsTrue(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadStatus, (FunctionCode)0x82));
            Assert.IsTrue(FunctionCodeHelper.IsExpectedCode(FunctionCode.SetRegistry, (FunctionCode)0x83));
            Assert.IsTrue(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadDateTime, (FunctionCode)0x84));
            Assert.IsTrue(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadEnergyValue, (FunctionCode)0x85));

            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadSerial, (FunctionCode)0x82));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadSerial, (FunctionCode)0x83));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadSerial, (FunctionCode)0x84));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadSerial, (FunctionCode)0x85));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadSerial, (FunctionCode)0xFF));

            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadStatus, (FunctionCode)0x81));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadStatus, (FunctionCode)0x83));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadStatus, (FunctionCode)0x84));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadStatus, (FunctionCode)0x85));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadStatus, (FunctionCode)0xFF));

            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.SetRegistry, (FunctionCode)0x81));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.SetRegistry, (FunctionCode)0x82));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.SetRegistry, (FunctionCode)0x84));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.SetRegistry, (FunctionCode)0x85));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.SetRegistry, (FunctionCode)0xFF));

            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadDateTime, (FunctionCode)0x81));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadDateTime, (FunctionCode)0x82));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadDateTime, (FunctionCode)0x83));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadDateTime, (FunctionCode)0x85));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadDateTime, (FunctionCode)0xFF));

            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadEnergyValue, (FunctionCode)0x81));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadEnergyValue, (FunctionCode)0x82));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadEnergyValue, (FunctionCode)0x83));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadEnergyValue, (FunctionCode)0x84));
            Assert.IsFalse(FunctionCodeHelper.IsExpectedCode(FunctionCode.ReadEnergyValue, (FunctionCode)0xFF));
        }

        [TestMethod]
        public void TestExpectedResponseSizes()
        {
            Assert.IsTrue(FunctionCodeHelper.IsExpectedSize(FunctionCode.ReadSerial, (new Random()).Next()));
            Assert.IsTrue(FunctionCodeHelper.IsExpectedSize(FunctionCode.ReadStatus, 4));
            Assert.IsTrue(FunctionCodeHelper.IsExpectedSize(FunctionCode.SetRegistry, 1));
            Assert.IsTrue(FunctionCodeHelper.IsExpectedSize(FunctionCode.ReadDateTime, 5));
            Assert.IsTrue(FunctionCodeHelper.IsExpectedSize(FunctionCode.ReadEnergyValue, 4));
        }
    }
}
