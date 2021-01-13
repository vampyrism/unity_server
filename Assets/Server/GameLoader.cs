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
        GameState.instance.CreateEnemy(45, 45);
        GameState.instance.CreateBow(48, 52);
        GameState.instance.CreateBow(40, 72);
        GameState.instance.CreateBow(8, 92);
        GameState.instance.CreateBow(8, 30);
        GameState.instance.CreateBow(88, 95);
        GameState.instance.CreateBow(60, 30);
        GameState.instance.CreateCrossbow(50, 52);
        GameState.instance.CreateCrossbow(91, 95);
        GameState.instance.CreateCrossbow(89, 20);
        GameState.instance.CreateCrossbow(73, 35);
        GameState.instance.CreateCrossbow(43, 92);
        GameState.instance.CreateCrossbow(17, 91);


    }
}

