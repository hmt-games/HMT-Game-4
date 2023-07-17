using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace util.GridRepresentation
{
    public class GridRepresentation
    {
        public static int layerSpacing = 10;

        public static Vector3 PositionFromGridCoord(int row, int col, int layer)
        {
            return new Vector3(row, layer * -layerSpacing, col);
        }
    }
}