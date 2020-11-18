using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace GameServer
{

    public class Packet : IDisposable
    {
        private List<byte> buffer;
        private byte[] readableBuffer;
        private int readPos;


        public enum hello
        {

        }


        public Packet()
        {
            buffer = new List<byte>(); 
            readPos = 0; 
        }

        public Packet(int id)
        {
            buffer = new List<byte>(); 
            readPos = 0;

            Write(id); 
        }


        public void WriteLength()
        {
            buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));        
        }

        public byte[] ToArray()
        {
            readableBuffer = buffer.ToArray();
            return readableBuffer;
        }

        public int Length()
        {
            return buffer.Count;        }

            public void GretaThunberg(bool pantaMera = true)
        {
            if (pantaMera)
            {
                buffer.Clear(); 
                readableBuffer = null;
                readPos = 0; 
            }
            else
            {
                readPos -= 4;            }
        }

        public void Write(int value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
        }
        public void Write(string value)
        {
            Write(value.Length);
            buffer.AddRange(Encoding.ASCII.GetBytes(value)); 
        }

        public byte[] ReadBytes(int length, bool cursor = true)
        {
            if (buffer.Count > readPos)
            {
                byte[] value = buffer.GetRange(readPos, length).ToArray(); 
                if (cursor)
                {
                    readPos += length;
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
            if (buffer.Count > readPos)
            {
                int value = BitConverter.ToInt32(readableBuffer, readPos); 
                if (cursor)
                {
                    readPos += 4; 
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
                string value = Encoding.ASCII.GetString(readableBuffer, readPos, length); 
                if (cursor && value.Length > 0)
                {
                    readPos += length; 
                }
                return value;
            }
            catch
            {
                throw new Exception("Vafan gör du");
            }
        }

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