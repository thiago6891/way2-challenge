using System;

namespace App
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Client("40.121.85.141", 20010);
            client.Connect();
            Console.WriteLine(client.ReadSerialNumber());
            ushort[] registers = client.ReadRegistryStatus();
            Console.WriteLine(registers[0]);
            Console.WriteLine(registers[1]);
            
            if (client.SetRegistryIndexToRead(registers[0]))
            {
                Console.WriteLine("Index set successfully!");
            }
            else
            {
                Console.WriteLine("ERROR...");
            }
        }
    }
}
