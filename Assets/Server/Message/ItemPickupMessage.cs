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
        public class ItemPickupMessage : Message
        {
            // Total size of message
            public static readonly int MESSAGE_SIZE = 29;

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
            private static readonly int ACTION_TYPE = 7;
            // 6       | Entity action descriptor
            private static readonly int ACTION_DESCRIPTOR = 8;
            // 7-10     | Entity X coordinate
            private static readonly int PICKUP_ITEM_ID = 9;
            // 11-14   | Entity Y coordinate
            private static readonly int PICKUP_CONFIRMED = 13;

            // Byte array containing the information
            private byte[] message = new byte[MESSAGE_SIZE];

            public ItemPickupMessage(byte[] bytes)
            {
                bytes.CopyTo(message, 0);
            }

            public ItemPickupMessage(byte[] bytes, int cursor)
            {
                Array.Copy(bytes, cursor, message, 0, MESSAGE_SIZE);
            }

            public ItemPickupMessage(ushort seqNum, UInt32 entityId, byte actionType, byte actionDescriptor, UInt32 itemId, ushort pickupConfirmed)
            {
                message[TYPE_ID] = MOVEMENT;
                SetSequenceNumber(seqNum);
                SetEntityId(entityId);
                SetActionType(actionType);
                SetActionDescriptor(actionDescriptor);
                SetPickupItemId(itemId);
                SetPickupConfirmed(pickupConfirmed);
            }

            // The Setters will convert the argument to bytes and copy them into the message buffer

            public void SetSequenceNumber(ushort sn)
            {
                Array.Copy(BitConverter.GetBytes(sn), 0, message, SEQUENCE_NUMBER, 2);
            }

            public void SetEntityId(UInt32 ei)
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

            public void SetPickupItemId(UInt32 itd)
            {
                Array.Copy(BitConverter.GetBytes(itd), 0, message, PICKUP_ITEM_ID, 4);
            }

            public void SetPickupConfirmed(ushort puc)
            {
                Array.Copy(BitConverter.GetBytes(puc), 0, message, PICKUP_CONFIRMED, 1);
            }

            // Getters 

            public ushort GetSequenceNumber() => BitConverter.ToUInt16(message, SEQUENCE_NUMBER);

            public UInt32 GetEntityId() => BitConverter.ToUInt32(message, ENTITY_ID);

            public byte GetActionType() => message[ACTION_TYPE];

            public byte GetActionDescriptor() => message[ACTION_DESCRIPTOR];

            public UInt32 GetPickupItemId() => BitConverter.ToUInt32(message, PICKUP_ITEM_ID);

            public ushort GetPickupConfirmed() => BitConverter.ToUInt16(message, PICKUP_CONFIRMED);
            public override byte[] Serialize() => message;

            public override int Size() => MESSAGE_SIZE;

            public override string ToString()
            {
                string s = "\n";
                s += "Sequence nr\t" + GetSequenceNumber() + "\n";
                s += "Entity id  \t" + GetEntityId() + "\n";
                s += "Action type\t" + GetActionType() + "\n";
                s += "Action desc\t" + GetActionDescriptor() + "\n";
                s += "X Coord    \t" + GetPickupItemId() + "\n";
                s += "Y Coord    \t" + GetPickupConfirmed() + "\n";

                return s;
            }

            public override void Accept(IMessageVisitor v)
            {
                v.Visit(this);
            }
        }
    }
