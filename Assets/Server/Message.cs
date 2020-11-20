
using System;
using System.Collections.Generic;

// Private class for making sure each message adheres to the schema.
namespace Assets.Server 
{
    public abstract class Message
    {
        // There are several types of messages, denoted by byte at beginning of header
        protected static readonly byte MOVEMENT = 0;
        protected static readonly byte PICKUP = 1;
        protected static readonly byte ATTACK = 2;

        private static readonly Dictionary<byte, Func<byte[], int, Message>> typeConstructors = 
        new Dictionary<byte, Func<byte[], int, Message>>()
        {
            { MOVEMENT, (byte[] bytes, int cursor) => new MovementMessage(bytes, cursor) }
        };

        // Parse bytes in order and return appropriate Message type
        public static Message Deserialize(byte[] bytes, int cursor) 
        {
            byte type = bytes[cursor];
            return (Message) typeConstructors[type].DynamicInvoke(bytes, cursor);
        }

        // Abstract

        // Size of message type in bytes

        // Return an efficient representation of the message as byte array
        public abstract byte[] Serialize();

        // Return size of the message in bytes
        public abstract int Size();
    }
}