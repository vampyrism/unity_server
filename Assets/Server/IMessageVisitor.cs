using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Server
{
    public interface IMessageVisitor
    {
        void Visit(MovementMessage m);
        void Visit(AttackMessage m);
        void Visit(EntityUpdateMessage m);
        void Visit(Message m);
    }
}
