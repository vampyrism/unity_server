
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

namespace GameServer
{

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
            // 0-1     | Sequence number
            private static readonly int SEQUENCE_NUMBER = 0;
            // 2-3     | Entity ID
            private static readonly int ENTITY_ID = 2;
            // 4       | Entity action type
            private static readonly int ACTION_TYPE = 4;
            // 5       | Entity action descriptor
            private static readonly int ACTION_DESCRIPTOR = 5;
            // 6-9     | Entity X coordinate
            private static readonly int X_COORDINATE = 6;
            // 10-13   | Entity Y coordinate
            private static readonly int Y_COORDINATE = 10;
            // 14-17   | Entity rotation
            private static readonly int ROTATION = 14;
            // 18-21   | Entity X velocity
            private static readonly int X_VELOCITY = 18;
            // 22-25   | Entity Y velocity
            private static readonly int Y_VELOCITY = 22;

            // Byte array containing the information
            private byte[] schema = new byte[SCHEMA_SIZE];

            // There are two constructors, one for mainly reading a packet and one for creating a packet

            public Message(byte[] bytes)
            {
                bytes.CopyTo(schema, 0);
            }

            public Message(short seqNum, short entityId, byte actionType, byte actionDescriptor, float x, float y, float r, float xd, float yd)
            {
                SetSequenceNumber(seqNum);
                SetEntityId(entityId);
                SetActionType(actionType);
                SetActionDescriptor(actionDescriptor);
                SetXCoordinate(x);
                SetYCoordinate(y);
                SetRotation(r);
                SetXVelocity(xd);
                SetYVelocity(yd);
            }

            // Create empty message
            public Message(){}

            // The Setters will convert the argument to bytes and copy them into the schema buffer

            public void SetSequenceNumber(short sn)
            {
                Array.Copy(BitConverter.GetBytes(sn), 0, schema, SEQUENCE_NUMBER, 2);
            }

            public void SetEntityId(short ei)
            {
                Array.Copy(BitConverter.GetBytes(ei), 0, schema, ENTITY_ID, 2);
            }

            public void SetActionType(byte at)
            {
                Array.Copy(BitConverter.GetBytes(at), 0, schema, ACTION_TYPE, 1);
            }

            public void SetActionDescriptor(byte ad)
            {
                Array.Copy(BitConverter.GetBytes(ad), 0, schema, ACTION_DESCRIPTOR, 1);
            }

            public void SetXCoordinate(float x)
            {
                Array.Copy(BitConverter.GetBytes(x), 0, schema, X_COORDINATE, 4);
            }

            public void SetYCoordinate(float y)
            {
                Array.Copy(BitConverter.GetBytes(y), 0, schema, Y_COORDINATE, 4);
            }

            public void SetRotation(float r)
            {
                Array.Copy(BitConverter.GetBytes(r), 0, schema, ROTATION, 4);
            }

            public void SetXVelocity(float xd)
            {
                Array.Copy(BitConverter.GetBytes(xd), 0, schema, X_VELOCITY, 4);
            }

            public void SetYVelocity(float yd)
            {
                Array.Copy(BitConverter.GetBytes(yd), 0, schema, Y_VELOCITY, 4);
            }

            // Getters 

            public short GetSequenceNumber()
            {
                return BitConverter.ToInt16(schema, SEQUENCE_NUMBER);
            }

            public short GetEntityId()
            {
                return BitConverter.ToInt16(schema, ENTITY_ID);
            }

            public byte GetActionType()
            {
                return schema[ACTION_TYPE];
            }

            public byte GetActionDescriptor()
            {
                return schema[ACTION_DESCRIPTOR];
            }

            public float GetXCoordinate()
            {
                return BitConverter.ToSingle(schema, X_COORDINATE);
            }

            public float GetYCoordinate()
            {
                return BitConverter.ToSingle(schema, Y_COORDINATE);
            }

            public float GetRotation()
            {
                return BitConverter.ToSingle(schema, ROTATION);
            }

            public float GetXVelocity()
            {
                return BitConverter.ToSingle(schema, X_VELOCITY);
            }

            public float GetYVelocity()
            {
                return BitConverter.ToSingle(schema, Y_VELOCITY);
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