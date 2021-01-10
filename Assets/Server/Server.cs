using Assets.Server;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Server
{
    public class Server : MonoBehaviour
    {
        public static string lobbyManager = "http://localhost:4000";
        public static int port = 9001;

        public static Server instance;
        // TODO: Should be a ConcurrentQueue
        public ConcurrentQueue<Action> TaskQueue { get; private set; }
        public ConcurrentDictionary<UInt32, Entity> Entities { get; private set; } = new ConcurrentDictionary<uint, Entity>();

        public LobbyResponse lobby;

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("Starting server on port " + port + "...");
            
            Debug.Log("Registering with lobbymanager... (" + lobbyManager + ")");
            UnityWebRequest wr = UnityWebRequest.Post(lobbyManager + "/api/lobby/new/" + port, "");
            wr.SendWebRequest();
            while (!wr.isDone) ;
            try
            {
                this.lobby = JsonUtility.FromJson<LobbyResponse>(wr.downloadHandler.text);
            } 
            catch(ArgumentException e)
            {
                Debug.LogError(wr.downloadHandler.text);
                Debug.LogError(e);
            }
            Debug.Log("Sever has ID " + this.lobby.id);
            // Start lobbymanager heartbeat
            InvokeRepeating("Heartbeat", 1f, 10f);


            Debug.Log("Loading game...");
            //Runs the GameLoader script
            (Resources.Load("GameLoader") as GameObject).GetComponent<GameLoader>().Init();
            this.TaskQueue = new ConcurrentQueue<Action>();
            Server.instance = this;


            Debug.Log("Listening for new connections...");
            UDPServer.getInstance().Init(this, port);
            Debug.Log("Loading complete");
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

        public void Heartbeat()
        {
            UnityWebRequest wr = UnityWebRequest.Post(lobbyManager + "/api/lobby/heartbeat/" + this.lobby.id, "");
            wr.SendWebRequest();
            while (!wr.isDone);

            if(wr.downloadHandler.text != "ok")
            {
                Debug.LogError("Lobbymanager heartbeat failed");
                Application.Quit();
            }
        }

    }
}