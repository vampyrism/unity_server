
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime;
using System.Security.Cryptography;

namespace Assets.Server 
{
    public class MovementMessage : Message
    {
        // Total size of schema
        public static readonly int MESSAGE_SIZE = 26;

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
        private byte[] schema = new byte[MESSAGE_SIZE];

        public MovementMessage(byte[] bytes)
        {
            bytes.CopyTo(schema, 0);
        }

        public MovementMessage(byte[] bytes, int cursor)
        {
            Array.Copy(bytes, cursor, schema, 0, MESSAGE_SIZE);
        }

        public MovementMessage(short seqNum, short entityId, byte actionType, byte actionDescriptor, float x, float y, float r, float xd, float yd)
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

        public short GetSequenceNumber() => BitConverter.ToInt16(schema, SEQUENCE_NUMBER);

        public short GetEntityId() => BitConverter.ToInt16(schema, ENTITY_ID);

        public byte GetActionType() => schema[ACTION_TYPE];

        public byte GetActionDescriptor() => schema[ACTION_DESCRIPTOR];

        public float GetXCoordinate() => BitConverter.ToSingle(schema, X_COORDINATE);

        public float GetYCoordinate() => BitConverter.ToSingle(schema, Y_COORDINATE);

        public float GetRotation() => BitConverter.ToSingle(schema, ROTATION);

        public float GetXVelocity() => BitConverter.ToSingle(schema, X_VELOCITY);

        public float GetYVelocity() => BitConverter.ToSingle(schema, Y_VELOCITY);

        public override byte[] Serialize() => schema;

        public override int Size() => MESSAGE_SIZE;

        public override string ToString()
        {
            string s = "\n";
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
}