using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Server 
{
    public class StateUpdateMessage : Message
    {
            /*
             * This message handles global updates of the game state, 
             * i.e. switches in day/night, game over, etc.
             * 
             * Bytes    | Type      | Description
             * ----------------------------------
             * 0        | byte      | Message type
             * 1        | byte      | Update type
             * 2        | byte      | Update descriptor
             */

            public enum Type : byte
            {
                DAY_NIGHT,
                GAME_OVER,
            }

            public enum Descriptor : byte
            {
                DAY,
                NIGHT,
            }

            public static readonly int MESSAGE_SIZE = 3;

            public static readonly int TYPE_ID = 0;
            public static readonly int UPDATE_TYPE = 1;
            public static readonly int UPDATE_DESCRIPTOR = 2;

            private byte[] message = new byte[MESSAGE_SIZE];

            public StateUpdateMessage(byte[] bytes)
            {
                bytes.CopyTo(this.message, 0);
            }

            public StateUpdateMessage(byte[] bytes, int cursor)
            {
                Array.Copy(bytes, cursor, message, 0, MESSAGE_SIZE);
            }

            public StateUpdateMessage(Type type, Descriptor descriptor)
            {
                message[TYPE_ID] = STATE_UPDATE;
                SetUpdateType(type);
                SetUpdateDescriptor(descriptor);
            }


            // Setters
            public void SetUpdateType(Type type)
            {
                this.message[UPDATE_TYPE] = (byte) type;
            }

            public void SetUpdateDescriptor(Descriptor descriptor)
            {
                this.message[UPDATE_DESCRIPTOR] = (byte) descriptor;
            }


            // Getters
            public Type GetUpdateType()
            {
                return (Type) this.message[UPDATE_TYPE];
            }

            public Descriptor GetUpdateDescriptor()
            {
                return (Descriptor) this.message[UPDATE_DESCRIPTOR];
            }


            public override byte[] Serialize() => this.message;

            public override int Size() => MESSAGE_SIZE;

            public override string ToString()
            {
                string s = "";
                s += "State Update Message\n";
                s += "Update type \t" + GetUpdateType() + "\n";
                s += "Update descr \t" + GetUpdateDescriptor() + "\n";

                return s;
            }

            public override void Accept(IMessageVisitor v)
            {
                v.Visit(this);
            }
    }
}
