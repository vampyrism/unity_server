using Assets.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    [SerializeField] private Transform tileMap;
    private Pathfinding pathfinding;
    // Start is called before the first frame update
    public void Init()
    {
        Instantiate(tileMap, new Vector3(0f, 100f), Quaternion.identity);
        this.pathfinding = new Pathfinding(100, 100, 1);
        GameState.instance.CreateEnemy(50, 50);
    }
}
