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
            if (!GameManager.S.CheckCoordValid(gridNodeCoordinates)) return null;
            return nodesOnThisGridLayer[(int)gridNodeCoordinates.x, (int)gridNodeCoordinates.y];
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