using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEngine;

public class MapGeneratorJSON : MonoBehaviour
{
    [SerializeField] private TextAsset configJSON;
    [SerializeField] private TextAsset towerJSON;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GridTheme grid2DTheme;
    [SerializeField] private List<GameObject> plantsPrefab;

    private Dictionary<string, PlantConfig> _plantConfigs;
    private Dictionary<string, SoilConfig> _soilConfigs;
    private Dictionary<string, GameObject> _name2Plant;
    private int _plantsIdxPointer = 0;

    private JObject _configJObject;
    
    private int _width;
    private int _depth;

    private void Awake()
    {
        _plantConfigs = new Dictionary<string, PlantConfig>();
        _soilConfigs = new Dictionary<string, SoilConfig>();
        _name2Plant = new Dictionary<string, GameObject>();
        _configJObject = JObject.Parse(configJSON.text);
    }

    private void Start()
    {
        CreateTower();
    }

    private void CreateTower()
    {
        JObject towerJObject = JObject.Parse(towerJSON.text);
        int width = (int)towerJObject["Tower"]["width"];
        int depth = (int)towerJObject["Tower"]["depth"];
        int height = (int)towerJObject["Tower"]["height"];
        Debug.Log($"Creating a tower with w{width}, d{depth}, h{height}");
        
        GameObject towerObj = new GameObject();
        towerObj.name = "Tower";
        Tower nTower = towerObj.AddComponent<Tower>();
        nTower.floors = new Floor[height];
        nTower.width = width;
        nTower.depth = depth;
        GameManager.Instance.parentTower = nTower;

        _width = width;
        _depth = depth;
        nTower.floors = new Floor[height];
        for (int level = 0; level < height; level++)
        {
            CreateFloor(towerJObject[$"Floor{level}"], level, nTower);
        }
    }

    private void CreateFloor(JToken floorJObject, int floorIdx, Tower parentTower)
    {
        GameObject floorObj = new GameObject();
        floorObj.name = $"Floor{floorIdx}";
        Floor nFloor = floorObj.AddComponent<Floor>();
        floorObj.transform.parent = parentTower.gameObject.transform;
        nFloor.parentTower = parentTower;
        nFloor.floorNumber = floorIdx;
        nFloor.Cells = new GridCellBehavior[_width, _depth];
        
        List<JToken> floorGrids = floorJObject.ToList();
        for (int gridIdx = 0; gridIdx < floorGrids.Count; gridIdx++)
        {
            CreateGrid(floorGrids[gridIdx], gridIdx, nFloor);
        }

        parentTower.floors[floorIdx] = nFloor;
    }

    private void CreateGrid(JToken gridJObject, int gridIdx, Floor parentFloor)
    {
        int x = gridIdx % _width;
        int z = gridIdx / _width;
        
        int floorOffsetY = parentFloor.floorNumber * (_depth + 1);
        
        GameObject gridObj = Instantiate(tilePrefab, new Vector3(x, z + floorOffsetY, 0.0f), Quaternion.identity);
        gridObj.name = $"Grid {x},{z}";
        gridObj.GetComponent<SpriteRenderer>().color = (x + z + parentFloor.floorNumber) % 2 == 0 ? grid2DTheme.lightColor : grid2DTheme.darkColor;
        gridObj.transform.parent = parentFloor.gameObject.transform;
        
        GridCellBehavior nGrid = gridObj.AddComponent<GridCellBehavior>();
        nGrid.parentFloor = parentFloor;
        nGrid.gridX = x;
        nGrid.gridZ = z;
        
        // add soil config
        string soilConfig = (string)gridJObject["soilConfig"];
        if (!_soilConfigs.ContainsKey(soilConfig)) CreateSoilConfig(soilConfig);
        nGrid.soilConfig = _soilConfigs[soilConfig];
        
        // set properties
        Vector4 nutrients = Vector4FromJTokenList(gridJObject["NutrientLevels"]["nutrients"].ToList());
        NutrientSolution nutrientSolution =
            new NutrientSolution((float)gridJObject["NutrientLevels"]["water"], nutrients);
        nGrid.NutrientLevels = nutrientSolution;
        
        // add all plants
        nGrid.rootedPlants = new List<PlantBehavior>();
        nGrid.surfacePlants = new List<PlantBehavior>();
        int plantIdx = 0;
        Transform plantsSlot = gridObj.transform.GetChild(1);
        JToken surfacePlants = gridJObject["surfacePlants"];
        if (surfacePlants is JObject surfacePlantsObj)
        {
            foreach (var property in surfacePlantsObj.Properties())
            {
                JToken plantJToken = property.Value;
                CreatePlant(plantJToken, nGrid, plantsSlot.GetChild(plantIdx), true);
                plantIdx++;
            }
        }

        parentFloor.Cells[x, z] = nGrid;
    }

    private void CreatePlant(JToken plantJToken, GridCellBehavior parentGrid, Transform plantSlot, bool isSurfacePlant)
    {
        string plantConfigName = (string)plantJToken["config"];
        if (!_plantConfigs.ContainsKey(plantConfigName)) CreatePlantConfig(plantConfigName);
        GameObject plantObj = Instantiate(_name2Plant[plantConfigName], plantSlot.position, quaternion.identity, plantSlot);
        PlantBehavior nPlant = plantObj.AddComponent<PlantBehavior>();
        nPlant.config = _plantConfigs[plantConfigName];
        nPlant.SetInitialProperties(
            (float)plantJToken["rootMass"],
            (float)plantJToken["height"],
            (float)plantJToken["energyLevel"],
            (float)plantJToken["health"],
            (float)plantJToken["age"],
            Vector4FromJTokenList(plantJToken["nutrient"].ToList()),
            (float)plantJToken["water"]);
        nPlant.parentCell = parentGrid;

        if (isSurfacePlant) parentGrid.surfacePlants.Add(nPlant);
        else parentGrid.rootedPlants.Add(nPlant);
    }

    private void CreatePlantConfig(string configName)
    {
        JToken planConfigJToken = _configJObject[configName];
        Vector4 capacities = Vector4FromJTokenList(planConfigJToken["capacities"].ToList());
        Vector4 uptakeRate = Vector4FromJTokenList(planConfigJToken["uptakeRate"].ToList());
        Vector4 metabolismNeeds = Vector4FromJTokenList(planConfigJToken["metabolismNeeds"].ToList());
        Vector4 metabolismFactor = Vector4FromJTokenList(planConfigJToken["metabolismFactor"].ToList());
        Vector4 growthConsumptionRateLimit = Vector4FromJTokenList(planConfigJToken["growthConsumptionRateLimit"].ToList());
        Vector4 growthFactor = Vector4FromJTokenList(planConfigJToken["growthFactor"].ToList());
        float rootHeightTransition = (float)planConfigJToken["rootHeightTransition"];
        float growthToleranceThreshold = (float)planConfigJToken["growthToleranceThreshold"];

        PlantConfig plantConfig = ScriptableObject.CreateInstance<PlantConfig>();
        plantConfig.capacities = capacities;
        plantConfig.uptakeRate = uptakeRate;
        plantConfig.metabolismNeeds = metabolismNeeds;
        plantConfig.metabolismFactor = metabolismFactor;
        plantConfig.growthConsumptionRateLimit = growthConsumptionRateLimit;
        plantConfig.growthFactor = growthFactor;
        plantConfig.rootHeightTransition = rootHeightTransition;
        plantConfig.growthToleranceThreshold = growthToleranceThreshold;

        _plantConfigs[configName] = plantConfig;
        _name2Plant[configName] = plantsPrefab[_plantsIdxPointer];
        _plantsIdxPointer = (_plantsIdxPointer + 1) % plantsPrefab.Count;
    }

    private void CreateSoilConfig(string configName)
    {
        JToken soilConfigJToken = _configJObject[configName];
        List<JToken> nutrientsList = soilConfigJToken["capacities"]["nutrients"].ToList();
        Vector4 nutrients = Vector4FromJTokenList(nutrientsList);
        float water = (float)soilConfigJToken["capacities"]["water"];
        NutrientSolution nutrientSolution = new NutrientSolution(water, nutrients);

        SoilConfig nSoilConfig = ScriptableObject.CreateInstance<SoilConfig>();
        nSoilConfig.drainTime = (float)soilConfigJToken["drainTime"];
        nSoilConfig.capacities = nutrientSolution;

        _soilConfigs[configName] = nSoilConfig;
    }

    private Vector4 Vector4FromJTokenList(List<JToken> list)
    {
        return new Vector4(
            (float)list[0], (float)list[1], 
            (float)list[2], (float)list[3]);
    }
}
