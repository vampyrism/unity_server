using System;
using System.Collections;
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

        public static UDPServer GetInstance()
        {
            return instance;
        }

        IPEndPoint serverEndpoint;
        UdpClient socket;

        Dictionary<(String, int), IPEndPoint> clients;

        BitArray ackdPackets = new BitArray(65536);
        UInt16 remoteSeqNum = 0;
        UInt16 localSeqNum = 0;

        private UDPServer()
        {
            this.serverEndpoint = new IPEndPoint(IPAddress.Any, 9000);
            
        }

        private void AckPacket(UInt16 seq)
        {
            if(remoteSeqNum < seq)
            {
                remoteSeqNum = seq;
            }

        }

        private bool IsAckPacket(UInt16 seq)
        {
            return false;
        }

        public void Init()
        {
            this.socket = new UdpClient(serverEndpoint);
            this.clients = new Dictionary<(string, int), IPEndPoint>();
            Debug.Log("Started socket on port " + 9000);

            Task.Run(() =>
            {
                while (true)
                {
                    byte[] data = new byte[1024];
                    
                    data = this.socket.Receive(ref this.serverEndpoint);
                    String ip = this.serverEndpoint.Address.ToString();
                    int port = this.serverEndpoint.Port;

                    if (!this.clients.ContainsKey((ip, port)))
                    {
                        this.clients.Add((ip, port), new IPEndPoint(this.serverEndpoint.Address, port));
                    }
                    
                    
                    int length = BitConverter.ToInt32(data, 1);
                    UInt16 pcktseq = BitConverter.ToUInt16(data, 5);
                    UInt16 strlen = BitConverter.ToUInt16(data, 7);

                    AckPacket(pcktseq);

                    String str = Encoding.ASCII.GetString(data, 9, strlen);

                    Debug.Log("Got message \"" + str + "\" from " + ip + ":" + port.ToString());
                }
            });

            Task.Run(() =>
            {
                while(true)
                {
                    //Debug.Log("Currently " + this.clients.Count.ToString() + " clients connected");

                    try
                    {
                        foreach (IPEndPoint endpoint in this.clients.Values.ToArray())
                        {
                            //Debug.Log("Updating endpoint : " + endpoint.ToString());
                            int len = 2 + 2;
                            byte[] res = new byte[len];

                            BitConverter.GetBytes((UInt16)this.localSeqNum).CopyTo(res, 0); // Local seq
                            BitConverter.GetBytes((UInt16)this.remoteSeqNum).CopyTo(res, 2); // Ack

                            this.socket.Send(res, res.Length, endpoint);
                            this.localSeqNum += 1;
                        }
                    } catch(Exception e)
                    {
                        Debug.Log("Exception from send task: " + e.Message);
                    }

                    System.Threading.Thread.Sleep(250);
                }
            });
        }
    }
}
