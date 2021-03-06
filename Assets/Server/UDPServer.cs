﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

        Server server;

        // Time until client get kicked out (in seconds)
        public static readonly int MAX_WAIT_TIME = 999999;
        private System.Timers.Timer ClearDisconnectedTimer;

        private CancellationTokenSource tokenSource;
        private CancellationToken token;

        private UDPServer()
        {
            this.serverEndpoint = new IPEndPoint(IPAddress.Any, Server.port);
            this.tokenSource = new CancellationTokenSource();
            this.token = tokenSource.Token;
        }

        public void Init(Server server, int server_port)
        {
            this.serverEndpoint = new IPEndPoint(IPAddress.Any, server_port);
            this.server = server;
            this.socket = new UdpClient(serverEndpoint);
            this.clients = new Dictionary<(string, int), Client>();
            Debug.Log("Started socket on port " + server_port);
            
            // Clear disconnected clients every second
            ClearDisconnectedTimer = new System.Timers.Timer();
            ClearDisconnectedTimer.Interval = 1000;
            ClearDisconnectedTimer.Elapsed += (source, e) => ClearDisconnectedClients();
            ClearDisconnectedTimer.Enabled = true;

            // Server main thread
            Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
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

                Debug.Log("Shutting down...");
            });
        }

        public void Stop() 
        {
            this.tokenSource.Cancel();
            this.socket.Close();
        }

        // This will be called intermittently in order to clear dead connnections
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
            this.ClearDisconnectedTimer.Enabled = !this.token.IsCancellationRequested;
        }

        public void HandleRawPacket(byte[] data, String ip, int port)
        {

            this.clients[(ip, port)].MadeContact();

            UDPPacket packet = new UDPPacket(data);

            #region ackpacket
            bool s = this.clients.TryGetValue((ip, port), out Client c);
            if (!s) throw new Exception("Unable to find client");
            if(!c.AckIncomingPacket(packet))
            {
                return;
            }
            #endregion

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

                    c.FixedUpdate();

                    while (c.PacketQueue.Count > 0)
                    {
                        UDPPacket p = c.PacketQueue.Dequeue();
                        this.socket.Send(p.Serialize(), p.Size, c.Endpoint);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("Exception from send task: " + e.Message + "\n stack trace:\n" + e.StackTrace);
            }
        }
    }
}
