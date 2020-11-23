using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Server 
{
    class ServerHandler
    {
        public void TestPacketReceived(int SentFrom, UDPPacket packet)
        {
            int actualSender = packet.ReadInt();
            string usr = packet.ReadString();

            if(SentFrom == actualSender)
            {
                Console.WriteLine("Bogus Sender");
            }
        }

        public void ReadHeader(List<Message> dataToHandle)
        {
            dataToHandle
        }

    }
}
