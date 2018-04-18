﻿using System;
using System.Collections;
using System.Text;

namespace App
{
    public class ResponseParser
    {
        public static string ParseSerialNumber(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }

        public static Tuple<ushort, ushort> ParseRegistryStatus(byte[] bytes)
        {
            return Tuple.Create<ushort, ushort>(
                BitConverter.ToUInt16(bytes, 0),
                BitConverter.ToUInt16(bytes, 2));
        }

        public static bool IsSetRegisterResponseSuccessful(byte[] bytes)
        {
            return bytes[0] == 0x00;
        }

        public static DateTime ParseDateTime(byte[] bytes)
        {
            Array.Reverse(bytes);
            var bits = ReverseBitArray(new BitArray(bytes));

            int year = ConvertBitsToInteger(bits, 0, 12);
            int month = ConvertBitsToInteger(bits, 12, 4);
            int day = ConvertBitsToInteger(bits, 16, 5);
            int hour = ConvertBitsToInteger(bits, 21, 5);
            int minute = ConvertBitsToInteger(bits, 26, 6);
            int second = ConvertBitsToInteger(bits, 32, 6);

            return new DateTime(year, month, day, hour, minute, second);
        }

        private static BitArray ReverseBitArray(BitArray array)
        {
            bool[] tmp = new bool[array.Count];
            array.CopyTo(tmp, 0);
            Array.Reverse(tmp);
            return new BitArray(tmp);
        }

        private static int ConvertBitsToInteger(BitArray bits, int idx, int length)
        {
            int result = 0;
            for (int i = idx, f = (int)Math.Round(Math.Pow(2, length - 1)); i < idx + length; i++, f /= 2)
            {
                result += bits[i] ? f : 0;
            }
            return result;
        }

        public static float ParseEnergyValue(byte[] bytes)
        {
            return BitConverter.ToSingle(bytes, 0);
        }
    }
}
