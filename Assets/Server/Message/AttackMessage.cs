
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
    public class AttackMessage : Message
    {
        // Total size of message
        public static readonly int MESSAGE_SIZE = 26;

        // Indices for the values in the message
        // Bytes   | Description
        // --------------------------
        // 0       | Type ID
        private static readonly int TYPE_ID = 0;
        // 1-2     | Sequence number
        private static readonly int SEQUENCE_NUMBER = 1;
        // 3-4     | Entity ID
        private static readonly int ENTITY_ID = 3;
        // 5       | Entity action type
        private static readonly int ACTION_TYPE = 5;
        // 6       | Entity action descriptor
        private static readonly int ACTION_DESCRIPTOR = 6;
        // 7-10     | Entity X coordinate
        private static readonly int X_COORDINATE = 7;
        // 11-14   | Entity Y coordinate
        private static readonly int Y_COORDINATE = 11;
        // 15-18   | Entity rotation
        private static readonly int ROTATION = 15;
        // 19-22   | Entity X velocity
        private static readonly int X_VELOCITY = 19;
        // 23-26   | Entity Y velocity
        private static readonly int Y_VELOCITY = 23;

        // Byte array containing the information
        private byte[] message = new byte[MESSAGE_SIZE];

        public AttackMessage(byte[] bytes)
        {
            bytes.CopyTo(message, 0);
            message[TYPE_ID] = ATTACK;
        }

        public AttackMessage(byte[] bytes, int cursor)
        {
            Array.Copy(bytes, cursor, message, 0, MESSAGE_SIZE);
            message[TYPE_ID] = ATTACK;
        }

        public AttackMessage(short seqNum, short entityId, byte actionType, byte actionDescriptor, float x, float y, float r, float xd, float yd)
        {
            message[TYPE_ID] = ATTACK;
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

        // The Setters will convert the argument to bytes and copy them into the message buffer

        public void SetSequenceNumber(short sn)
        {
            Array.Copy(BitConverter.GetBytes(sn), 0, message, SEQUENCE_NUMBER, 2);
        }

        public void SetEntityId(short ei)
        {
            Array.Copy(BitConverter.GetBytes(ei), 0, message, ENTITY_ID, 2);
        }

        public void SetActionType(byte at)
        {
            Array.Copy(BitConverter.GetBytes(at), 0, message, ACTION_TYPE, 1);
        }

        public void SetActionDescriptor(byte ad)
        {
            Array.Copy(BitConverter.GetBytes(ad), 0, message, ACTION_DESCRIPTOR, 1);
        }

        public void SetXCoordinate(float x)
        {
            Array.Copy(BitConverter.GetBytes(x), 0, message, X_COORDINATE, 4);
        }

        public void SetYCoordinate(float y)
        {
            Array.Copy(BitConverter.GetBytes(y), 0, message, Y_COORDINATE, 4);
        }

        public void SetRotation(float r)
        {
            Array.Copy(BitConverter.GetBytes(r), 0, message, ROTATION, 4);
        }

        public void SetXVelocity(float xd)
        {
            Array.Copy(BitConverter.GetBytes(xd), 0, message, X_VELOCITY, 4);
        }

        public void SetYVelocity(float yd)
        {
            Array.Copy(BitConverter.GetBytes(yd), 0, message, Y_VELOCITY, 4);
        }

        // Getters 

        public short GetSequenceNumber() => BitConverter.ToInt16(message, SEQUENCE_NUMBER);

        public short GetEntityId() => BitConverter.ToInt16(message, ENTITY_ID);

        public byte GetActionType() => message[ACTION_TYPE];

        public byte GetActionDescriptor() => message[ACTION_DESCRIPTOR];

        public float GetXCoordinate() => BitConverter.ToSingle(message, X_COORDINATE);

        public float GetYCoordinate() => BitConverter.ToSingle(message, Y_COORDINATE);

        public float GetRotation() => BitConverter.ToSingle(message, ROTATION);

        public float GetXVelocity() => BitConverter.ToSingle(message, X_VELOCITY);

        public float GetYVelocity() => BitConverter.ToSingle(message, Y_VELOCITY);

        public override byte[] Serialize() => message;

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

        public override void Accept(IMessageVisitor v)
        {
            v.Visit(this);
        }
    }
}