using System;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Console.Title = "Vamp Server";
            //Some unused port
            ServerLogic.Start(32, 4500);

        }
    }
}
