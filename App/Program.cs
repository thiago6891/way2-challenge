using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace App
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Missing argument file...");
                Environment.Exit(1);
            }

            var iFile = args[0];

            var reader = new StreamReader(iFile);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var parameters = line.Split(new char[] { ' ' });

                var ip = parameters[0];
                var port = Int32.Parse(parameters[1]);
                var initIdx = Int32.Parse(parameters[2]);
                var lastIdx = Int32.Parse(parameters[3]);

                var client = new Client(ip, port);
                client.Connect();

                var serialNumber = client.ReadSerialNumber();
                Console.WriteLine("Serial Number: {0}", serialNumber);

                var registers = client.ReadRegistryStatus();

                var firstRegister = (ushort)Math.Max(initIdx, registers.Item1);
                var lastRegister = (ushort)Math.Min(lastIdx, registers.Item2);

                Console.WriteLine("Reading Registers: {0} - {1}", firstRegister, lastRegister);

                var readData = new List<Tuple<ushort, DateTime, float>>();
                for (ushort i = firstRegister; i <= lastRegister; i++)
                {
                    if (client.SetRegistryIndexToRead(i))
                    {
                        Console.WriteLine("Register Set Successfully: {0}", i);
                        var dateTime = client.ReadDateTime();
                        var energyVal = client.ReadEnergyValue();
                        readData.Add(new Tuple<ushort, DateTime, float>(i, dateTime, energyVal));
                    }
                }

                client.Disconnect();
                
                var oFile = String.Format("{0}_{1}_{2}.txt", ip, port, DateTime.Now.ToString("yyyyMMddHHmm"));
                var writer = new StreamWriter(oFile);

                Console.WriteLine("Writing to File: {0}", oFile);

                writer.WriteLine(serialNumber);
                foreach (var entry in readData)
                {
                    var dateTime = entry.Item2.ToString("yyyy-MM-dd HH:mm:ss");
                    var energyVal = Math.Round(entry.Item3, 2, MidpointRounding.ToEven)
                        .ToString("F2", CultureInfo.CreateSpecificCulture("pt-BR"));
                    writer.WriteLine(String.Format("{0};{1};{2}", entry.Item1, dateTime, energyVal));
                }

                writer.Close();
            }

            reader.Close();
        }
    }
}
