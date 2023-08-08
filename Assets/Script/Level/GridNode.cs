using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using util.GameRepresentation;

public class GridNode : MonoBehaviour
{
    [Header("NODE INFO")]
    public Vector2Int coordinate;
    public int layer;
    public util.GridRepresentation.GridState gridNodeState = util.GridRepresentation.GridState.Empty;
    public Dictionary<NutritionType, int> nutrition;

    // [Space(15)] 
    // [Header("NODE PLANT INFO")] 
    // public float dormantTime;
    // public PlantType initPlantType;
    
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
