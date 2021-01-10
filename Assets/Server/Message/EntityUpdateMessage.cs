using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Server
{
    public class EntityUpdateMessage : Message
    {
        /*
         * Bytes    | Type      | Description
         * ----------------------------------
         * 0        | byte      | Message type
         * 1-2      | uint16    | Entity type (e.g. 0 is current player)
         * 3        | byte      | Entity action (see Action)
         * 4        | uint32    | Entity ID
         */

        public enum Type : UInt16
        {
            PLAYER,
            ENEMY,
            WEAPON_CROSSBOW,
            WEAPON_BOW
        }

        public enum Action : byte
        {
            CREATE,
            DELETE,
            CONTROL,
            HP_UPDATE
        }

        public static readonly int MESSAGE_SIZE = 8; // TODO: Wrong!

        public static readonly int TYPE_ID = 0;
        public static readonly int ENTITY_TYPE = 1;
        public static readonly int ENTITY_ACTION = 3;
        public static readonly int ENTITY_ID = 4;
        public static readonly int ENTITY_HP = 5;

        private byte[] message = new byte[MESSAGE_SIZE];

        public EntityUpdateMessage(byte[] bytes)
        {
            bytes.CopyTo(this.message, 0);
        }

        public EntityUpdateMessage(byte[] bytes, int cursor)
        {
            Array.Copy(bytes, cursor, message, 0, MESSAGE_SIZE);
        }

        public EntityUpdateMessage(Type type, Action action, UInt32 id, float hp)
        {
            message[TYPE_ID] = ENTITY_UPDATE;
            SetEntityType(type);
            SetEntityAction(action);
            SetEntityID(id);
            SetEntityHP(hp);
        }


        // Setters
        public void SetEntityType(Type type)
        {
            if (!Enum.IsDefined(typeof(Type), type))
            {
                throw new ArgumentOutOfRangeException("Entity type " + type + " does not exist");
            }

            Array.Copy(BitConverter.GetBytes((UInt16)type), 0, message, ENTITY_TYPE, 2);
        }

        public void SetEntityAction(Action action)
        {
            if (!Enum.IsDefined(typeof(Action), action))
            {
                throw new ArgumentOutOfRangeException("Entity action " + action + " is not implemented");
            }

            Array.Copy(BitConverter.GetBytes((byte)action), 0, message, ENTITY_ACTION, 1);
        }

        public void SetEntityID(UInt32 id)
        {
            Array.Copy(BitConverter.GetBytes(id), 0, message, ENTITY_ID, 4);
        }

        public void SetEntityHP(float hp)
        {
            Array.Copy(BitConverter.GetBytes(hp), 0, message, ENTITY_HP, 4);
        }

        // Getters
        public Type GetEntityType()
        {
            return (Type)BitConverter.ToUInt16(message, ENTITY_TYPE);
        }

        public Action GetEntityAction()
        {
            return (Action)message[ENTITY_ACTION];
        }

        public UInt32 GetEntityID()
        {
            return BitConverter.ToUInt32(message, ENTITY_ID);
        }

        public UInt32 GetEntityHP()
        {
            return BitConverter.ToUInt16(message, ENTITY_HP);
        }
        public override byte[] Serialize() => this.message;

        public override int Size() => MESSAGE_SIZE;

        public override string ToString()
        {
            string s = "";
            s += "Entity Update Message\n";
            s += "Entity type \t" + Enum.GetName(typeof(Type), GetEntityType()) + "\n";
            s += "Entity action \t" + Enum.GetName(typeof(Action), GetEntityAction()) + "\n";
            s += "Entity ID \t" + GetEntityID();
            s += "Entity HP \t" + GetEntityHP();

            return s;
        }

        public override void Accept(IMessageVisitor v)
        {
            v.Visit(this);
        }
    }
}
