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

        Dictionary<(String, int), Client> clients;

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
            this.clients = new Dictionary<(string, int), Client>();
            Debug.Log("Started socket on port " + 9000);

            // Server main thread
            Task.Run(() =>
            {
                while (true)
                {
                    byte[] data = new byte[2048];
                    
                    data = this.socket.Receive(ref this.serverEndpoint);
                    Debug.Log("Received packet");
                    String ip = this.serverEndpoint.Address.ToString();
                    int port = this.serverEndpoint.Port;

                    if (data.SequenceEqual(ASCIIEncoding.ASCII.GetBytes("Knock, knock")))
                    {
                        Debug.Log("New client connecting!");
                        byte[] res = ASCIIEncoding.ASCII.GetBytes("VAMPIRES!");
                        this.socket.Send(res, res.Length, this.serverEndpoint);

                        if (!this.clients.ContainsKey((ip, port)))
                        {
                            this.clients.Add((ip, port), new Client(new IPEndPoint(this.serverEndpoint.Address, port))); // TODO: Refactor to client.
                        }

                        // Initialize client
                        Client c;
                        this.clients.TryGetValue((ip, port), out c);
                        this.server.NewClient(c);

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
            try
            {
                this.server.HandleMessages(messages);
            } 
            catch(Exception e)
            {
                Debug.LogError("Exception when handling messages: " + e.Message);
            }
        }

        public void SendMessage(Message m)
        {

        }

        public void FixedUpdate()
        {
            try
            {
                foreach (Client c in this.clients.Values.ToArray())
                {
                    int len = 508;
                    byte[] res = new byte[len];

                    //MovementMessage m = new MovementMessage(this.localSeqNum, 1, 0, 0, 0, 0, 0, 0, 0);
                    //UDPPacket packet = new UDPPacket();
                    //packet.AddMessage(m);
                    //res = packet.Serialize();

                    this.socket.Send(res, res.Length, c.Endpoint);
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
