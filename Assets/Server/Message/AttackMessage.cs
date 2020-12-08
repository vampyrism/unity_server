
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
        public static readonly int MESSAGE_SIZE = 15;

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
        // 7-8     | Entity X coordinate
        private static readonly int TARGET_ENTITY_ID = 7;
        // 9   | Type of weapon to use
        private static readonly int WEAPON_TYPE = 9;
        // 12   | 0 is an invalid attack 1 is valid
        private static readonly int ATTACK_VALID = 10;
        // Byte array containing the information
        private static readonly int DAMAGE_AMOUNT = 11;


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

        public AttackMessage(short seqNum, UInt32 entityId, byte actionType, byte actionDescriptor, UInt32 targetEntityId, short weaponType, short attackValid, float damageAmount)
        {
            message[TYPE_ID] = ATTACK;
            SetSequenceNumber(seqNum);
            SetEntityId(entityId);
            SetActionType(actionType);
            SetActionDescriptor(actionDescriptor);
            SetTargetEntityId(targetEntityId);
            SetWeaponType(weaponType);
            SetAttackValid(attackValid);
            SetDamageAmount(damageAmount);
        }

        // The Setters will convert the argument to bytes and copy them into the message buffer

        public void SetSequenceNumber(short sn)
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

        public void SetTargetEntityId(UInt32 tid)
        {
            Array.Copy(BitConverter.GetBytes(tid), 0, message, TARGET_ENTITY_ID, 2);
        }
        public void SetWeaponType(short wid)
        {
            Array.Copy(BitConverter.GetBytes(wid), 0, message, WEAPON_TYPE, 1);
        }
        public void SetAttackValid(short avl)
        {
            Array.Copy(BitConverter.GetBytes(avl), 0, message, ATTACK_VALID, 1);
        }
        public void SetDamageAmount(float dmg)
        {
            Array.Copy(BitConverter.GetBytes(dmg), 0, message, DAMAGE_AMOUNT, 4);
        }


        // Getters 

        public short GetSequenceNumber() => BitConverter.ToInt16(message, SEQUENCE_NUMBER);

        public uint GetEntityId() => BitConverter.ToUInt32(message, ENTITY_ID);

        public byte GetActionType() => message[ACTION_TYPE];

        public byte GetActionDescriptor() => message[ACTION_DESCRIPTOR];
        public uint GetTargetEntityId() => BitConverter.ToUInt32(message, TARGET_ENTITY_ID);
        public short GetWeaponType() => BitConverter.ToInt16(message, WEAPON_TYPE);
        public short GetAttackValid() => BitConverter.ToInt16(message, ATTACK_VALID);
        public float GetDamageAmount() => BitConverter.ToSingle(message, DAMAGE_AMOUNT);

        public override byte[] Serialize() => message;

        public override int Size() => MESSAGE_SIZE;

        public override string ToString()
        {
            string s = "\n";
            s += "Sequence nr\t" + GetSequenceNumber() + "\n";
            s += "Entity id  \t" + GetEntityId() + "\n";
            s += "Action type\t" + GetActionType() + "\n";
            s += "Action desc\t" + GetActionDescriptor() + "\n";
            s += "Target Entity id    \t" + GetTargetEntityId() + "\n";
            s += "Weapon Type    \t" + GetWeaponType() + "\n";
            s += "Attack Validity   \t" + GetAttackValid() + "\n";
            s += "Damage Amount\t" + GetDamageAmount() + "\n";

            return s;
        }

        public override void Accept(IMessageVisitor v)
        {
            v.Visit(this);
        }
    }
}