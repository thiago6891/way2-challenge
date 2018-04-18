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
        }
    }
}
