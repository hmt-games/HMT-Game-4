using System;
using System.Collections;
using System.Collections.Generic;
using GameConstants;
using UnityEngine;
using util.GameRepresentation;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager S;
    
    public PlantTheme plantTheme;
    public Draw3DGrid draw3DGrid;
    public PlantConfigs plantConfigs;
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
        levelInitializer.InitGridStats();
    }

    public bool CheckCoordValid(Vector2 coord, int layer = 0)
    {
        if (layer < 0) return false;
        if (layer >= levelConfig.layerCount) return false;
        if (coord.x >= draw3DGrid.levelConfig.width || coord.x < 0) return false;
        if (coord.y >= draw3DGrid.levelConfig.height || coord.y < 0) return false;
        return true;
    }

    public void SpawnPlant(Vector2 coord, int layer, PlantType plantType)
    {
        if (!CheckCoordValid(coord, layer)) return;

        GridNode gridNode = GameMap.allGridLayers[layer].GetGridNodeByCoordinate(coord);
        if (gridNode.gameObject.GetComponent<PlantStateMachine>() != null) Destroy(gridNode.gameObject.GetComponent<PlantStateMachine>());
        PlantStateMachine nFSM = gridNode.gameObject.AddComponent<PlantStateMachine>();
        nFSM.parentGrid = gridNode;
        nFSM.plantType = plantType;
        plantConfigs.GetPlantInfo(plantType, out PlantConfigs.Plant plant);
        nFSM.dormantTime = Random.Range(plant.dormantMinTime, plant.dormantMaxTime);
        nFSM.StartTick();
    }

    public void RainyEffect()
    {
        var TopLayer = GameMap.allGridLayers[0];

        for (int x = 0; x < levelConfig.width; x++)
        {
            for (int y = 0; y < levelConfig.height; y++)
            {
                GridNode gridNode = TopLayer.GetGridNodeByCoordinate(new Vector2(x, y));

                if(gridNode.waterLevel < GameConstants.Grid.MaxWaterLevel - 1.0f)
                {
                    gridNode.waterLevel += 1.0f;
                }
                else
                {
                    gridNode.waterLevel = GameConstants.Grid.MaxWaterLevel;
                }
            }
        }
    }
}
