using Assets.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Server : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Starting server...");
        UDPServer.getInstance().Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
