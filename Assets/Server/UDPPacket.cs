
@@ -0,0 +1,341 @@
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime;
using System.Security.Cryptography;

//
// This module handles serialization of the information sent over UDP between the game client and server.
// It implements the IDisposable interface in order to give greater control of when the object
// is released.
// 
// The UDPPacket design follows the following schema:
// 
// Bytes   | Description
// --------------------------
// 0-1     | Sequence number
// 2-3     | Entity ID
// 4       | Entity action type
// 5       | Entity action descriptor
// 6-9     | Entity X coordinate
// 10-13   | Entity Y coordinate
// 14-17   | Entity rotation
// 18-21   | Entity X velocity
// 22-25   | Entity Y velocity
//
// Multiple messages can be packed into the same packet, since the message size is far below the safe threshold
// of 508 bytes. If this threshold is surpassed the packet may be split up into several IP packages which is not ideal.
// 

namespace Assets.Server
{

    public enum ServerPacketTypes
    {
        TestPacket,
        PlayerMovement,
        AIMovement,
        PlayerVitals,
        AIVitals,
        PlayerDamage,
        AIDamange
    }

    public enum ClientPacketTypes
    {
        TestPacket,
        PlayerMovement,
        AIMovement,
        PlayerVitals,
        AIVitals,
        PlayerDamage,
        AIDamange
    }

    public class UDPPacket : IDisposable
    {

        // The payload should not exceed 508 bytes for the most reliable UDP communication.
        public static readonly int SAFE_PAYLOAD = 508;

        // Keep track of all the messages to serialize
        private List<Message> messages = new List<Message>();
        // Readable buffer for serialization
        private byte[] payload;

        private bool disposed = false;



        // Constructor

        public UDPPacket() {}

        public UDPPacket(byte[] bytes)
        {
            Deserialize(bytes);
        }

        // See if adding one more message would mean we're still below safe threshold
        public bool SafeToAdd()
        {
            return (this.Length() + 1) * Message.SCHEMA_SIZE < SAFE_PAYLOAD;
        }

        // Add new message to the packet
        public void AddMessage()
        {
            Message message = new Message();
            messages.Add(message);
        }

        public void AddMessage(byte[] bytes)
        {
            Message message = new Message(bytes);
            messages.Add(message);
        }

        public void AddMessage(short seqNum, short entityId, byte actionType, byte actionDescriptor, float x, float y, float r, float xd, float yd)
        {
            Message message = new Message(seqNum, entityId, actionType, actionDescriptor, x, y, r, xd, yd);
            messages.Add(message);
        }

        public Message ReadMessage(int index)
        {
            return messages[index];
        }

        public Message ReadMessage()
        {
            return ReadMessage(0);
        }

        // Returns the number of messages currently in packet.
        public int Length()
        {
            return messages.Count;
        }

        // Returns the packet messages as single byte array
        public byte[] Serialize()
        {
            payload = new byte[Message.SCHEMA_SIZE * this.Length()];
            int cursor = 0;
            // Iterate through all messages and add their serialization to the buffer
            foreach (Message message in messages)
            {
                Array.Copy(message.Serialize(), 0, payload, cursor, Message.SCHEMA_SIZE);
                cursor += Message.SCHEMA_SIZE;
            }

            return payload;
        }

        // Deserialize byte array into list of messages
        public void Deserialize(byte[] bytes)
        {
            int cursor = 0;
            int length = bytes.Length;
            byte[] message = new byte[Message.SCHEMA_SIZE];
            while (cursor < length)
            {
                Array.Copy(bytes, cursor, message, 0, Message.SCHEMA_SIZE);
                AddMessage(message);
                cursor += Message.SCHEMA_SIZE;
            }
        }

        public void Print()
        {
            foreach (Message message in messages)
            {
                Console.WriteLine("--------------------");
                Console.WriteLine(message.ToString());
            }

            Console.WriteLine("--------------------");
        }



        // Private class for making sure each message adheres to the schema.
        public class Message
        {
            // Total size of schema
            public static readonly int SCHEMA_SIZE = 26;

            // Indices for the values in the schema
            // Bytes   | Description
            // --------------------------
            //0-1 SenderID
            private static readonly int SENDER_ID = 0;
            // 0-1     | Sequence number
            private static readonly int SEQUENCE_NUMBER = 1;
            // 2-3     | Entity ID
            private static readonly int ENTITY_ID = 3;
            //
            private static readonly int PACKET_TYPE = 5;
            // 4       | Entity action type
            private static readonly int ACTION_TYPE = 6;
            // 5       | Entity action descriptor
            private static readonly int ACTION_DESCRIPTOR = 7;

            // Byte array containing the information
            private byte[] schema = new byte[SCHEMA_SIZE];

            // There are two constructors, one for mainly reading a packet and one for creating a packet

            public Message(byte[] bytes)
            {
                bytes.CopyTo(schema, 0);
            }

            //Adds header info necessary in all packets.
            public AddMessageHeader(short seqNum, short entityId, short packetType, byte actionType, byte actionDescriptor)
            {
                SetShort(seqNum, SEQUENCE_NUMBER);
                SetShort(entityId, ENTITY_ID);
                SetShort(packetType, PACKET_TYPE)
                SetBytes(actionType, ACTION_TYPE);
                SetBytes(actionDescriptor, ACTION_DESCRIPTOR);
            }

            

            // Create empty message
            public Message(){}

            // The Setters will convert the argument to bytes and copy them into the schema buffer
            public void SetShort(byte shortToSet, int index)
            {
                Array.Copy(BitConverter.GetBytes(ShortToSet), 0, schema, index, 2);
            }

            public void SetBytes(byte bytesToSet, int index)
            {
                Array.Copy(BitConverter.GetBytes(bytesToSet), 0, schema, index, 1);
            }
            
            public void SetFloat(float floatToSet, int index)
            {
                Array.Copy(BitConverter.GetBytes(floatToSet), 0, schema, index, 4);
            }
            


            //Get functions for reading packages
            public short GetShort(int readCursor) 
            {
                return BitConverter.ToInt16(schema, readCursor);
            }


            public short GetInt(int readCursor) 
            {
                return BitConverter.ToInt32(schema, readCursor);
            }

            public float GetFloat(int readCursor)
            {
                return BitConverter.ToInt32(schema, readCursor);
            }            

            public byte GetBytes(int readCursor)
            {
                return schema[readCursor];
            }


            public byte[] Serialize()
            {
                return schema;
            }

            public override string ToString()
            {
                string s = "";
                s += "Sequence nr\t" + GetSequenceNumber() + "\n";
                s += "Entity id  \t" + GetEntityId() + "\n";
                s += "Action type\t" + GetActionType() + "\n";
                s += "Action desc\t" + GetActionDescriptor() + "\n";
                s += "X Coord    \t" + GetXCoordinate() + "\n";
                s += "Y Coord    \t" + GetYCoordinate() + "\n";
                s += "Rotation   \t" + GetRotation() + "\n";
                s += "X Velocity \t" + GetXVelocity() + "\n";
                s += "Y Velocity \t" + GetYVelocity();

                return s;
            }
        }



        // Implementation of displosable interface



        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    messages = null;
                    payload = null;
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
} 