using System;
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
            //Debug.Log(m);
            // TODO: Make sure that player can only control their character!
            
            Server.instance.TaskQueue.Enqueue(new Action(() =>
            {
                /*Debug.Log("Moving entity " + m.GetEntityId() + " to " + 
                            m.GetXCoordinate() + " " + 
                            m.GetYCoordinate() + " " + 
                            m.GetXVelocity() + " " + 
                            m.GetYVelocity());*/
            
                GameState.instance.PlayerMove(m.GetEntityId(),
                                              m.GetSequenceNumber(),
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
                GameState.instance.PlayerAttack(m.GetEntityId(), m.GetTargetEntityId(), m.GetWeaponType(), m.GetAttackDirectionX(), m.GetAttackDirectionY());
                Debug.Log(m);
            }));
            
        }

        public void Visit(EntityUpdateMessage m)
        {
            Debug.Log(m);
        }

        public void Visit(Message m) { Debug.Log(m); }

        public void Visit(ItemPickupMessage m)
        {
            GameState.instance.ItemPickup(m.GetEntityId(), m.GetPickupItemId());
            Debug.Log(m);
        }
    }
}
