﻿using Assets.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Assets.Server
{
    public class Server : MonoBehaviour
    {
        public static Server instance;

        public Queue<Action> TaskQueue { get; private set; }

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("Starting server...");
            this.TaskQueue = new Queue<Action>();
            Server.instance = this;
            UDPServer.getInstance().Init(this);
        }

        // Update is called once per frame
        void Update()
        {

        }

        void FixedUpdate()
        {
            while(this.TaskQueue.Count > 0)
            {
                this.TaskQueue.Dequeue().Invoke();
            }

            UDPServer.getInstance().FixedUpdate();
        }

        public void HandleMessages(List<Message> messages)
        {
            MessageVisitorGameStateUpdater v = new MessageVisitorGameStateUpdater();
            foreach(Message message in messages)
            {
                message.Accept(v);
            }
        }

        public void NewClient(Client c)
        {
            
        }
    }
}