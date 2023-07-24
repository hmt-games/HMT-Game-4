using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoombaBotPathFinder
{
    List<GridNode> finalPath;

    public List<GridNode> FindPath(GridNode startNode, GridNode endNode)
    {
        startNode.G = 0;
        startNode.H = GetManhattanDistance(startNode, endNode);

        List<GridNode> nodesToCheckList = new List<GridNode>();
        List<GridNode> checkedNodesList = new List<GridNode>();

        nodesToCheckList.Add(startNode);

        while(nodesToCheckList.Count > 0)
        {
            //Find the tile with lowest F cost
            GridNode currentLowestFCostGridNode = nodesToCheckList.OrderBy(gridNode => gridNode.F).First();

            //Remove from available list
            nodesToCheckList.Remove(currentLowestFCostGridNode);

            //Add to closed list
            checkedNodesList.Add(currentLowestFCostGridNode);

            //If reached end
            if(currentLowestFCostGridNode == endNode)
            {
                finalPath = new List<GridNode>();
                GridNode tempNode = endNode;

                //Trace path from end to start. As the whole path is actually linked by the "previous" nodes
                while (tempNode != startNode)
                {                    
                    finalPath.Add(tempNode);
                    tempNode = tempNode.cameFromNode;
                }

                //As the path was traced from End -> Start. Reverese it to get the right path
                finalPath.Reverse();

                //FINAL PATH
                return finalPath;
            }

            //If not end
            else
            {
                var neighbors = GetNeighbors(currentLowestFCostGridNode);
                foreach (var neighborNode in neighbors)
                {
                    //If node is unavailable
                    if(neighborNode.gridNodeState == util.GridRepresentation.GridState.Planted || checkedNodesList.Contains(neighborNode))
                    {
                        continue;
                    }

                    //Calculate Manhattan Distance
                    neighborNode.G = GetManhattanDistance(startNode, neighborNode);
                    neighborNode.H = GetManhattanDistance(endNode, neighborNode);

                    //Set a reference to the previous node. Used to trace back the final path.
                    neighborNode.cameFromNode = currentLowestFCostGridNode;

                    if(!nodesToCheckList.Contains(neighborNode))
                    {
                        nodesToCheckList.Add(neighborNode);
                    }
                }
                    
            }

        }

        //If no valid path
        return null;
    }



    //Manhattan Distance - https://taketake2.com/ne1615_en.png
    int GetManhattanDistance(GridNode fromNode, GridNode toNode)
    {
        return (Mathf.Abs(fromNode.coordinate.y - toNode.coordinate.y) +
            Mathf.Abs(fromNode.coordinate.x - toNode.coordinate.x));
    }




    util.GridRepresentation.GridLayer currentGridLayer;
    List<GridNode> currentNeighbors;
    //NOTE: Orientation of neighbors are currently defined here based on the grid generation method. If that is changed please make changes here accordingly.
    List<GridNode> GetNeighbors(GridNode gridNode)
    {
        currentGridLayer = GameConstants.GameMap.allGridLayers[gridNode.layer];
        currentNeighbors = new List<GridNode>();

        //Neighbor above
        if(gridNode.coordinate.x != 0)
            currentNeighbors.Add(currentGridLayer.nodesOnThisGridLayer[gridNode.coordinate.x - 1, gridNode.coordinate.y]);

        //Neighbor below
        if (gridNode.coordinate.x != (currentGridLayer.nodesOnThisGridLayer.GetLength(0) - 1))
            currentNeighbors.Add(currentGridLayer.nodesOnThisGridLayer[gridNode.coordinate.x  + 1, gridNode.coordinate.y]);

        //Neighbor left
        if (gridNode.coordinate.y != 0)
            currentNeighbors.Add(currentGridLayer.nodesOnThisGridLayer[gridNode.coordinate.x, gridNode.coordinate.y - 1]);

        //Neighbor right
        if (gridNode.coordinate.y != (currentGridLayer.nodesOnThisGridLayer.GetLength(1) - 1))
            currentNeighbors.Add(currentGridLayer.nodesOnThisGridLayer[gridNode.coordinate.x, gridNode.coordinate.y + 1]);


        return currentNeighbors;
    }
}
