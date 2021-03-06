﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace Assets.Server
{
    class MessageVisitorGameStateUpdater : IMessageVisitor
    {
        public void Visit(MovementMessage m)
        {
            // TODO: Make sure that player can only control their character!
            
            Server.instance.TaskQueue.Enqueue(new Action(() =>
            {
                GameState.instance.PlayerMove(m.GetEntityId(),
                                              m.SequenceNumber,
                                              m.GetXCoordinate(), 
                                              m.GetYCoordinate(), 
                                              m.GetXVelocity(), 
                                              m.GetYVelocity());
            }));
        }

        public void Visit(AttackMessage m)
        {
            Debug.Log("Inside AttackMessage Visit");
            Server.instance.TaskQueue.Enqueue(new Action(() => {
                GameState.instance.PlayerAttack(m.GetEntityId(), m.GetWeaponType(), m.GetAttackPositionX(), m.GetAttackPositionY());
                Debug.Log(m);
            }));
            
        }

        public void Visit(EntityUpdateMessage m)
        {
            Debug.Log(m);
        }

        public void Visit(ItemPickupMessage m) {
            Entity item;
            if (GameState.instance.Entities.TryGetValue(m.GetPickupItemId(), out item)) {
                m.SetPickupConfirmed(1);
                UDPServer.getInstance().BroadcastMessage(m);
                GameState.instance.DestroyEntityID(m.GetPickupItemId());
            }
        }

        public void Visit(PlayerUpdateMessage m)
        {
            Debug.Log("Got player update message");
        }

        public void Visit(Message m) { Debug.Log(m); }
    }
}
