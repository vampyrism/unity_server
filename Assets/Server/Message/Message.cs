using System;
using System.Collections.Generic;

// This is an abstract class for implementing different kinds of messages.
// Can also deserialize a message and its' type from bytes. 

namespace Assets.Server 
{
    public abstract class Message
    {
        // There are several types of messages, denoted by byte at beginning of header
        protected static readonly byte MOVEMENT = 0;
        protected static readonly byte PICKUP = 1;
        protected static readonly byte ATTACK = 2;
        protected static readonly byte ENTITY_UPDATE = 3;
        protected static readonly byte STATE_UPDATE = 4;
        protected static readonly byte PLAYER_UPDATE = 5;

        // Factory dictionary for message types
        private static readonly Dictionary<byte, Func<byte[], int, Message>> typeConstructors = 
        new Dictionary<byte, Func<byte[], int, Message>>()
        {
            { MOVEMENT,         (byte[] bytes, int cursor) => new MovementMessage(bytes, cursor) },
            { PICKUP,           (byte[] bytes, int cursor) => new ItemPickupMessage(bytes, cursor) },
            { ATTACK,           (byte[] bytes, int cursor) => new AttackMessage(bytes, cursor) },
            { ENTITY_UPDATE,    (byte[] bytes, int cursor) => new EntityUpdateMessage(bytes, cursor) },
            { STATE_UPDATE,     (byte[] bytes, int cursor) => new StateUpdateMessage(bytes, cursor) },
            { PLAYER_UPDATE,    (byte[] bytes, int cursor) => new PlayerUpdateMessage(bytes, cursor) }
        };

        // Parse bytes and return appropriate Message type
        public static Message Deserialize(byte[] bytes, int cursor) 
        {
            byte type = bytes[cursor];
            return (Message) typeConstructors[type].DynamicInvoke(bytes, cursor);
        }

        public static Message Deserialize(byte[] bytes, int cursor, UInt16 seqnum)
        {
            byte type = bytes[cursor];
            Message m = (Message)typeConstructors[type].DynamicInvoke(bytes, cursor);
            m.SequenceNumber = seqnum;
            return m;
        }

        // Add packet sequence number when deserializing
        public UInt16 SequenceNumber;

        // Abstract

        // Return an efficient representation of the message as byte array
        public abstract byte[] Serialize();

        // Return size of the message in bytes
        public abstract int Size();

        public virtual void Accept(IMessageVisitor v)
        {
            throw new Exception("Visitor not implemented for class");
        }
    }
}