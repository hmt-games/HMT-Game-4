using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameConstants;
using util.GameRepresentation;

public class LevelInitializer : MonoBehaviour
{
    public void InitPlantsOnGrid()
    {
        foreach (var gridLayer in GameMap.allGridLayers)
        {
            for (int x = 0; x < GameManager.S.levelConfig.width; x++)
            {
                for (int y = 0; y < GameManager.S.levelConfig.height; y++)
                {
                    GridNode gridNode = gridLayer.GetGridNodeByCoordinate(new Vector2(x, y));
                    PlantStateMachine nFSM = gridNode.gameObject.AddComponent<PlantStateMachine>();
                    nFSM.parentGrid = gridNode;
                    nFSM.plantType = PlantType.Cactus;
                    nFSM.dormantTime = 1.0f;
                    nFSM.StartTick();
                }
            }
        }
    }
}
