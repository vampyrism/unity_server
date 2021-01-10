using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Server
{
    public class Client : IDisposable
    {
        public IPEndPoint Endpoint { get; private set; }
        public Queue<Message> MessageQueue { get; private set; }
        public List<UDPPacket> PacketQueue { get; private set; }
        public UInt16 RemoteSeqNum { get; private set; }
        public UInt16 LocalSeqNum { get; private set; }

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
            this.PacketQueue = new List<UDPPacket>();
            this.RemoteSeqNum = 0;
            this.LocalSeqNum = 0;
            this.LastContact = DateTime.Now;

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
                        Player e = (Player)kv.Value;
                        EntityUpdateMessage entity = new EntityUpdateMessage(
                            EntityUpdateMessage.Type.PLAYER,
                            EntityUpdateMessage.Action.CREATE,
                            kv.Key,
                            0
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
                            kv.Key,
                            0
                            );
                        MovementMessage move = new MovementMessage(0, kv.Key, 0, 0, e.X, e.Y, 0, e.DX, e.DY);

                        this.SendMessage(entity);
                        this.SendMessage(move);
                    }

                    if (kv.Value.GetType() == typeof(Character))
                    {
                        Character character = (Character)kv.Value;
                        EntityUpdateMessage hpUpdate = new EntityUpdateMessage(
                            EntityUpdateMessage.Type.PLAYER,
                            EntityUpdateMessage.Action.HP_UPDATE,
                            kv.Value.ID,
                            character.currentHealth);
                        this.SendMessage(hpUpdate);
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
                        else if (e.GetType() == typeof(Crossbow))
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

        // Update time when client last made contact
        public void MadeContact()
        {
            this.LastContact = DateTime.Now;
        }



        public UDPPacket NextPacket()
        {
            return BuildPacket(new UDPPacket());
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

            while (this.MessageQueue.Count > 0 && this.MessageQueue.Peek().Size() < space)
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed and unmanaged resources.
                if (disposing)
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