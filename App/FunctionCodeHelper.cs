namespace App
{
    class FunctionCodeHelper
    {
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

        public static bool IsExpectedCode(FunctionCode sentCode, FunctionCode receivedCode)
        {
            return (byte)receivedCode == ExpectedResponseCode(sentCode);
        }

        public static bool IsExpectedSize(FunctionCode sentCode, int dataReceived)
        {
            if (!ExpectedResponseSize(sentCode).HasValue) return true;

            return ExpectedResponseSize(sentCode).Value == dataReceived;
        }
    }
}
