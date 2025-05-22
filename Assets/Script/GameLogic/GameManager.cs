using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using Fusion;
using TMPro;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Tower parentTower;
    public PlayerPuppetBot player;
    public long currentTick = 0;
    public float secondPerTick = 1.0f;

    public BotModeSO DefaultBotMode;

    // Game Score
    [SerializeField] private TMP_Text scoreGoalText;
    [SerializeField] private TMP_Text scoreCurrentText;
    private Dictionary<string, int> _currentScore;

    [Header("bots")]
    public GameObject puppetBot;
    
    private void Awake()
    {
        if (Instance) Destroy(this.gameObject);
        else Instance = this;
    }

    public void Tick()
    {
        parentTower.OnTick();
        
        //HeatMapSwicher.S.SwitchOnHeatMap();
        currentTick++;
    }

    //TODO: score goal should be configable in a separate config file, supplied to map generator
    public void InitScoring(Dictionary<string, PlantConfigSO> plantConfigs)
    {
        _currentScore = new Dictionary<string, int>();
        StringBuilder sb = new StringBuilder();
        sb.Append("Goal: ");
        bool separator = false;
        
        foreach (var kvp in plantConfigs)
        {
            if (separator) sb.Append(" | ");
            
            string plantName = kvp.Key;
            sb.Append($"{plantName}: {10}");
            _currentScore[plantName] = 0;
            
            if (!separator) separator = true;
        }

        scoreGoalText.text = sb.ToString();
        UpdateCurrentScore();
    }

    public void SubmitPlant(Dictionary<string, int> submittedPlant)
    {
        foreach (var kvp in submittedPlant)
        {
            _currentScore[kvp.Key] += kvp.Value;
        }
        
        UpdateCurrentScore();
    }

    private void UpdateCurrentScore()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Current: ");
        bool separator = false;
        
        foreach (var kvp in _currentScore)
        {
            if (separator) sb.Append(" | ");
            
            sb.Append($"{kvp.Key}: {kvp.Value}");
            
            if (!separator) separator = true;
        }

        scoreCurrentText.text = sb.ToString();
    }

    #region temperary testing scripts

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
                    SoilCellBehavior cell = Cells[i, j] as SoilCellBehavior;
                    if (cell != null)
                    {
                        foreach (PlantBehavior plant in cell.plants)
                        {
                            plant.GetComponent<SpriteRenderer>().sprite = plant.config.plantSprites[plantStages];
                        }
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

    public void SpawnPlayerPuppet()
    {
        int x = Random.Range(0, parentTower.floors[0].Cells.GetLength(0));
        int y = Random.Range(0, parentTower.floors[0].Cells.GetLength(1));
        Debug.Log($"{parentTower == null}");
        Debug.Log($"{parentTower.floors == null}");
        GameObject nBotObj = Instantiate(puppetBot, parentTower.floors[0].Cells[x, y].transform.position, Quaternion.identity);
        PlayerPuppetBot nBot = nBotObj.GetComponent<PlayerPuppetBot>();
        nBot.InitBot(0, x, y);
        nBot.PlayerEmbody();
    }

    #endregion
}
