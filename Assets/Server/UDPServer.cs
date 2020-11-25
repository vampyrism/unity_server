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

        Dictionary<(String, int), IPEndPoint> clients;

        UInt16 remoteSeqNum = 0;
        UInt16 localSeqNum = 0;

        Server server;


        private UDPServer()
        {
            this.serverEndpoint = new IPEndPoint(IPAddress.Any, 9000);
            this.remoteSeqNum = 0;
            this.localSeqNum = 0;
        }

        private void AckPacket(UInt16 seq)
        {
            if (remoteSeqNum < seq)
            {
                remoteSeqNum = seq;
            }
        }

        public void Init(Server server)
        {
            this.server = server;
            this.socket = new UdpClient(serverEndpoint);
            this.clients = new Dictionary<(string, int), IPEndPoint>();
            Debug.Log("Started socket on port " + 9000);

            // Server main thread
            Task.Run(() =>
            {
                while (true)
                {
                    Debug.Log("Received packet");
                    byte[] data = new byte[2048];
                    
                    data = this.socket.Receive(ref this.serverEndpoint);
                    String ip = this.serverEndpoint.Address.ToString();
                    int port = this.serverEndpoint.Port;

                    if (!this.clients.ContainsKey((ip, port)))
                    {
                        this.clients.Add((ip, port), new IPEndPoint(this.serverEndpoint.Address, port)); // TODO: Refactor to client.
                    }

                    if (data.SequenceEqual(ASCIIEncoding.ASCII.GetBytes("Knock, knock")))
                    {
                        Debug.Log("New client connecting!");
                        byte[] res = ASCIIEncoding.ASCII.GetBytes("VAMPIRES!");
                        this.socket.Send(res, res.Length, this.serverEndpoint);
                        continue;
                    }

                    HandleRawPacket(data, ip, port);
                }
            });
        }

        public void HandleRawPacket(byte[] data, String ip, int port)
        {
            // AckPacket(pcktseq);

            UDPPacket packet = new UDPPacket(data);

            List<Message> messages = packet.GetMessages();
            this.server.HandleMessages(messages);
        }

        public void FixedUpdate()
        {
            try
            {
                foreach (IPEndPoint endpoint in this.clients.Values.ToArray())
                {
                    int len = 2 + 2;
                    byte[] res = new byte[len];

                    MovementMessage m = new MovementMessage(this.localSeqNum, 1, 0, 0, 0, 0, 0, 0, 0);
                    UDPPacket packet = new UDPPacket();
                    packet.AddMessage(m);
                    res = packet.Serialize();

                    this.socket.Send(res, res.Length, endpoint);
                    this.localSeqNum += 1;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Exception from send task: " + e.Message);
            }
        }
    }
}
