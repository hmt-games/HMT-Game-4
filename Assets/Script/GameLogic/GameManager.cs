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
    [HideInInspector] public Tower parentTower;
    [HideInInspector] public PlayerPuppetBot player;

    [Header("Game Config")]
    public GameConfigSO gameConfig;
    
    [Header("Game Score Text")]
    [SerializeField] private TMP_Text scoreGoalText;
    [SerializeField] private TMP_Text scoreCurrentText;
    private Dictionary<string, int> _currentScore;

    [Header("bots")]
    public BotModeSO DefaultBotMode;
    public GameObject puppetBot;
    
    [Header("Ticks")]
    public ulong currentTick = 0; //At 1 sec/this, this should allow for several millenia of game time
    public float secondPerTick = 1.0f;
    
    private void Awake()
    {
        if (Instance) Destroy(this.gameObject);
        else Instance = this;

        secondPerTick = gameConfig.secondPerTick;
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
        
        foreach (var kvp in gameConfig.GetGameGoalDict())
        {
            if (separator) sb.Append(" | ");
            
            string plantName = kvp.Key;
            sb.Append($"{plantName}: {kvp.Value}");
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
