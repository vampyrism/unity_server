using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;


namespace GameServer
{
    class ServerLogic
    {   
        public static int Port { get; private set; }
        public static int MaxNumConnections { get; private set; }
        public delegate void PacketHandler(int id, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;
        public static Dictionary<int, Client> connectedClients = new Dictionary<int, Client>();
        public static UdpClient udpListener;


        public static void initializeClientDictionary()
        {
            for (int i = 1; i <= MaxNumConnections; i++)
            {
                connectedClients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandler.TestPacketReceived }
            };

        }

        public static void SendUDPPacket(IPEndPoint clientEndpoint, Packet packet)
        {
            try
            {
                if (clientEndpoint != null)
                {
                    udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Could not write package");
            }
        }

        public static void Start(int maxConn, int port)
        {
            Console.WriteLine("Booting up");
            MaxNumConnections = maxConn;
            Port = port;
            initializeClientDictionary();
            //UDP related
            udpListener = new UdpClient(port);
            udpListener.BeginReceive(UponReceiveUDPCallback, null);
        }


    
        public static void UponReceiveUDPCallback(IAsyncResult result)
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] dataReceived = udpListener.EndReceive(result, ref clientEndPoint);
            udpListener.BeginReceive(UponReceiveUDPCallback, null);

            //Perform checks on packet and handle data

            using (Packet packet = new Packet(dataReceived))
            {
                int clientId = packet.ReadInt();
                if(connectedClients[clientId].udpInstance.endPoint == null)
                {
                    connectedClients[clientId].udpInstance.Connect(clientEndPoint);
                    Console.WriteLine("Newly connected Client");
                    return;
                }

                //Check who it came from

                connectedClients[clientId].udpInstance.HandleData(packet);
            }

        }
    }
}
