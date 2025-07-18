using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Text;
using Cinemachine;
using GameConstant;
using UnityEngine.Serialization;

public class MapGeneratorJSON : NetworkBehaviour
{
    public static MapGeneratorJSON Instance { get; private set; }

    private TextAsset configJSON;
    private TextAsset towerJSON;
    [SerializeField] private GameObject soilPrefab;
    [SerializeField] private GameObject stationPrefab;
    [SerializeField] private GridTheme grid2DTheme;
    [SerializeField] private GameObject plantPrefab;
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject floorCamPrefab;


    [FormerlySerializedAs("_plantConfigs")] 
    public Dictionary<string, PlantConfigSO> plantConfigs;
    private Dictionary<string, SoilConfigSO> _soilConfigs;
    // private Dictionary<string, GameObject> _name2Plant;

    private JObject _configJObject;
    
    public int width;
    public int depth;
    public int height;



    public bool mapUpdated;
    
    private void Awake()
    {
        if (Instance) Destroy(this.gameObject);
        else Instance = this;
    }

    private void Start()
    {
        configJSON = GameManager.Instance.gameConfig.configJSON;
        towerJSON = GameManager.Instance.gameConfig.towerJSON;
        
        
        plantConfigs = new Dictionary<string, PlantConfigSO>();
        _soilConfigs = new Dictionary<string, SoilConfigSO>();
        // _name2Plant = new Dictionary<string, GameObject>();
        _configJObject = JObject.Parse(configJSON.text);
        
        CreateTower();
    }

    private IEnumerator SpawnBot()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        //GameManager.Instance.SpawnBot();
        for (int i = 0; i < 1; i++)
            GameManager.Instance.SpawnPlayerPuppet();
    }

    //TODO: probly dont need to do this without networking
    private void CreateConfigs()
    {
        foreach (var config in _configJObject.Properties())
        {
            if (config.Name.ToLower().Contains("plant")) CreatePlantConfig(config.Name);
            else CreateSoilConfig(config.Name);
        }
    } 

    public void CreateTower()
    {
        JObject towerJObject = JObject.Parse(towerJSON.text);
        width = (int)towerJObject["Tower"]["width"];
        depth = (int)towerJObject["Tower"]["depth"];
        height = (int)towerJObject["Tower"]["height"];

        GameObject towerObj = Instantiate(towerPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
        CreateConfigs();

        Tower nTower = towerObj.GetComponent<Tower>();
        nTower.floors = new Floor[height];
        nTower.width = width;
        nTower.depth = depth;
        
        GameManager.Instance.parentTower = nTower;

        nTower.floors = new Floor[height];

        for (int level = 0; level < height; level++)
        {
            CreateFloor(towerJObject[$"Floor{level}"], level, nTower);
        }
        
        StartCoroutine(SpawnBot());
        
        GameManager.Instance.InitScoring(plantConfigs);
        GameActionGoldenFinger.Instance.Init();
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
        Debug.Log("floorGridx.Count: " + floorGrids.Count);
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
        //is this the y in the old version? yes - ziyu
        int z = gridIdx / width;
        
        int floorOffsetY = parentFloor.floorNumber * (depth + 1);
        
        GameObject gridObj;
        string tileType = gridJObject["GridType"].ToString();
        switch (tileType)
        {
            case "Soil":
                gridObj = Instantiate(soilPrefab, new Vector3(x, z + floorOffsetY * parentFloor.floorNumber, 0.0f), Quaternion.identity);
                break;
            default:
                gridObj = Instantiate(stationPrefab, new Vector3(x, z + floorOffsetY * parentFloor.floorNumber, 0.0f), Quaternion.identity);
                break;
        }

        //TODO - this will need to be rebuilt for the new station system
        // setup stations
        if (tileType != "Soil")
        {
            gridObj.name = $"{tileType} {x},{z}";
            gridObj.transform.parent = parentFloor.gameObject.transform;
            StationCellBehavior nStation = gridObj.GetComponent<StationCellBehavior>();
            nStation.parentFloor = parentFloor;
            nStation.gridX = x;
            nStation.gridZ = z;

            SpriteRenderer spriteRenderer = gridObj.GetComponent<SpriteRenderer>();
            //switch (tileType)
            //{
            //    case "Harvest":
            //        nStation.tileType = TileType.HarvestStation;
            //        //spriteRenderer.sprite = SpriteResources.Instance.harvestStation;
            //        break;
            //    case "Pluck":
            //        nStation.tileType = TileType.PluckStation;
            //        //spriteRenderer.sprite = SpriteResources.Instance.pluckStation;
            //        break;
            //    case "Plant":
            //        nStation.tileType = TileType.PlantStation;
            //        //spriteRenderer.sprite = SpriteResources.Instance.plantStation;
            //        break;
            //    case "Sample":
            //        nStation.tileType = TileType.SampleStation;
            //        //spriteRenderer.sprite = SpriteResources.Instance.sampleStation;
            //        break;
            //    case "SprayA":
            //        nStation.tileType = TileType.SprayAStation;
            //        //spriteRenderer.sprite = SpriteResources.Instance.sprayAStation;
            //        break;
            //    case "SprayB":
            //        nStation.tileType = TileType.SprayBStation;
            //        //spriteRenderer.sprite = SpriteResources.Instance.sprayBStation;
            //        break;
            //    case "SprayC":
            //        nStation.tileType = TileType.SprayCStation;
            //        //spriteRenderer.sprite = SpriteResources.Instance.sprayCStation;
            //        break;
            //    case "SprayD":
            //        nStation.tileType = TileType.SprayDStation;
            //        //spriteRenderer.sprite = SpriteResources.Instance.sprayDStation;
            //        break;
            //    case "Till":
            //        nStation.tileType = TileType.TillStation;
            //        //spriteRenderer.sprite = SpriteResources.Instance.tillStation;
            //        break;
            //    case "Discard":
            //        nStation.tileType = TileType.DiscardStation;
            //        //spriteRenderer.sprite = SpriteResources.Instance.discardStation;
            //        break;
            //    case "Score":
            //        nStation.tileType = TileType.ScoreStation;
            //        //spriteRenderer.sprite = SpriteResources.Instance.scoreStation;
            //        break;
            //}

            parentFloor.Cells[x, z] = nStation;
            return;
        }

        gridObj.name = $"Grid {x},{z}";
        gridObj.GetComponent<SpriteRenderer>().color = (x + z + parentFloor.floorNumber) % 2 == 0 ? grid2DTheme.lightColor : grid2DTheme.darkColor;
        gridObj.transform.parent = parentFloor.gameObject.transform;

        SoilCellBehavior nSoilCell = gridObj.GetComponent<SoilCellBehavior>();
        nSoilCell.parentFloor = parentFloor;
        nSoilCell.gridX = x;
        nSoilCell.gridZ = z;
        
        // add soil config
        string soilConfig = (string)gridJObject["SoilConfig"];
        if (!_soilConfigs.ContainsKey(soilConfig)) CreateSoilConfig(soilConfig);
        nSoilCell.soilConfig = _soilConfigs[soilConfig];
        
        // set properties
        Vector4 nutrients = Vector4FromJTokenList(gridJObject["NutrientLevels"]["nutrients"].ToList());
        NutrientSolution nutrientSolution =
            new NutrientSolution((float)gridJObject["NutrientLevels"]["water"], nutrients);

        //not sure if this works in networked version (INetworkStruct)
        nSoilCell.NutrientLevels = nutrientSolution;
        
        JToken plants = gridJObject["Plants"];
        if (plants is JObject plantsObj) {
            foreach (var property in plantsObj.Properties()) {
                JToken plantJToken = property.Value;
                CreatePlant(plantJToken, nSoilCell);
            }
        }

        parentFloor.Cells[x, z] = nSoilCell;
    }

    private void CreatePlant(JToken plantJToken, SoilCellBehavior parentCell)
    {
        string plantConfigName = (string)plantJToken["config"];
        if (!plantConfigs.ContainsKey(plantConfigName)) CreatePlantConfig(plantConfigName);

        //GameObject plantObj = Instantiate(plantPrefab, plantSlot.position, quaternion.identity, plantSlot);
        GameObject plantObj = PrefabPooler.Instance.InstantiatePrefab("plant");
        PlantBehavior nPlant = plantObj.GetComponent<PlantBehavior>();

        NutrientSolution nutrientSolution = new NutrientSolution(
            (float)plantJToken["water"],
            Vector4FromJTokenList(plantJToken["nutrient"].ToList()));

        nPlant.SetPlantState(new PlantStateData(
            plantConfigs[plantConfigName],
            nutrientSolution,
            (float)plantJToken["rootMass"],
            (float)plantJToken["height"],
            (float)plantJToken["energyLevel"],
            (int)plantJToken["age"]));

        parentCell.AddPlant(nPlant);
    }

    private void CreatePlantConfig(string configName)
    {
        JToken planConfigJToken = _configJObject[configName];
        if (planConfigJToken == null)
        {
            Debug.LogError($"MapGenerator tries to retrieve config:{configName} but it does not exist");
            return;
        }
        
        PlantConfigSO plantConfig = ScriptableObject.CreateInstance<PlantConfigSO>();

        plantConfig.speciesName = configName;
        plantConfig.waterCapacity = (float)planConfigJToken["capacities"];
        plantConfig.uptakeRate = (float)planConfigJToken["uptakeRate"];
        plantConfig.metabolismRate = (float)planConfigJToken["metabolismRate"];
        plantConfig.metabolismFactor = Vector4FromJTokenList(planConfigJToken["metabolismFactor"].ToList());
        plantConfig.rootHeightTransition = (float)planConfigJToken["rootHeightTransition"];
        plantConfig.growthToleranceThreshold = (float)planConfigJToken["growthToleranceThreshold"];
        plantConfig.leachingEnergyThreshold = (float)planConfigJToken["leachingEnergyThreshold"];
        plantConfig.leachingFactor = Vector4FromJTokenList(planConfigJToken["leachingFactor"].ToList());
        plantConfig.plantSprites = PlantToSpriteCapturer.Instance.CaptureAllStagesAtOnce();
        plantConfigs[configName] = plantConfig;
    }

    private void CreateSoilConfig(string configName)
    {
        JToken soilConfigJToken = _configJObject[configName];
        float water = (float)soilConfigJToken["capacities"];

        SoilConfigSO nSoilConfig = ScriptableObject.CreateInstance<SoilConfigSO>();
        nSoilConfig.drainRate = (float)soilConfigJToken["drainTime"];
        nSoilConfig.waterCapacity = water;

        _soilConfigs[configName] = nSoilConfig;
    }

    private Vector4 Vector4FromJTokenList(List<JToken> list)
    {
        return new Vector4(
            (float)list[0], (float)list[1], 
            (float)list[2], (float)list[3]);
    }
}
