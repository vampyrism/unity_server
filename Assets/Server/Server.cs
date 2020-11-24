using Assets.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Server
{
    public class Server : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("Starting server...");
            UDPServer.getInstance().Init(this);
        }

        // Update is called once per frame
        void Update()
        {

        }

        void FixedUpdate()
        {
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
    }
}