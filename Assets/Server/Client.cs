using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Server
{
    public class Client
    {
        public IPEndPoint Endpoint { get; private set; }
        public Queue<Message> MessageQueue { get; private set; }
        public List<UDPPacket> PacketQueue { get; private set; }
        public UInt16 RemoteSeqNum { get; private set; }
        public UInt16 LocalSeqNum { get; private set; }

        public GameObject Player { get; private set; }

        public Client(IPEndPoint endpoint)
        {
            Debug.Log("Creating player entity!");
            this.Endpoint = endpoint;
            this.MessageQueue = new Queue<Message>();
            this.PacketQueue = new List<UDPPacket>();
            this.RemoteSeqNum = 0;
            this.LocalSeqNum = 0;

            Server.instance.TaskQueue.Enqueue(new Action(() => { this.Init(); }));
        }

        /// <summary>
        /// Initializes Player, must be run on the main thread
        /// </summary>
        public void Init()
        {
            try
            {
                this.Player = GameObject.Instantiate(Resources.Load("Player") as GameObject);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public UDPPacket NextPacket()
        {
            return new UDPPacket();
        }

        /// <summary>
        /// Adds <c>UDPPacket</c> using Messages in the <c>Client</c>
        /// message queue.
        /// </summary>
        /// <param name="p">packet to append new messages to</param>
        /// <returns><c>UDPPacket</c> with messages</returns>
        private UDPPacket BuildPacket(UDPPacket p)
        {
            int space = p.SizeLeft();

            while(this.MessageQueue.Count > 0 && this.MessageQueue.Peek().Size() < space)
            {
                Message m = this.MessageQueue.Dequeue();
                p.AddMessage(m);
            }

            return p;
        }

        public void SendMessage(Message m)
        {
            this.MessageQueue.Enqueue(m);
        }

        public void AckPacket(UInt16 remoteSeqNum)
        {

        }
    }
}
