using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Fusion;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Tower parentTower;
    public long currentTick = 0;
    public float secondPerTick = 1.0f;

    private void Awake()
    {
        if (Instance) Destroy(this.gameObject);
        else Instance = this;
    }

    public void Tick()
    {
        if (BasicSpawner._runner.IsServer)
        {
            parentTower.OnTick();
        }

        //HeatMapSwicher.S.SwitchOnHeatMap();
        currentTick++;
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
                    foreach (PlantBehavior plant in cell.plants)
                    {
                        plant.GetComponent<SpriteRenderer>().sprite = plant.config.plantSprites[plantStages];
                    }
                }
            }
        }
    }

    // Test script for bot Spawn
    // TODO: Delete this later
    [SerializeField] private GameObject bot;
    public void SpawnBot()
    {
        Instantiate(bot, parentTower.floors[0].Cells[0, 0].transform.position, Quaternion.identity);
    }

    [SerializeField] private GameObject puppetBot;
    public void SpawnPuppetBot()
    {
        int x = Random.Range(0, parentTower.floors[0].Cells.GetLength(0));
        int y = Random.Range(0, parentTower.floors[0].Cells.GetLength(1));
        GameObject nBotObj = Instantiate(puppetBot, parentTower.floors[0].Cells[x, y].transform.position, Quaternion.identity);
        DefaultPuppetBot nBot = nBotObj.GetComponent<DefaultPuppetBot>();
        nBot.InitBot(0, x, y);
    }
}
