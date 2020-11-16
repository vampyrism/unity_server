using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class ServerSend
    {
        //This class hould hold the various methods for sending various packages across the network


        public static void TestPacketSend(int ClientID, string message)
        {
            using (Packet packet = new Packet((int)ServerPackets.welcome))
            {
                //Send some sort of package
                packet.Write(message);
                packet.Write(ClientID);
                SendUDPData(ClientID, packet);
            }
        }

        private static void SendUDPData(int ClientID, Packet packet)
        {
            packet.WriteLength();
            ServerLogic.connectedClients[ClientID].udpInstance.SendPacket(packet);
        }

    }
}
