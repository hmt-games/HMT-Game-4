using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using util.GameRepresentation;

public class GridNode : MonoBehaviour
{
    [Header("NODE META")]
    public Vector2Int coordinate;
    public int layer;
    public util.GridRepresentation.GridState gridNodeState = util.GridRepresentation.GridState.Empty;

    [Space(15)] 
    [Header("NODE INFO")] 
    public float waterLevel;
    public Dictionary<NutritionType, float> nutrition;

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
