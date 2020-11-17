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
    public static int MaxNumConnections { get; private set; }
    public delegate void PacketHandler(int id, Packet packet);
    public static Dictionary<int, PacketHandler> packetHandlers;
    public static Dictionary<int, Client> connectedClients = new Dictionary<int, Client>();


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




    class UDPServer
    {
        IPEndPoint serverEndpoint;
        UdpClient socket;

        public UDPServer()
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
                    Debug.Log(Encoding.ASCII.GetString(data, 0, data.Length));
                }
            });
        }
    }
}
