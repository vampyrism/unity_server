using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Assets.Server
{
    public class Client
    {
        #region attributes
        public IPEndPoint Endpoint { get; private set; }

        #region outgoing_queues
        public Queue<Message> MessageQueue { get; private set; }
        public List<UDPPacket> PacketQueue { get; private set; }
        #endregion

        #region sequence_numbers
        public UInt16 RemoteSeqNum { get; private set; }
        public UInt16 LocalSeqNum { get; private set; }
        #endregion

        // Send and Receive buffers
        #region send_receive_buffers
        private static readonly int BufferSize = 1024;
        public UInt32[] ReceiveSequenceBuffer   { get; private set; } = new UInt32[BufferSize];
        public UInt32[] SendSequenceBuffer      { get; private set; } = new UInt32[BufferSize];
        public UDPAckPacket[] ReceiveBuffer     { get; private set; } = new UDPAckPacket[BufferSize];
        public UDPAckPacket[] SendBuffer        { get; private set; } = new UDPAckPacket[BufferSize];
        #endregion
        #endregion

        public GameObject Player { get; private set; }

        public Client(IPEndPoint endpoint)
        {
            Debug.Log("Creating player entity!");
            this.Endpoint = endpoint;
            this.MessageQueue = new Queue<Message>();
            this.PacketQueue = new List<UDPPacket>();
            this.RemoteSeqNum = 0;
            this.LocalSeqNum = 0;

            #region initialize_send_receive_buffers
            for (int i = 0; i < BufferSize; i++)
            {
                this.ReceiveSequenceBuffer[i] = 0xFFFFFFFF;
                this.SendSequenceBuffer[i] = 0xFFFFFFFF;
            }
            #endregion

            Server.instance.TaskQueue.Enqueue(new Action(() => { this.Init(); }));
        }

        /// <summary>
        /// Initializes Player, must be run on the main thread
        /// </summary>
        public void Init()
        {
            try
            {
                // Update gamestate of current client
                foreach (KeyValuePair<UInt32, Entity> kv in GameState.instance.Entities)
                {
                    if (kv.Value.GetType() == typeof(Player)) // TODO: Create all entities, not just player
                    {
                        Player e = (Player) kv.Value;
                        EntityUpdateMessage entity = new EntityUpdateMessage(
                            EntityUpdateMessage.Type.PLAYER,
                            EntityUpdateMessage.Action.CREATE,
                            kv.Key
                            );
                        MovementMessage move = new MovementMessage(0, kv.Key, 0, 0, e.X, e.Y, 0, e.DX, e.DY);

                        this.SendMessage(entity);
                        this.SendMessage(move);
                    }

                    if (kv.Value.GetType() == typeof(Enemy))
                    {
                        Enemy e = (Enemy)kv.Value;
                        EntityUpdateMessage entity = new EntityUpdateMessage(
                            EntityUpdateMessage.Type.ENEMY,
                            EntityUpdateMessage.Action.CREATE,
                            kv.Key
                            );
                        MovementMessage move = new MovementMessage(0, kv.Key, 0, 0, e.X, e.Y, 0, e.DX, e.DY);

                        this.SendMessage(entity);
                        this.SendMessage(move);
                    }

                    if (kv.Value.GetType() == typeof(Bow) || kv.Value.GetType() == typeof(Crossbow))
                    {
                        EntityUpdateMessage entity = null;
                        Weapon e = (Weapon)kv.Value;
                        if (e.GetType() == typeof(Bow)) 
                        {
                            entity = new EntityUpdateMessage(
                                EntityUpdateMessage.Type.WEAPON_BOW,
                                EntityUpdateMessage.Action.CREATE,
                                kv.Key
                                );
                        } 
                        else if ( e.GetType() == typeof(Crossbow)) 
                        {
                            entity = new EntityUpdateMessage(
                                EntityUpdateMessage.Type.WEAPON_CROSSBOW,
                                EntityUpdateMessage.Action.CREATE,
                                kv.Key
                                );
                        } 
                        else 
                        {
                            throw new Exception("Weapon type not found");
                        }
                        Debug.Log("Creating a weapon move message with x: " + e.X + " y: " + e.Y);
                        MovementMessage move = new MovementMessage(0, kv.Key, 0, 0, e.X, e.Y, 0, e.DX, e.DY);

                        this.SendMessage(entity);
                        this.SendMessage(move);
                    }

                }

                //this.Player = GameObject.Instantiate(Resources.Load("Player") as GameObject);
                UInt32 playerID = GameState.instance.CreatePlayer(this, 50, 50);
                //Server.instance.Entities.TryAdd(this.Player.GetComponent<Player>().ID, this.Player.GetComponent<Player>());

                Debug.Log("Entity id is " + playerID);

                EntityUpdateMessage NewClient = new EntityUpdateMessage(
                    EntityUpdateMessage.Type.PLAYER,
                    EntityUpdateMessage.Action.CREATE,
                    playerID
                    );
                UDPServer.getInstance().BroadcastMessage(NewClient);

                MovementMessage ClientMovement = new MovementMessage(0, playerID, 0, 0, 0, 0, 0, 0, 0);
                UDPServer.getInstance().BroadcastMessage(ClientMovement);

                EntityUpdateMessage control = new EntityUpdateMessage(
                    EntityUpdateMessage.Type.PLAYER,
                    EntityUpdateMessage.Action.CONTROL,
                    playerID
                    );
                this.SendMessage(control);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        /// <summary>
        /// Builds a new packet and increments the local sequence ID or
        /// a packet that needs to be resent.
        /// </summary>
        /// <returns>UDPPacket to be sent</returns>
        public UDPPacket NextPacket()
        {
            UDPPacket p = BuildPacket(new UDPPacket(this.LocalSeqNum, this.RemoteSeqNum));

            // TODO: Generate Packet ack bitfield
            /*foreach(UDPAckPacket ap in this.ReceiveBuffer)
            {
                if(ap.Acked)
                {
                    p.AckPacket()
                }
            }*/
            UInt16 index = (UInt16)(this.RemoteSeqNum % BufferSize);
            for (UInt16 offset = 0; offset < 32; offset++)
            {
                int i = index - offset;
                if(i < 0)
                {
                    i = BufferSize + i;
                }

                if (this.ReceiveSequenceBuffer[i] == this.RemoteSeqNum - offset)
                {
                    if(this.ReceiveBuffer[i].Acked)
                    {
                        p.AckPacket((UInt16)(this.RemoteSeqNum - offset));
                    }
                }
            }

            this.SendBuffer[this.LocalSeqNum % BufferSize] = new UDPAckPacket { 
                Acked = false,
                SendTime = Time.realtimeSinceStartup, // Note: we care about diff between send-ACK time for RTT calc
                Packet = p
            };

            this.SendSequenceBuffer[this.LocalSeqNum % BufferSize] = this.LocalSeqNum;

            this.LocalSeqNum += 1;

            return p;
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

        /// <summary>
        /// Acks packet with local sequence number <c>SequenceNumber</c>
        /// </summary>
        /// <param name="SequenceNumber">the local sequence number to ack</param>
        public void AckPacket(UInt16 SequenceNumber)
        {
            int i = SequenceNumber % BufferSize;
            if(this.SendSequenceBuffer[i] == SequenceNumber)
            {
                this.SendBuffer[i].Acked = true;
                // TODO: Update RTT calculation
            }
        }

        public UDPAckPacket GetSentPacket(UInt16 SequenceNumber)
        {
            int i = SequenceNumber % BufferSize;
            if (this.SendSequenceBuffer[i] == SequenceNumber)
            {
                return this.SendBuffer[i];
            } 
            else
            {
                return new UDPAckPacket { Acked = false, SendTime = 0, Packet = null };
            }
        }
    }
}
