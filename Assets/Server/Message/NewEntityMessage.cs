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
            WEAPON_CROSSBOW
        }

        public enum Action : byte
        {
            CREATE,
            DELETE
        }

        public static readonly int MESSAGE_SIZE = 8;

        public static readonly int TYPE_ID = 0;
        public static readonly int ENTITY_TYPE = 1;
        public static readonly int ENTITY_ACTION = 3;
        public static readonly int ENTITY_ID = 4;

        private byte[] message = new byte[MESSAGE_SIZE];

        public EntityUpdateMessage(byte[] bytes)
        {
            bytes.CopyTo(this.message, 0);
        }

        public EntityUpdateMessage(byte[] bytes, int cursor)
        {
            Array.Copy(bytes, cursor, message, 0, MESSAGE_SIZE);
        }

        public EntityUpdateMessage(Type type, Action action, UInt32 id)
        {
            message[TYPE_ID] = ENTITY_UPDATE;
            SetEntityType(type);
            SetEntityAction(action);
            SetEntityID(id);
        }


        // Setters
        public void SetEntityType(Type type)
        {
            if(!Enum.IsDefined(typeof(Type), type))
            {
                throw new ArgumentOutOfRangeException("Entity type " + type + " does not exist");
            }

            Array.Copy(BitConverter.GetBytes((UInt16) type), 0, message, ENTITY_TYPE, 2);
        }

        public void SetEntityAction(Action action)
        {
            if (!Enum.IsDefined(typeof(Action), action))
            {
                throw new ArgumentOutOfRangeException("Entity action " + action + " is not implemented");
            }

            Array.Copy(BitConverter.GetBytes((byte) action), 0, message, ENTITY_ACTION, 1);
        }

        public void SetEntityID(UInt32 id)
        {
            Array.Copy(BitConverter.GetBytes(id), 0, message, ENTITY_ID, 4);
        }


        // Getters
        public UInt16 GetEntityType()
        {
            return BitConverter.ToUInt16(message, ENTITY_TYPE);
        }

        public byte GetEntityAction()
        {
            return message[ENTITY_ACTION];
        }

        public UInt32 GetEntityID()
        {
            return BitConverter.ToUInt32(message, ENTITY_ID);
        }

        public override byte[] Serialize() => this.message;

        public override int Size() => MESSAGE_SIZE;

        public override void Accept(IMessageVisitor v)
        {
            v.Visit(this);
        }
    }
}
