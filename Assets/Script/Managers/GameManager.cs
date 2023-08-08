using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager S;
    
    public PlantTheme plantTheme;
    public Draw3DGrid draw3DGrid;
    [SerializeField] private LevelInitializer levelInitializer;
    [HideInInspector] public LevelConfig levelConfig;

    private void Awake()
    {
        if (!S) S = this;
        else Destroy(this.gameObject);

        levelConfig = draw3DGrid.levelConfig;
    }

    private void Start()
    {
        draw3DGrid.Create3DGrid();
        levelInitializer.InitPlantsOnGrid();
    }

    public bool CheckCoordValid(Vector2 coord)
    {
        if (coord.x >= draw3DGrid.levelConfig.width) return false;
        if (coord.y >= draw3DGrid.levelConfig.height) return false;
        return true;
    }
}
