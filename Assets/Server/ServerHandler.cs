using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class ServerHandler
    {
        public void TestPacketReceived(int SentFrom, Packet packet)
        {
            int actualSender = packet.ReadInt();
            string usr = packet.ReadString();

            if(SentFrom == actualSender)
            {
                Console.WriteLine("Bogus Sender");
            }
        }

    }
}
