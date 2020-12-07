using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    private PathfindingGrid<PathNode> grid;
    public int x;
    public int y; 

    public int gCost;
    public int hCost;
    public int fCost;

    public bool isWalkable;
    public PathNode cameFromNode;
    public List<PathNode> neighbourList;

    public PathNode(PathfindingGrid<PathNode> grid, int x, int y) {
        this.grid = grid;
        this.x = x;
        this.y = y;
        isWalkable = true;
        neighbourList = new List<PathNode>();
    }

    public void CalculateFCost() {
        fCost = gCost + hCost;
    }

    public override string ToString() {
        return x + "," + y;
    }

}
