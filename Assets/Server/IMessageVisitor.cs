using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Server
{
    public interface IMessageVisitor
    {
        public void Visit(MovementMessage m);
        public void Visit(AttackMessage m);
    }
}
