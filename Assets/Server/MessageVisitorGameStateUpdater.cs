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
            UDPServer.getInstance().BroadcastMessage(m);
        }

        public void Visit(AttackMessage m)
        {
            Debug.Log(m);
        }

        public void Visit(EntityUpdateMessage m)
        {
            Debug.Log(m);
        }

        public void Visit(Message m) { Debug.Log(m); }
    }
}
