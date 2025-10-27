using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEngine;
using Cinemachine;
using UnityEngine.Serialization;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance { get; private set; }
    
    [Header("Level Spec")]
    [SerializeField] private TextAsset levelConfig;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject soilPrefab;
    [SerializeField] private GameObject stationPrefab;
    [SerializeField] private GridTheme grid2DTheme;
    [SerializeField] private GameObject plantPrefab;
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject floorCamPrefab;
    [SerializeField] private GameObject botPrefab;
    
    // internal use
    public Dictionary<string, PlantConfigSO> plantConfigs;
    private Dictionary<string, SoilConfigSO> _soilConfigs;
    private Dictionary<string, StationConfigSO> _stationConfigs;

    // public access attributes
    [HideInInspector] public string levelName;
    [HideInInspector] public int width;
    [HideInInspector] public int depth;
    [HideInInspector] public int height;
    
    //TODO: delete these
    public List<SpriteRenderer> quadDisplay;

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        plantConfigs = new Dictionary<string, PlantConfigSO>();
        _soilConfigs = new Dictionary<string, SoilConfigSO>();
        _stationConfigs = new Dictionary<string, StationConfigSO>();
        
        CreateTower();
    }

    private void CreateTower()
    {
        JObject towerJObject = JObject.Parse(levelConfig.text);

        levelName = (string)towerJObject["levelName"];
        width = (int)towerJObject["width"];
        depth = (int)towerJObject["depth"];
        height = (int)towerJObject["height"];
        
        GameObject towerObj = Instantiate(towerPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
        Tower nTower = towerObj.GetComponent<Tower>();
        nTower.floors = new Floor[height];
        nTower.width = width;
        nTower.depth = depth;
        nTower.floors = new Floor[height];

        List<JToken> floorsJToken = towerJObject["floors"].ToList();
        
        for (int level = 0; level < height; level++)
        {
            CreateFloor(floorsJToken[level], level, nTower);
        }

        GameManager.Instance.parentTower = nTower;
    }

    private void CreateFloor(JToken floorJObject, int floorIdx, Tower parentTower)
    {
        GameObject floorObj = Instantiate(floorPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
        floorObj.name = $"Floor{floorIdx}";
        Floor nFloor = floorObj.GetComponent<Floor>();
        floorObj.transform.parent = parentTower.gameObject.transform;
        nFloor.parentTower = parentTower;
        nFloor.floorNumber = floorIdx;
        nFloor.Cells = new GridCellBehavior[width, depth];
        
        List<JToken> floorGrids = floorJObject.ToList();
        for (int gridIdx = 0; gridIdx < floorGrids.Count; gridIdx++)
        {
            CreateGrid(floorGrids[gridIdx], gridIdx, nFloor);
        }
        
        parentTower.floors[floorIdx] = nFloor;
        
        // create virtual camera for this floor
        GameObject nCam = Instantiate(floorCamPrefab,
            new Vector3((width - 1.0f) / 2, (depth + 1) * floorIdx + (depth - 1.0f) / 2, -14.0f),
            quaternion.identity);
        CinemachineVirtualCamera virtualCamera = nCam.GetComponent<CinemachineVirtualCamera>();
        if (floorIdx == 0) virtualCamera.m_Priority = 100;
        else virtualCamera.m_Priority = 0;
        CameraManager.Instance.floorCams.Add(virtualCamera);
    }

    private void CreateGrid(JToken gridJObject, int gridIdx, Floor parentFloor)
    {
        int x = gridIdx % width;
        int z = gridIdx / width;
        
        int floorOffsetY = parentFloor.floorNumber * (depth + 1);
        string gridType = gridJObject["gridType"].ToString();
        string gridConfigString = (string)gridJObject["gridConfig"];
        Vector3 gridWorldPosition = new Vector3(x, z + floorOffsetY * parentFloor.floorNumber, 0.0f);
        
        GameObject gridObj;
        GridCellBehavior gridCellBehavior;

        switch (gridType)
        {
            case "soil":
                gridObj = Instantiate(soilPrefab, gridWorldPosition, Quaternion.identity);
                gridObj.name = $"Grid {x},{z}";
                gridObj.GetComponent<SpriteRenderer>().color = (x + z + parentFloor.floorNumber) % 2 == 0 ? grid2DTheme.lightColor : grid2DTheme.darkColor;
                
                SoilCellBehavior nSoilCell = gridObj.GetComponent<SoilCellBehavior>();
                gridCellBehavior = nSoilCell;

                // add config
                SoilConfigSO soilConfigSO;
                if (_soilConfigs.ContainsKey(gridConfigString)) soilConfigSO = _soilConfigs[gridConfigString];
                else
                {
                    soilConfigSO = Resources.Load<SoilConfigSO>($"Configs/SOs/Soils/{gridConfigString}");
                    _soilConfigs[gridConfigString] = soilConfigSO;
                }
                nSoilCell.soilConfig = soilConfigSO;
                
                // set properties
                List<float> nutrientSolutionList = List5FloatsFromJToken(gridJObject["nutrientLevels"]);
                NutrientSolution nutrientSolution = new NutrientSolution(nutrientSolutionList);
                nSoilCell.NutrientLevels = nutrientSolution;
                
                // create plants
                nSoilCell.plants = new List<PlantBehavior>();
                JToken plants = gridJObject["plants"];
                if (plants is JObject plantsObj) {
                    foreach (var property in plantsObj.Properties()) {
                        JToken plantJToken = property.Value;
                        CreatePlant(plantJToken, nSoilCell);
                    }
                }

                parentFloor.Cells[x, z] = nSoilCell;
                break;
            
            case "station":
                gridObj = Instantiate(stationPrefab, gridWorldPosition, Quaternion.identity);
                gridObj.name = $"Station {x},{z}";

                StationCellBehavior nStationCell = gridObj.GetComponent<StationCellBehavior>();
                gridCellBehavior = nStationCell;

                StationConfigSO stationConfigSO;
                if (_stationConfigs.ContainsKey(gridConfigString)) stationConfigSO = _stationConfigs[gridConfigString];
                else
                {
                    stationConfigSO = Resources.Load<StationConfigSO>($"Configs/SOs/Stations/{gridConfigString}");
                    _stationConfigs[gridConfigString] = stationConfigSO;
                }
                nStationCell.SetStationConfig(stationConfigSO);
                
                parentFloor.Cells[x, z] = nStationCell;
                break;
            
            default:
                Debug.LogError($"Grid config for ({x}, {z}, {parentFloor.floorNumber}) has invalid gridType {gridType}");
                return;
        }
        
        gridObj.transform.parent = parentFloor.gameObject.transform;
        gridCellBehavior.parentFloor = parentFloor;
        gridCellBehavior.gridX = x;
        gridCellBehavior.gridZ = z;
        
        // create bots
        string botConfigString = (string)gridJObject["botOnGrid"];
        if (botConfigString != "none") CreateBot(botConfigString, gridCellBehavior);
    }

    private void CreatePlant(JToken plantJToken, SoilCellBehavior parentGrid)
    {
        string plantConfigName = (string)plantJToken["plantConfig"];
        PlantConfigSO plantConfigSo;
        if (plantConfigs.ContainsKey(plantConfigName)) plantConfigSo = plantConfigs[plantConfigName];
        {
            plantConfigSo = Resources.Load<PlantConfigSO>($"Configs/SOs/Plants/{plantConfigName}");
            plantConfigSo.plantSprites = PlantToSpriteCapturer.Instance.CaptureAllStagesAtOnce();
            plantConfigs[plantConfigName] = plantConfigSo;

            if (plantConfigSo.plantSprites.Count == 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    quadDisplay[i].sprite = plantConfigSo.plantSprites[i];
                }
            }
        }

        //TODO: use object pooler
        GameObject plantObj = Instantiate(plantPrefab, Vector3.zero, quaternion.identity);
        PlantBehavior nPlant = plantObj.GetComponent<PlantBehavior>();
        
        List<float> nutrientSolutionList = List5FloatsFromJToken(plantJToken["nutrientSolution"]);
        NutrientSolution nutrientSolution = new NutrientSolution(nutrientSolutionList);

        nPlant.SetPlantState(new PlantStateData(
            plantConfigSo,
            nutrientSolution,
            (float)plantJToken["rootMass"],
            (float)plantJToken["surfaceMass"],
            (float)plantJToken["energyLevel"],
            (int)plantJToken["age"], (int)plantJToken["currentStage"]));

        parentGrid.AddPlant(nPlant);
    }

    private void CreateBot(string botConfigString, GridCellBehavior spawnGrid)
    {
        GameObject nBotObj = Instantiate(botPrefab, spawnGrid.transform.position, quaternion.identity);
        FarmBot nBot = nBotObj.GetComponent<FarmBot>();
        
        BotModeSO botModeSo = Resources.Load<BotModeSO>($"Configs/SOs/Bots/{botConfigString}");
        
        nBot.InitBot(spawnGrid.parentFloor.floorNumber, spawnGrid.gridX, spawnGrid.gridZ, botModeSo);
        spawnGrid.botOccupant = nBot;
    }

    private List<float> List5FloatsFromJToken(JToken JList)
    {
        List <JToken> jList = JList.ToList();
        List<float> ret = new List<float>(5);
        foreach (JToken e in jList)
        {
            ret.Add((float)e);
        }

        return ret;
    }
}