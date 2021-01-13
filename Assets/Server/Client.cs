using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Assets.Server
{
    public class Client : IDisposable
    {
        #region attributes
        public IPEndPoint Endpoint { get; private set; }

        #region outgoing_queues
        public Queue<Message> MessageQueue { get; private set; }
        public Queue<UDPPacket> PacketQueue { get; private set; }
        #endregion

        #region sequence_numbers
        public UInt16 RemoteSeqNum { get; private set; }
        public UInt16 LocalSeqNum { get; private set; }
        #endregion

        // Send and Receive buffers
        #region send_receive_buffers
        private static readonly UInt16 BufferSize = 1024;
        public UInt32[] ReceiveSequenceBuffer   { get; private set; } = new UInt32[BufferSize];
        public UInt32[] SendSequenceBuffer      { get; private set; } = new UInt32[BufferSize];
        public UDPAckPacket[] ReceiveBuffer     { get; private set; } = new UDPAckPacket[BufferSize];
        public UDPAckPacket[] SendBuffer        { get; private set; } = new UDPAckPacket[BufferSize];
        #endregion
        #endregion

        public GameObject Player { get; private set; }
        public UInt32 PlayerID { get; private set; }

        // Time when this client last made contact to server
        public DateTime LastContact { get; private set; }

        private bool disposed = false;

        public Client(IPEndPoint endpoint)
        {
            Debug.Log("Creating player entity!");
            this.Endpoint = endpoint;
            this.MessageQueue = new Queue<Message>();
            this.PacketQueue = new Queue<UDPPacket>();
            this.RemoteSeqNum = 0;
            this.LocalSeqNum = 0;
            this.LastContact = DateTime.Now;

            #region initialize_send_receive_buffers
            for (int i = 0; i < BufferSize; i++)
            {
                this.ReceiveSequenceBuffer[i] = 0xFFFFFFFF;
                this.SendSequenceBuffer[i] = 0xFFFFFFFF;
            }
            #endregion

            Server.instance.TaskQueue.Enqueue(new Action(() => { this.Init(); }));
        }

        private void UpdateHP(KeyValuePair<UInt32, Entity> kv)
        {
            Character character = (Character)kv.Value;
            EntityUpdateMessage hpUpdate = new EntityUpdateMessage(
                EntityUpdateMessage.Type.PLAYER,
                EntityUpdateMessage.Action.HP_UPDATE,
                kv.Value.ID,
                character.currentHealth);
            this.SendMessage(hpUpdate);
            Debug.Log("hello");
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
                            kv.Key,
                            0
                            );
                        MovementMessage move = new MovementMessage(0, kv.Key, 0, 0, e.X, e.Y, 0, e.DX, e.DY);

                        this.SendMessage(entity);
                        this.SendMessage(move);
                        this.UpdateHP(kv);
                    }

                    if (kv.Value.GetType() == typeof(Enemy))
                    {
                        Enemy e = (Enemy)kv.Value;
                        EntityUpdateMessage entity = new EntityUpdateMessage(
                            EntityUpdateMessage.Type.ENEMY,
                            EntityUpdateMessage.Action.CREATE,
                            kv.Key,
                            0
                            );
                        MovementMessage move = new MovementMessage(0, kv.Key, 0, 0, e.X, e.Y, 0, e.DX, e.DY);

                        this.SendMessage(entity);
                        this.SendMessage(move);
                        this.UpdateHP(kv);
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
                                kv.Key,
                                0
                                );
                        } 
                        else if ( e.GetType() == typeof(Crossbow)) 
                        {
                            entity = new EntityUpdateMessage(
                                EntityUpdateMessage.Type.WEAPON_CROSSBOW,
                                EntityUpdateMessage.Action.CREATE,
                                kv.Key,
                                0
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
                this.PlayerID = GameState.instance.CreatePlayer(this, 50, 50);
                //Server.instance.Entities.TryAdd(this.Player.GetComponent<Player>().ID, this.Player.GetComponent<Player>());

                Debug.Log("Entity id is " + this.PlayerID);

                EntityUpdateMessage NewClient = new EntityUpdateMessage(
                    EntityUpdateMessage.Type.PLAYER,
                    EntityUpdateMessage.Action.CREATE,
                    this.PlayerID,
                    0
                    );
                UDPServer.getInstance().BroadcastMessage(NewClient);

                MovementMessage ClientMovement = new MovementMessage(0, this.PlayerID, 0, 0, 0, 0, 0, 0, 0);
                UDPServer.getInstance().BroadcastMessage(ClientMovement);

                EntityUpdateMessage control = new EntityUpdateMessage(
                    EntityUpdateMessage.Type.PLAYER,
                    EntityUpdateMessage.Action.CONTROL,
                    this.PlayerID,
                    0
                    );
                this.SendMessage(control);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public void FixedUpdate()
        {
            UDPPacket p = this.NextPacket();
            this.PacketQueue.Enqueue(p);

            for(int i = 1; i <= 2; i++)
            {
                int index = (UInt16) (this.LocalSeqNum - (UInt16)(i * 15)) % BufferSize;

                if(this.SendSequenceBuffer[index] == (UInt16) (this.LocalSeqNum - (UInt16)(i * 15)))
                {
                    if(!this.SendBuffer[index].Acked)
                    {
                        this.PacketQueue.Enqueue(this.SendBuffer[index].Packet);
                    }
                }
            }    
        }
        
        // Update time when client last made contact
        public void MadeContact()
        {
            this.LastContact = DateTime.Now;
        }

        /// <summary>
        /// Builds a new packet and increments the local sequence ID or
        /// a packet that needs to be resent.
        /// </summary>
        /// <returns>UDPPacket to be sent</returns>
        public UDPPacket NextPacket()
        {
            UDPPacket p = BuildPacket(new UDPPacket(this.LocalSeqNum, this.RemoteSeqNum));

            UInt16 index = (UInt16)(this.RemoteSeqNum % BufferSize);
            for (UInt16 offset = 0; offset < 32; offset++)
            {
                int i = (UInt16)(this.RemoteSeqNum - (UInt16)offset) % BufferSize;

                if(i < 0)
                {
                    i = BufferSize + i;
                }

                if (this.ReceiveSequenceBuffer[i] == (UInt16)(this.RemoteSeqNum - offset))
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

        private int WrapArray(int a)
        {
            if (a < 0)
            {
                return BufferSize + a;
            }
            else
            {
                return a;
            }
        }

        /// <summary>
        /// Acks incoming packet
        /// </summary>
        /// <param name="packet">the packet to ack</param>
        public void AckIncomingPacket(UDPPacket packet)
        {
            int i = packet.SequenceNumber % BufferSize;
            
            if(packet.SequenceNumber > this.RemoteSeqNum)
            {
                this.RemoteSeqNum = packet.SequenceNumber;
            }

            this.ReceiveSequenceBuffer[i] = packet.SequenceNumber;
            this.ReceiveBuffer[i].Acked = true;
            this.ReceiveBuffer[i].Packet = packet;


            i = packet.AckNumber % BufferSize;
            if (this.SendSequenceBuffer[i] == packet.AckNumber)
            {
                this.SendBuffer[i].Acked = true;
            }

            for (UInt16 offset = 1; offset <= 32; offset++)
            {
                if(packet.AckArray[offset - 1])
                {
                    if(this.SendSequenceBuffer[WrapArray((packet.AckNumber - offset) % BufferSize)] == (packet.AckNumber - offset))
                    {
                        this.SendBuffer[WrapArray((packet.AckNumber - offset) % BufferSize)].Acked = true;
                    }
                }
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                // If disposing equals true, dispose all managed and unmanaged resources.
                if(disposing)
                {
                    GameState.instance.DestroyEntityID(this.PlayerID);
                    EntityUpdateMessage message = new EntityUpdateMessage(
                        EntityUpdateMessage.Type.PLAYER,
                        EntityUpdateMessage.Action.DELETE,
                        this.PlayerID,
                        0
                        );
                    UDPServer.getInstance().BroadcastMessage(message);
                }
                
                disposed = true;
            }
        }

        ~Client()
        {
            Dispose(false);
        }
    }
}
