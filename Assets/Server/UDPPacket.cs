
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
using System.Collections;

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
    /// <summary>
    /// Used for storing the ACK datastructure in UDPServer/Client
    /// </summary>
    public struct UDPAckPacket
    {
        public bool Acked;
        public double SendTime;
        public UDPPacket Packet;
    }

    public class UDPPacket
    {

        // The payload should not exceed 508 bytes for the most reliable UDP communication.
        public static readonly int SAFE_PAYLOAD = 508;

        // Keep track of all the messages to serialize  
        private List<Message> messages = new List<Message>();

        
        /*
         * Packet structure
         * 
         * BitArray AckArray        (4B) Acks up to AckNumber-1
         * UInt16 AckNumber         (2B) Last packet to be acknowledged
         * UInt16 SequenceNumber    (2B) UDPPacket sequence number
         * List<Message>            (?B) List of messages
         * 
         */
        
        // Readable buffer for serialization
        private byte[] payload;


        private BitArray AckArray { get; set; } = new BitArray(32);
        public UInt16 AckNumber { get; private set; }
        public UInt16 SequenceNumber { get; private set; }
        
        // Total size of all messages in list
        // Begin at 8 due to packet header (ack+seq)
        public int Size { get; private set; } = 8;

        // Constructor
        /// <summary>
        /// Creates a new <c>UDPPacket</c>
        /// </summary>
        /// <param name="sequence_number">Local sequence number</param>
        /// <param name="ack_number">Remote sequence number (i.e. last received seq from remote)</param>
        public UDPPacket(UInt16 sequence_number, UInt16 ack_number) {
            this.SequenceNumber = sequence_number;
            this.AckNumber = ack_number;
        }

        /// <summary>
        /// Acknowledges packet with <c>sequence_number</c> in <c>UDPPacket</c>
        /// If the offset is greater than 32 it will remain unacknowledged.
        /// </summary>
        /// <param name="sequence_number">the packet to be acknowledged</param>
        public void AckPacket(UInt16 sequence_number)
        {
            UInt16 offset = (UInt16) (this.AckNumber - sequence_number);

            if(offset >= 32)
            {
                return;
            }

            if(offset < 32 && offset >= 0)
            {
                this.AckArray.Set((int) offset, true);
            }
        }

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

            this.AckArray.CopyTo(payload, cursor);
            cursor += this.AckArray.Length / 8;
            BitConverter.GetBytes(this.AckNumber).CopyTo(payload, cursor);
            cursor += 2;
            BitConverter.GetBytes(this.SequenceNumber).CopyTo(payload, cursor);
            cursor += 2;
            
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
            int length = bytes.Length;
            int cursor = 0;
            if(length < 8)
            {
                return messages;
            }

            this.AckArray = new BitArray(BitConverter.GetBytes(BitConverter.ToUInt32(bytes, cursor)));
            cursor += 4;
            this.AckNumber = BitConverter.ToUInt16(bytes, cursor);
            cursor += 2;
            this.SequenceNumber = BitConverter.ToUInt16(bytes, cursor);
            cursor += 2;

            while (cursor < length)
            {
                Message message = Message.Deserialize(bytes, cursor);
                AddMessage(message);
                cursor += message.Size();
            }

            return messages;
        }

        public void PrintHeader()
        {
            Debug.Log("Packet with sequence id " + this.SequenceNumber
            + "\nAcked packet number " + this.AckNumber
            + "\nBitField " + PrintBitArray(this.AckArray));
        }

        private string PrintBitArray(BitArray ba)
        {
            string s = "";
            foreach(bool b in ba)
            {
                s = s + " " + b;
            }
            return s;
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