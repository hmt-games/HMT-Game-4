using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameConstants;
using util.GameRepresentation;

public class LevelInitializer : MonoBehaviour
{
    public void InitPlantsOnGrid()
    {
        // foreach (var gridLayer in GameMap.allGridLayers)
        // {
        //     for (int x = 0; x < GameManager.S.levelConfig.width; x++)
        //     {
        //         for (int y = 0; y < GameManager.S.levelConfig.height; y++)
        //         {
        //             GridNode gridNode = gridLayer.GetGridNodeByCoordinate(new Vector2(x, y));
        //             PlantStateMachine nFSM = gridNode.gameObject.AddComponent<PlantStateMachine>();
        //             nFSM.parentGrid = gridNode;
        //             nFSM.plantType = PlantType.Cactus;
        //             nFSM.dormantTime = 1.0f;
        //             nFSM.StartTick();
        //         }
        //     }
        // }
        
        GameManager.S.SpawnPlant(new Vector2(2, 2), 0, PlantType.Cactus);
    }

    public void InitGridStats()
    {
        // dummy init just for testing plant FSM
        // TODO: Talk to designer abt initial setup
        foreach (var gridLayer in GameMap.allGridLayers)
        {
            for (int x = 0; x < GameManager.S.levelConfig.width; x++)
            {
                for (int y = 0; y < GameManager.S.levelConfig.height; y++)
                {
                    GridNode gridNode = gridLayer.GetGridNodeByCoordinate(new Vector2(x, y));
                    gridNode.waterLevel = 10.0f;
                    gridNode.nutrition = new Dictionary<NutritionType, float>();
                    gridNode.nutrition[NutritionType.Christrogen] = 10.0f;
                    gridNode.nutrition[NutritionType.Eriktonium] = 10.0f;
                    gridNode.nutrition[NutritionType.Farrtrite] = 10.0f;
                }
            }
        }
    }
}
