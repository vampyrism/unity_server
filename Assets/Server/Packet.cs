using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace GameServer
{

    public class Packet : IDisposable
    {
        private List<byte> readBuffer;
        private byte[] sendBuffer;
        private int cursor;

//##################################################### Packet types ###########################################
        public enum hello
        {

        }


//the id is the type of packet we want to send
        public Packet(int id)
        {
            readBuffer = new List<byte>(); 
            cursor = 0;

            Write(id); 
        }


//####################################### Utility methods ##############################################
         //Could be done in the packet handler too i suppose
        public void AddPacketLength()
        {
            readBuffer.InsertRange(0, BitConverter.GetBytes(readBuffer.Count));        
        }

        public byte[] ToArray()
        {
            sendBuffer = readBuffer.ToArray();
            return sendBuffer;
        }

        public int Length()
        {
            return readBuffer.Count;        
        }

        public void ReUsePacket(bool pantaMera = true)
        {
            if (pantaMera)
            {
                readBuffer.Clear(); 
                sendBuffer = null;
                cursor = 0; 
            }
            else
            {
                cursor -= 4;            }
        }

//####################################### Writing different datatypes ##################################
        public void Write(int IntToWrite)
        {
            readBuffer.AddRange(BitConverter.GetBytes(IntToWrite));
        }

        public void Write(float FloatToWrite)
        {
            readBuffer.AddRange(BitConverter.GetBytes(FloatToWrite));
        }

        public void Write(string StringToWrite)
        {
           //Likely gonna need to write the length of the string first or last
           //otherwise you wont know where the string ends
            Write(StringToWrite.Length);
            readBuffer.AddRange(Encoding.ASCII.GetBytes(StringToWrite)); 
        }

        public void Write(bool BoolToWrite)
        {
            readBuffer.AddRange(BitConverter.GetBytes(BoolToWrite));
        }



//###################################### Reading different datatypes #####################################
        public byte[] ReadBytes(int length, bool cursor = true)
        {
            if (readBuffer.Count > cursor)
            {
                byte[] value = readBuffer.GetRange(cursor, length).ToArray(); 
                if (cursor)
                {
                    cursor += length;
                }
                return value;
            }
            else
            {
                throw new Exception("Vad fan gör du");
            }
        }


        public int ReadInt(bool cursor = true)
        {
            if (readBuffer.Count > cursor)
            {
                int value = BitConverter.ToInt32(sendBuffer, cursor); 
                if (cursor)
                {
                    cursor += 4; 
                }
                return value; 
            }
            else
            {
                throw new Exception("Int äre inte");
            }
        }
        public string ReadString(bool cursor = true)
        {
            try
            {
                int length = ReadInt();
                string value = Encoding.ASCII.GetString(sendBuffer, cursor, length); 
                if (cursor && value.Length > 0)
                {
                    cursor += length; 
                }
                return value;
            }
            catch
            {
                throw new Exception("Vafan gör du");
            }
        }


//############################################# Disposable interface methods ##############################################
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    buffer = null;
                    readableBuffer = null;
                    readPos = 0;
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