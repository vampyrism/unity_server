using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Server
{
    abstract class GameStateHandler
    {
        void Update(List<Message> messages)
        {
            foreach(dynamic message in messages)
            {
                Handle(message);
            }
        }

        void Handle(MovementMessage message)
        {

        }

        void Handle(AttackMessage message)
        {

        }
    }
}
