﻿using Assets.Server;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Assets.Server
{
    public class Server : MonoBehaviour
    {
        public static Server instance;
        // TODO: Should be a ConcurrentQueue
        public ConcurrentQueue<Action> TaskQueue { get; private set; }
        public ConcurrentDictionary<UInt32, Entity> Entities { get; private set; } = new ConcurrentDictionary<uint, Entity>();
        

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("Starting server...");
            //Runs the GameLoader script
            (Resources.Load("GameLoader") as GameObject).GetComponent<GameLoader>().Init();
            this.TaskQueue = new ConcurrentQueue<Action>();
            Server.instance = this;
            UDPServer.getInstance().Init(this);
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDestroy() {
            UDPServer.getInstance().Stop();
        }

        void FixedUpdate()
        {
            while(this.TaskQueue.Count > 0)
            {
                bool s = this.TaskQueue.TryDequeue(out Action a);

                if (s)
                {
                    a.Invoke();
                }
            }

            GameState.instance.FixedUpdate();
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

        public void ServerDestroyEntity(Entity e) {
            Debug.Log("Inside serverdestroy");
            Destroy(e.gameObject);
        }

    }
}