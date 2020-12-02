using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private static Pathfinding _instance;
    public static Pathfinding Instance { get; private set; }

    private PathfindingGrid<PathNode> grid;
    private List<PathNode> openList;
    private List<PathNode> closedList;
    private bool changedEndNodeIsWalkable = false;

    public Pathfinding (int width, int height, int cellSize) {
        Instance = this;
        grid = new PathfindingGrid<PathNode>(width, height, cellSize, Vector3.zero, (PathfindingGrid<PathNode> g, int x, int y) => new PathNode(g, x, y));
        SetNodesIsWalkable();
        SetNeighbourLists();
    }

    public PathfindingGrid<PathNode> GetGrid() {
        return grid;
    }
    
    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition) {
        grid.GetXY(startWorldPosition, out int startX, out int startY);
        grid.GetXY(endWorldPosition, out int endX, out int endY);

        List<PathNode> path = FindPath(startX, startY, endX, endY);
        if (path == null) {
            return null;
        } else {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (PathNode pathNode  in path) {
                vectorPath.Add(new Vector3(pathNode.x, pathNode.y) * grid.GetCellsize() + Vector3.one * grid.GetCellsize() * .5f);
            }
            return vectorPath;
        }
    }

    public Vector3 FixCornerCollision(Vector3 currentPosition, Vector3 targetPosition, Vector2 contactPosition) {
        PathNode newTargetNode = new PathNode(grid, 0,0);

        grid.GetXY(currentPosition, out int unitX, out int unitY);
        grid.GetXY(targetPosition, out int targetX, out int targetY);
        grid.GetXY(contactPosition, out int contactX, out int contactY);

        Debug.Log("currentPosition: " + unitX + "," + unitY);
        Debug.Log("targetPosition: " + targetX + "," + targetY);
        Debug.Log("contactPosition: " + contactX + "," + contactY);

        if (contactY < targetY) {
            Debug.Log("Uppåt");
            newTargetNode.x = unitX;
            newTargetNode.y = contactY + 3;
        } else if (contactX < targetX) {

            Debug.Log("Höger");
            newTargetNode.x = contactX + 3;
            newTargetNode.y = unitY;
        } else if (contactY > targetY) {

            Debug.Log("Neråt");
            newTargetNode.x = unitX;
            newTargetNode.y = contactY - 3;
        } else if (contactX > targetX) {
            Debug.Log("Vänster");
            newTargetNode.x = contactX - 3;
            newTargetNode.y = unitY;
        } else {
            Debug.Log("Not handled case");
            newTargetNode.x = targetX;
            newTargetNode.y = targetY;
        }

        if (unitX == targetX && unitX == contactX) {
            if (currentPosition.x > contactPosition.x) {
                Debug.Log("Höger Bounce");
                newTargetNode.x = targetX + 1;
                newTargetNode.y = targetY;
            } else {
                Debug.Log("Vänster Bounce");
                newTargetNode.x = targetX - 1;
                newTargetNode.y = targetY;
            }
        } else if (unitY == targetY && unitY == contactY) {
            if (currentPosition.y > contactPosition.y) {
                Debug.Log("Uppåt Bounce ");
                newTargetNode.x = targetX;
                newTargetNode.y = targetY + 1;
            } else {
                Debug.Log("Nedåt Bounce ");
                newTargetNode.x = targetX;
                newTargetNode.y = targetY - 1;

            }

        }
        Vector3 newTargetVector = new Vector3(newTargetNode.x, newTargetNode.y) * grid.GetCellsize() + Vector3.one * grid.GetCellsize() * .5f;
        return newTargetVector;
    }

    private List<PathNode> FindPath(int startX, int startY, int endX, int endY) {
        PathNode startNode = grid.GetGridObject(startX, startY);
        PathNode endNode = grid.GetGridObject(endX, endY);

        if (endNode.isWalkable == false) {
            endNode.isWalkable = true;
            changedEndNodeIsWalkable = true;
        }

        openList = new List<PathNode>{ startNode };
        closedList = new List<PathNode>();

        for (int x = 0; x < grid.GetWidth(); x++) {
            for (int y = 0; y < grid.GetHeight(); y++) {
                PathNode pathNode = grid.GetGridObject(x,y);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while(openList.Count > 0) {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode) {
                // Reached final node
                if (changedEndNodeIsWalkable == true) {
                    endNode.isWalkable = false;
                    changedEndNodeIsWalkable = false;
                }
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighbourNode in currentNode.neighbourList) {
                if (closedList.Contains(neighbourNode)) continue;
                if(!neighbourNode.isWalkable) {
                    closedList.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost) {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if(!openList.Contains(neighbourNode)) {
                        openList.Add(neighbourNode);
                    }
                }
            }

        }

        // Out of nodes on the openList (Could not find a path)
        Debug.Log("Failed to find path");
        if (changedEndNodeIsWalkable == true) {
            endNode.isWalkable = false;
            changedEndNodeIsWalkable = false;
        }
        return null;
    }

    private PathNode GetNode(int x, int y) {
        return grid.GetGridObject(x, y);
    }

    private List<PathNode> CalculatePath(PathNode endNode) {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.cameFromNode != null) {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b) {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList) {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++) {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost) {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }

    private void SetNodesIsWalkable() {
        float cellSize = grid.GetCellsize();
        cellSize = 1f;
        for (int x = 0; x < grid.GetWidth(); x++) {
            for (int y = 0; y < grid.GetHeight(); y++) {

                //Check collision in the middle of the square
                Collider2D hit = Physics2D.OverlapPoint(new Vector2(x + (0.5f * cellSize), y + (0.5f * cellSize)) * grid.GetCellsize());

                //Check collision in the bottom left part of the square
                if (hit == null) {
                    hit = Physics2D.OverlapPoint(new Vector2(x + (0.2f * cellSize), y + (0.2f * cellSize)) * grid.GetCellsize());
                }

                //Check collision in the top left part of the square
                if (hit == null) {
                    hit = Physics2D.OverlapPoint(new Vector2(x + (0.2f * cellSize), y + (0.8f * cellSize)) * grid.GetCellsize());
                }

                //Check collision in the top right part of the square
                if (hit == null) {
                    hit = Physics2D.OverlapPoint(new Vector2(x + (0.8f * cellSize), y + (0.8f * cellSize)) * grid.GetCellsize());
                }

                //Check collision in the bottom right of the square
                if (hit == null) {
                    hit = Physics2D.OverlapPoint(new Vector2(x + (0.8f * cellSize), y + (0.2f * cellSize)) * grid.GetCellsize());
                }

                if (hit != null) {
                    grid.GetGridObject(x, y).isWalkable = false;

                    //Debugging the collision detection on the grid.
                    //Debug.Log("Found Collision here: " + x + "," + y);
                    Debug.DrawLine(new Vector3(x, y + (0.5f * cellSize)) * grid.GetCellsize(), new Vector3(x + (1f * cellSize), y + (0.5f * cellSize)) * grid.GetCellsize(), Color.red, 100f);
                    Debug.DrawLine(new Vector3(x + (0.5f * cellSize), y) * grid.GetCellsize(), new Vector3(x + (0.5f * cellSize), y + (1f * cellSize)) * grid.GetCellsize(), Color.red, 100f);
                }

            }
        }
    }

    private void SetNeighbourLists() {
        //Setting the neighbour lists for each PathNode
        for (int x = 0; x < grid.GetWidth(); x++) {
            for (int y = 0; y < grid.GetHeight(); y++) {
                PathNode pathNode = grid.GetGridObject(x, y);
                pathNode.neighbourList = MakeNeighbourList(pathNode);
            }
        }
    }

    public List<PathNode> MakeNeighbourList(PathNode currentNode) {
        List<PathNode> neighbourList = new List<PathNode>();
        //if (currentNode.isWalkable == false) {
        //    return neighbourList;
        //}
        PathNode leftNode = GetNode(currentNode.x - 1, currentNode.y);
        PathNode rightNode = GetNode(currentNode.x + 1, currentNode.y);
        PathNode downNode = GetNode(currentNode.x, currentNode.y - 1);
        PathNode upNode = GetNode(currentNode.x, currentNode.y + 1);

        if (currentNode.x - 1 >= 0) {
            // Left
            neighbourList.Add(leftNode);
            if (leftNode.isWalkable == true) {
                // Left Down
                if (currentNode.y - 1 >= 0 && downNode.isWalkable == true) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
                // Left Up
                if (currentNode.y + 1 < grid.GetHeight() && upNode.isWalkable == true) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
            }
        }
        if (currentNode.x + 1 < grid.GetWidth()) {
            // Right
            neighbourList.Add(rightNode);
            if (rightNode.isWalkable == true) {
                // Right Down
                if (currentNode.y - 1 >= 0 && downNode.isWalkable == true) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
                // Right Up
                if (currentNode.y + 1 < grid.GetHeight() && upNode.isWalkable == true) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
            }
        }
        // Down
        if (currentNode.y - 1 >= 0) neighbourList.Add(downNode);
        // Up
        if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(upNode);

        return neighbourList;
    }
}
