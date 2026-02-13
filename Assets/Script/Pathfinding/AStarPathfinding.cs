using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStarPathfinding : MonoBehaviour
{
    private AStarGrid _grid;
    private PathRequestManager _pathRequestManager;

    private void Awake()
    {
        _grid = GetComponent<AStarGrid>();
        _pathRequestManager = GetComponent<PathRequestManager>();
    }

    public void TryFindPath(Vector3 startPosition, Vector3 targetPosition)
    {
        StartCoroutine(FindPath(startPosition, targetPosition));
    }

    /// <summary>
    /// A* pathfinding algorithm based on
    /// https://www.youtube.com/watch?v=-L-WgKMFuhE&list=PLFt_AvWsXl0cq5Umv3pMC9SPnKjfp9eGW&index=1
    /// </summary>
    private IEnumerator FindPath(Vector3 startPosition, Vector3 targetPosition)
    {
        bool pathFindSuccess = false;
        Vector3[] waypoints = new Vector3[0];
        
        AStarNode startNode = _grid.WorldPointToNode(startPosition);
        AStarNode targetNode = _grid.WorldPointToNode(targetPosition);

        Heap<AStarNode> openSet = new Heap<AStarNode>(_grid.GridMaxSize);
        HashSet<AStarNode> closeSet = new HashSet<AStarNode>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            AStarNode currentNode = openSet.RemoveFirst();
            closeSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                pathFindSuccess = true;
                break;
            }

            foreach (AStarNode neighbour in _grid.GetNeighbours(currentNode))
            {
                if (!neighbour.Walkable || closeSet.Contains(neighbour)) continue;
                
                int movementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (movementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = movementCostToNeighbour;
                    neighbour.Parent = currentNode;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    
                    if (!openSet.Contains(neighbour)) openSet.Add(neighbour);
                    else openSet.UpdateItem(neighbour);
                }
            }
        }

        yield return null;
        if (pathFindSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
        }
        _pathRequestManager.FinishProcessingPath(waypoints, pathFindSuccess);
    }

    private Vector3[] RetracePath(AStarNode starNode, AStarNode targetNode)
    {
        List<AStarNode> path = new List<AStarNode>();
        AStarNode currentNode = targetNode;

        while (currentNode != starNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }
    
    Vector3[] SimplifyPath(List<AStarNode> path) {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;
		
        for (int i = 1; i < path.Count; i ++) {
            Vector2 directionNew = new Vector2(path[i-1].GridX - path[i].GridX,path[i-1].GridY - path[i].GridY);
            if (directionNew != directionOld) {
                waypoints.Add(path[i].NodePosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }
    
    int GetDistance(AStarNode nodeA, AStarNode nodeB) {
        int dstX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
        int dstY = Mathf.Abs(nodeA.GridY - nodeB.GridY);

        if (dstX > dstY)
            return 14*dstY + 10* (dstX-dstY);
        return 14*dstX + 10 * (dstY-dstX);
    }
}
