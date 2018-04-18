using System;

namespace App
{
    class Frame
    {
        protected const int MIN_FRAME_SIZE = 4;

        protected const int HEADER_INDEX = 0;
        protected const int LENGTH_INDEX = 1;
        protected const int CODE_INDEX = 2;
        protected const int MSG_INDEX = 3;

        protected const byte HEADER = 0x7D;

        private byte[] bytes;

        public byte[] Bytes { get => bytes; }
        public bool IsError { get => Code == FunctionCode.Error; }
        public FunctionCode Code { get => (FunctionCode)bytes[CODE_INDEX]; }
        public byte Checksum { get => bytes[bytes.Length - 1]; }

        public byte[] Data
        {
            get
            {
                var result = new byte[(int)bytes[LENGTH_INDEX]];
                Array.Copy(bytes, MSG_INDEX, result, 0, result.Length);
                return result;
            }
        }

        public Frame(FunctionCode code, byte[] data = null)
        {
            data = data ?? new byte[0];

            bytes = new byte[MIN_FRAME_SIZE + data.Length];

            bytes[HEADER_INDEX] = HEADER;
            bytes[LENGTH_INDEX] = (byte)data.Length;
            bytes[CODE_INDEX] = (byte)code;

            Array.Copy(data, 0, bytes, MSG_INDEX, data.Length);
            
            bytes[bytes.Length - 1] = CalculateChecksum(bytes);
        }

        private static byte CalculateChecksum(byte[] bytes)
        {
            // Calculates the XOR between all bytes except the first and the last ones.
            byte result = bytes[1];
            for (int i = 2; i < bytes.Length - 1; i++)
                result ^= bytes[i];
            return result;
        }

        public static Frame Parse(byte[] bytes, int bytesToParse)
        {
            if (bytes.Length < MIN_FRAME_SIZE) throw new InvalidFrameException("Invalid size: Too small.");
            if (bytes[HEADER_INDEX] != HEADER) throw new InvalidFrameException("Invalid header.");
            if (bytesToParse != (int)bytes[LENGTH_INDEX] + MIN_FRAME_SIZE) throw new InvalidFrameException("Unexpected size.");

            var data = new byte[(int)bytes[LENGTH_INDEX]];
            Array.Copy(bytes, MSG_INDEX, data, 0, (int)bytes[LENGTH_INDEX]);
            var result = new Frame((FunctionCode)bytes[CODE_INDEX], data);
            
            if (result.Checksum != bytes[bytesToParse - 1])
                throw new InvalidFrameException("Wrong checksum.");

            return result;
        }
    }
}
