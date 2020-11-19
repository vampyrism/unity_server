using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Server
{
    class UDPServer
    {

        private static readonly UDPServer instance = new UDPServer();
        public static UDPServer getInstance()
        {
            return instance;
        }

        IPEndPoint serverEndpoint;
        UdpClient socket;

        public delegate void PacketHandler(int id, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;
        public List<Client> connectedClients = new List<Client>();


        public static void initializeServerData()
        {
            for (int i = 1; i <= MaxNumConnections; i++)
            {
                connectedClients.Add(new Client(i));
            }

            Console.WriteLine("clients initialized");


            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPacketTypes.TestPacket, ServerHandler.TestPacketReceived },
                { (int)ClientPacketTypes.PlayerMovement, ServerHandler.UpdateMovement },
            };

            Console.WriteLine("packetHandlers initialized");

        }



        private UDPServer()
        {
            this.serverEndpoint = new IPEndPoint(IPAddress.Any, 9000);
        }

        public void Init()
        {
            this.socket = new UdpClient(serverEndpoint);
            Debug.Log("Started socket on port " + 9000);

            Task.Run(() =>
            {
                while (true)
                {
                    byte[] data = new byte[1024];
                    
                    data = this.socket.Receive(ref this.serverEndpoint);
                    String ip = this.serverEndpoint.Address.ToString();
                    int port = this.serverEndpoint.Port;

                    DelegatePacket(data);

                    Debug.Log("Got message \"" + Encoding.ASCII.GetString(data, 0, data.Length) + "\" from " + ip + ":" + port.ToString());
                }
            });
        }

        public void DelegatePacket(byte[] data)
        {
            using (UDPPacket packet = new UDPPacket(dataReceived))
            {
                List<Message> messageList = (packet.Deserialize()).messages;
                //gonna write client id at the head
                int senderID = readableData.GetInt(readableData.SENDER_ID);
                //Check who it came from
                connectedClients[senderID].udpInstance.HandleData(readableData);
            }
        }




    }
}
