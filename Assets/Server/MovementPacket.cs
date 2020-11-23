
@@ -0,0 +1,341 @@
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime;
using System.Security.Cryptography;

//
// This module handles serialization of the information sent over UDP between the game client and server.
// It implements the IDisposable interface in order to give greater control of when the object
// is released.
// 
// The UDPPacket design follows the following schema:
// 
// Bytes   | Description
// --------------------------
// 0-1     | Sequence number
// 2-3     | Entity ID
// 4       | Entity action type
// 5       | Entity action descriptor
// 6-9     | Entity X coordinate
// 10-13   | Entity Y coordinate
// 14-17   | Entity rotation
// 18-21   | Entity X velocity
// 22-25   | Entity Y velocity
//
// Multiple messages can be packed into the same packet, since the message size is far below the safe threshold
// of 508 bytes. If this threshold is surpassed the packet may be split up into several IP packages which is not ideal.
// 

namespace Assets.Server
{
    public class MovementPacket : UDPPacket 
    {
        public static readonly int SCHEMA_SIZE = 27;
        // 8-11    | Entity X coordinate
        private static readonly int X_COORDINATE = 8;
        // 12-13   | Entity Y coordinate
        private static readonly int Y_COORDINATE = 12;
        // 16-19   | Entity rotation
        private static readonly int ROTATION = 16;
        // 20-23   | Entity X velocity
        private static readonly int X_VELOCITY = 20;
        // 24-27   | Entity Y velocity
        private static readonly int Y_VELOCITY = 24;


        public MovementMessage(short seqNum, short entityId, short packetType, byte actionType, byte actionDescriptor, float x, float y, float r, float xd, float yd)
        {
            base.AddMessageHeader(seqNum, entityId, packetType, actionType, actionDescriptor);
            base.SetFloat(x, X_COORDINATE);
            base.SetFloat(y, Y_COORDINATE);
            base.SetFloat(r, ROTATION);
            base.SetFloat(xd, X_VELOCITY);
            base.SetFloat(yd, Y_VELOCITY);
        }





    }
}