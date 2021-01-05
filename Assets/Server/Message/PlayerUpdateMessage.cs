using System;

namespace Assets.Server
{
    public class PlayerUpdateMessage : Message
    {
        public enum Type : byte
        {
            PLAYERNAME
        }

        public static int MESSAGE_SIZE = 6;

        public static readonly int TYPE_ID = 0;
        public static readonly int UPDATE_TYPE = 1;
        public static readonly int UPDATE_DATA_LENGTH = 2;
        public static readonly int UPDATE_DATA = 6;

        private byte[] message;

        public PlayerUpdateMessage(byte[] bytes)
        {
            throw new Exception("Can't create PlayerUpdateMessage from bytearray");
        }

        public PlayerUpdateMessage(byte[] bytes, int cursor)
        {

            int len = BitConverter.ToInt32(bytes, cursor + UPDATE_DATA_LENGTH);
            message = new byte[MESSAGE_SIZE + len];
            Array.Copy(bytes, cursor, message, 0, MESSAGE_SIZE + len);
        }

        public PlayerUpdateMessage(Type type, string name)
        {
            message = new byte[MESSAGE_SIZE + System.Text.Encoding.UTF8.GetBytes(name).Length];
            message[TYPE_ID] = PLAYER_UPDATE;
            BitConverter.GetBytes(System.Text.Encoding.UTF8.GetBytes(name).Length).CopyTo(message, UPDATE_DATA_LENGTH);
            SetUpdateType(type);
            SetPlayerName(name);
        }


        // Setters
        public void SetUpdateType(Type type)
        {
            this.message[UPDATE_TYPE] = (byte)type;
        }

        public void SetPlayerName(string name)
        {
            System.Text.Encoding.UTF8.GetBytes(name).CopyTo(this.message, UPDATE_DATA);
        }

        // Getters
        public Type GetUpdateType()
        {
            return (Type)this.message[UPDATE_TYPE];
        }

        public int GetDataLength()
        {
            return BitConverter.ToInt32(this.message, UPDATE_DATA_LENGTH);
        }

        public string GetPlayerName()
        {
            return System.Text.Encoding.UTF8.GetString(this.message, UPDATE_DATA, GetDataLength());
        }


        public override byte[] Serialize() => this.message;

        public override int Size() => MESSAGE_SIZE + GetDataLength();

        public override string ToString()
        {
            string s = "";
            s += "State Update Message\n";
            s += "Update type \t" + GetUpdateType() + "\n";
            s += "Data omitted.";

            return s;
        }

        public override void Accept(IMessageVisitor v)
        {
            v.Visit(this);
        }
    }
}
