
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime;
using System.Security.Cryptography;
using UnityEngine;

//
// This module handles serialization of the information sent over UDP between the game client and server.
// It implements the IDisposable interface in order to give greater control of when the object
// is released.
// 
// Multiple messages can be packed into the same packet, since the message size is far below the safe threshold
// of 508 bytes. If this threshold is surpassed the packet may be split up into several IP packages which is not ideal.
// 

namespace Assets.Server
{

    public class UDPPacket
    {

        // The payload should not exceed 508 bytes for the most reliable UDP communication.
        public static readonly int SAFE_PAYLOAD = 508;

        // Keep track of all the messages to serialize  
        private List<Message> messages = new List<Message>();

        // Readable buffer for serialization
        private byte[] payload;
        
        // Total size of all messages in list
        public int Size { get; private set; } = 0;

        // Constructor

        public UDPPacket() {}

        public UDPPacket(byte[] bytes)
        {
            Deserialize(bytes);
        }

        // See if adding one more message would mean we're still below safe threshold
        public bool SafeToAdd(int size)
        {
            return this.Size + size < SAFE_PAYLOAD; 
        }

        /// <summary>
        /// Returns size of unused space in <c>UDPPacket</c>
        /// </summary>
        /// <returns>Space left in packet</returns>
        public int SizeLeft()
        {
            return SAFE_PAYLOAD - this.Size;
        }

        public void AddMessage(Message message)
        {
            messages.Add(message);
            Size += message.Size();
        }

        public Message GetMessage(int index)
        {
            return messages[index];
        }

        public Message GetMessage()
        {
            return GetMessage(0);
        }

        public List<Message> GetMessages()
        {
            return messages;
        }

        // Returns the number of messages currently in packet.
        public int Length()
        {
            return messages.Count;
        }

        // Returns the packet messages as single byte array
        public byte[] Serialize()
        {
            payload = new byte[Size];
            int cursor = 0;
            // Iterate through all messages and add their serialization to the buffer
            foreach (Message message in messages)
            {
                Array.Copy(message.Serialize(), 0, payload, cursor, message.Size());
                cursor += message.Size();
            }

            return payload;
        }

        // Deserialize byte array into list of messages
        public List<Message> Deserialize(byte[] bytes)
        {
            int cursor = 0;
            int length = bytes.Length;
            while (cursor < length)
            {
                Message message = Message.Deserialize(bytes, cursor);
                AddMessage(message);
                cursor += message.Size();
            }

            return messages;
        }

        public void Print()
        {
            foreach (Message message in messages)
            {
                Debug.Log("--------------------");
                Debug.Log(message.ToString());
            }

            Debug.Log("--------------------");
        }
    }
} 