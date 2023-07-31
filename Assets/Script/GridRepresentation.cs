using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace util.GridRepresentation
{
    public enum GridState
    {
        Empty,
        Planted
    }


    [System.Serializable]
    public class GridLayer
    {
        public GridNode[,] nodesOnThisGridLayer;
        public GridLayer(int rowSize, int columnSize)
        {
            this.nodesOnThisGridLayer = new GridNode[rowSize, columnSize];
        }    
        

        
        public GridNode GetGridNodeByCoordinate(Vector2 gridNodeCoordinates)
        {            
            foreach (var node in nodesOnThisGridLayer)
            {
                if (node.coordinate == gridNodeCoordinates)
                    return node;
            }

            return null;
        }
    }


    public class GridRepresentation
    {
        public static int layerSpacing = 10;
        public static int gridLayer = 3;



        public static Vector3 PositionFromGridCoord(int row, int col, int layer)
        {
            return new Vector3(row, layer * -layerSpacing, col);
        }
    }
}