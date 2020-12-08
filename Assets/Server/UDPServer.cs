using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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

        // Time until client get kicked out (in seconds)
        public static readonly int MAX_WAIT_TIME = 3;
        private Timer ClearDisconnectedTimer;

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
            
            // Clear disconnected clients every second
            ClearDisconnectedTimer = new System.Timers.Timer();
            ClearDisconnectedTimer.Interval = 1000;
            ClearDisconnectedTimer.Elapsed += (source, e) => ClearDisconnectedClients();
            ClearDisconnectedTimer.Enabled = true;

            // Server main thread
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        byte[] data = new byte[2048];

                        try
                        {
                            data = this.socket.Receive(ref this.serverEndpoint);
                        }
                        catch (System.Net.Sockets.SocketException e) { continue; }

                        //Debug.Log("Received packet");
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

                                // Initialize client globally
                                Client c;
                                this.clients.TryGetValue((ip, port), out c);
                                this.server.NewClient(c);
                            } 
                            else
                            {
                                // Initiate client locally since they are reconnecting
                            }

                            continue;
                        }

                        try
                        {
                            HandleRawPacket(data, ip, port);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning(e);
                            continue;
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            });
        }

        public void Stop() 
        {
            this.ClearDisconnectedTimer.Enabled = false;
        }

        public void ClearDisconnectedClients()
        {
            DateTime now = DateTime.Now;
            foreach (var cursor in this.clients)
            {   
                Client client = cursor.Value;
                int wait = now.Subtract(client.LastContact).Seconds;
                if (wait > MAX_WAIT_TIME)
                {
                    client.Dispose();
                    this.clients.Remove(cursor.Key);
                }
            }
        }

        public void HandleRawPacket(byte[] data, String ip, int port)
        {
            // AckPacket(pcktseq);
            
            this.clients[(ip, port)].MadeContact();

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
        

        /// <summary>
        /// Adds <c>Message</c> to all Clients message queue
        /// </summary>
        /// <param name="m"></param>
        public void BroadcastMessage(Message m)
        {
            foreach(var cursor in this.clients)
            {
                Client cursorValue = cursor.Value;
                cursorValue.MessageQueue.Enqueue(m);
/*              UDPPacket packetToSend = new UDPPacket();
                packetToSend.AddMessage(m);
                byte[] res = packetToSend.Serialize();
                this.socket.Send(res, res.Length, cursorValue.Endpoint);*/
            }
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

                    UDPPacket p = c.NextPacket();

                    this.socket.Send(p.Serialize(), p.Size, c.Endpoint);
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
