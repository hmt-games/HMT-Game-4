using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNode : MonoBehaviour
{
    [Header("NODE INFO")]
    public Vector2Int coordinate;
    public int layer;


    public util.GridRepresentation.GridState gridNodeState = util.GridRepresentation.GridState.Empty;


    [Space(15)]

    [Header("FOR PATH FINDER")]
    public int G;
    public int H;
    public int F
    {
        get
        {
            return (G + H);
        }
    }
    [Space(5)]
    public GridNode cameFromNode;
}
