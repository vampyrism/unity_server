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
        public static readonly int MESSAGE_SIZE = 23;

        // Indices for the values in the message
        // Bytes   | Description
        // --------------------------
        // 0       | Type ID
        private static readonly int TYPE_ID = 0;
        // 3-4     | Entity ID
        private static readonly int ENTITY_ID = 1;
        // 5       | Entity action type
        private static readonly int ACTION_TYPE = 3;
        // 6       | Entity action descriptor
        private static readonly int ACTION_DESCRIPTOR = 4;
        // 7-8     | Entity X coordinate
        private static readonly int TARGET_ENTITY_ID = 5;
        // 9   | Type of weapon to use
        private static readonly int WEAPON_TYPE = 7;
        // 10   | 0 is an invalid attack 1 is valid
        private static readonly int ATTACK_VALID = 8;
        // 11-14 | Attack damage amount
        private static readonly int DAMAGE_AMOUNT = 9;
        // 15-18  | Attack vector direction
        private static readonly int ATTACK_POSITION_X = 13;
        // 19-22  | Attack vector direction
        private static readonly int ATTACK_POSITION_Y = 17;
        // 23  | Attack initiated 0 for false 1 for true
        private static readonly int ATTACK_INITIATED = 21;
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

        public AttackMessage(UInt32 entityId, byte actionType, byte actionDescriptor, UInt32 targetEntityId, short weaponType, short attackValid, float damageAmount, float attackPositionX, float attackPositionY, short attackInit)
        {
            message[TYPE_ID] = ATTACK;
            SetEntityId(entityId);
            SetActionType(actionType);
            SetActionDescriptor(actionDescriptor);
            SetTargetEntityId(targetEntityId);
            SetWeaponType(weaponType);
            SetAttackValid(attackValid);
            SetDamageAmount(damageAmount);
            SetAttackPositionX(attackPositionX);
            SetAttackPositionY(attackPositionY);
            SetAttackInitiated(attackInit);
        }

        // The Setters will convert the argument to bytes and copy them into the message buffer

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
        public void SetAttackPositionX(float posX)
        {
            Array.Copy(BitConverter.GetBytes(posX), 0, message, ATTACK_POSITION_X, 4);
        }
        public void SetAttackPositionY(float posY)
        {
            Array.Copy(BitConverter.GetBytes(posY), 0, message, ATTACK_POSITION_Y, 4);
        }
        public void SetAttackInitiated(short ati)
        {
            Array.Copy(BitConverter.GetBytes(ati), 0, message, ATTACK_INITIATED, 2);
        }

        // Getters 

        public uint GetEntityId() => BitConverter.ToUInt32(message, ENTITY_ID);
        public byte GetActionType() => message[ACTION_TYPE];
        public byte GetActionDescriptor() => message[ACTION_DESCRIPTOR];
        public uint GetTargetEntityId() => BitConverter.ToUInt32(message, TARGET_ENTITY_ID);
        public short GetWeaponType() => BitConverter.ToInt16(message, WEAPON_TYPE);
        public short GetAttackValid() => BitConverter.ToInt16(message, ATTACK_VALID);
        public float GetDamageAmount() => BitConverter.ToSingle(message, DAMAGE_AMOUNT);
        public float GetAttackPositionX() => BitConverter.ToSingle(message, ATTACK_POSITION_X);
        public float GetAttackPositionY() => BitConverter.ToSingle(message, ATTACK_POSITION_Y);
        public short GetAttackInitiated() => BitConverter.ToInt16(message, ATTACK_INITIATED);
        public override byte[] Serialize() => message;

        public override int Size() => MESSAGE_SIZE;

        public override string ToString()
        {
            string s = "\n";
            s += "Entity id  \t" + GetEntityId() + "\n";
            s += "Action type\t" + GetActionType() + "\n";
            s += "Action desc\t" + GetActionDescriptor() + "\n";
            s += "Target Entity id    \t" + GetTargetEntityId() + "\n";
            s += "Weapon Type    \t" + GetWeaponType() + "\n";
            s += "Attack Validity   \t" + GetAttackValid() + "\n";
            s += "Damage Amount\t" + GetDamageAmount() + "\n";
            s += "Attack direction x\t" + GetAttackPositionX() + "\n";
            s += "Attack Direction y\t" + GetAttackPositionY() + "\n";
            s += "Attack Initiated\t" + GetAttackInitiated() + "\n";
            return s;
        }

        public override void Accept(IMessageVisitor v)
        {
            v.Visit(this);
        }
    }
}