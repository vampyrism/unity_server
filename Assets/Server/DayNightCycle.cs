using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Keeps track of current time

namespace Assets.Server
{
    public class DayNightCycle
    {
        // How much time is left of current day or night
        public float TimeLeft { get; private set;}

        // How  long the day/night lasts
        public float DayLength { get; private set;}
        public float NightLength { get; private set;}

        // To keep track of current state
        public bool IsDay { get; private set; }

        /// <summary>
        /// Keeps track of day/night cycle and will broadcast updates to clients.
        /// </summary>
        /// <param name="DayLength">Length of day in seconds.</param>
        /// <param name="NightLength">Length of night in seconds.</param>
        public DayNightCycle(float dayLength, float nightLength)
        {
            this.DayLength = dayLength;
            this.NightLength = nightLength;
            // Start cycle during day
            SetToDay();
        }
        
        /// <summary>
        /// Reset time to day.
        /// </summary>
        public void SetToDay()
        {
            IsDay = true;
            TimeLeft = DayLength;
        }

        /// <summary>
        /// Reset time to night.
        /// </summary>
        public void SetToNight()
        {
            IsDay = false;
            TimeLeft = NightLength;
        }

        /// <summary>
        /// Reset to night if day and vice versa.
        /// </summary>
        public void Switch()
        {
            if (IsDay)  
            {
                SetToNight();
            }
            else
            {
                SetToDay();
            }
        }

        /// <summary>
        /// Add message about the current mode to broadcast queue.
        /// </summary>
        public void BroadcastState()
        {
            StateUpdateMessage.Descriptor descriptor = IsDay ? StateUpdateMessage.Descriptor.DAY : StateUpdateMessage.Descriptor.NIGHT;
            StateUpdateMessage message = new StateUpdateMessage(
                StateUpdateMessage.Type.DAY_NIGHT,
                descriptor
            );
            Debug.Log(message);
            UDPServer.getInstance().BroadcastMessage(message);
        }

        /// <summary>
        /// Update current time and state based on tick rate.
        /// </summary>
        public void FixedUpdate() 
        {
            TimeLeft -= Time.deltaTime;
            //Debug.Log(TimeLeft);
            if (TimeLeft <= 0)
            {
                Switch();
                BroadcastState();
            }
        }
    }
}
