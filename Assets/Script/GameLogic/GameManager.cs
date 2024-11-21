using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Tower parentTower;

    private void Awake()
    {
        if (Instance) Destroy(this.gameObject);
        else Instance = this;
    }

    public void Tick()
    {
        parentTower.OnTick();
        //HeatMapSwicher.S.SwitchOnHeatMap();
    }
    
    // golden finger for testing
    private int plantStages = 0;
    public void PlantNextStage()
    {
        plantStages = (plantStages + 1) % 4;
        foreach (Floor floor in parentTower.floors)
        {
            GridCellBehavior[,] Cells = floor.Cells;
            for (int i = 0; i < Cells.GetLength(0); i++)
            {
                for (int j = 0; j < Cells.GetLength(1); j++)
                {
                    GridCellBehavior cell = Cells[i,j];
                    foreach (PlantBehavior plant in cell.surfacePlants)
                    {
                        plant.GetComponent<SpriteRenderer>().sprite = plant.config.plantSprites[plantStages];
                    }
                    foreach (PlantBehavior plant in cell.rootedPlants)
                    {
                        plant.GetComponent<SpriteRenderer>().sprite = plant.config.plantSprites[plantStages];
                    }
                }
            }
        }
    }
}
