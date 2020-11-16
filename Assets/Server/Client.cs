using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Data;
using System.Runtime.InteropServices.ComTypes;
using System.ComponentModel;

namespace GameServer
{
    class Client
    {
        public static int BufferSize = 1024;
        public int id;
        public UDP udpInstance;

        public Client(int clientId)
        {
            id = clientId;
            udpInstance = new UDP(id); 
        }
       
        
        public class UDP
        {
            public int id;
            public UdpClient udpClient;

            public IPEndPoint endPoint;

            public UDP(int instanceID)
            {
                id = instanceID;
            }

            public void Connect(IPEndPoint end)
            {
                endPoint = end;
            }

            public void SendPacket(Packet packet)
            {
                ServerLogic.SendUDPPacket(endPoint, packet);
            }

            public void HandleData(Packet packet)
            {
                int length = packet.ReadInt();
                byte[] receivedData = packet.ReadBytes(length);

                using (Packet pkt = new Packet(receivedData))
                {
                    int packetID = pkt.ReadInt();
                    ServerLogic.packetHandlers[packetID](id, pkt);
                }
            }
        }
