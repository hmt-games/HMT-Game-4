using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using GameConstant;
using HMT.Puppetry;
using Unity.Mathematics;
using GameConfig;
using Random = UnityEngine.Random;

public class GameActions : MonoBehaviour
{
    public static GameActions Instance;

    [SerializeField] private GameObject plantPrefab;

    private PlantInitInfo _plantInitInfo =
        new PlantInitInfo(1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 5.0f,
            new Vector4(5.0f, 5.0f, 5.0f, 5.0f));

    private void Awake()
    {
        if (Instance) Destroy(this);
        else Instance = this;
    }

    /// <summary>
    /// Fruits are removed and added to actors inventory.
    /// Target plant must bear fruit. Plants go back to mature stage.
    /// </summary>
    /// /// <param name="targetPlant"></param>
    public void Pick(PlantBehavior targetPlant)
    {

    }

    public void Pick(PlantBehavior targetPlant, FarmPuppetBot bot)
    {
        targetPlant.SurfaceMass = targetPlant.stageTransitionThreshold[2];
        targetPlant.plantCurrentStage = 0;
        targetPlant.PlantNextStage();

        // add fruit to bot inventory
        //TODO: considering object pooling plant objects to save head time
        int yield = Random.Range(PickConfig.harvestAmountMin, PickConfig.harvestAmountMax + 1);
        for (int i = 0; i < yield; i++)
        {
            if (bot.plantInventory.Count > bot.plantInventoryCapacity) break;

            NetworkObject nPlantObj =
                BasicSpawner._runner.Spawn(plantPrefab, new Vector3(9999, 9999, -9999), quaternion.identity);
            PlantBehavior nPlant = nPlantObj.GetComponent<PlantBehavior>();
            nPlant.config = targetPlant.config;
            _plantInitInfo.Nutrient = nPlant.config.metabolismFactor;
            _plantInitInfo.Water = nPlant.config.metabolismRate;
            nPlant.SetInitialProperties(_plantInitInfo);
            nPlant.GetComponent<SpriteRenderer>().sprite = nPlant.config.plantSprites[0];

            bot.plantInventory.Add(nPlant);
        }
    }

    public bool RequestPick(SoilCellBehavior tile, FarmPuppetBot bot)
    {
        Debug.Log("request Harvest");
        List<PlantBehavior> fertilePlants = new List<PlantBehavior>();
        foreach (PlantBehavior plant in tile.plants)
        {
            if (plant.hasFruit) fertilePlants.Add(plant);
        }

        if (fertilePlants.Count == 0) return false;

        PlantSelectionUIManager.Instance.ShowSelection(fertilePlants, bot, StartPick);
        return true;
    }


    private void StartPick(PlantBehavior plant, FarmPuppetBot bot)
    {
        StartCoroutine(bot.StartPick(plant));
    }

    public bool RequestPlant(FarmPuppetBot bot)
    {
        List<PlantBehavior> seedPlants = new List<PlantBehavior>();
        foreach (PlantBehavior plant in bot.plantInventory)
        {
            if (plant.Age == 0.0f) seedPlants.Add(plant);
        }

        if (seedPlants.Count == 0) return false;

        PlantSelectionUIManager.Instance.ShowSelection(seedPlants, bot, StartPlant);
        return true;
    }

    private void StartPlant(PlantBehavior plant, FarmPuppetBot bot)
    {
        StartCoroutine(bot.StartPlant(plant));
    }

    public void Plant(SoilCellBehavior tile, PlantBehavior plant, FarmPuppetBot bot)
    {
        plant.parentCell = tile;
        tile.plants.Add(plant);
        tile.plantCount += 1;

        plant.transform.SetParent(tile.transform.GetChild(1).GetChild(tile.plantCount - 1), false);
        plant.transform.GetComponent<NetworkTransform>().Teleport(plant.transform.parent.position);

        bot.plantInventory.Remove(plant);
    }

    /// <summary>
    /// distribute nutrient solution based on relative surface mass of plants
    /// </summary>
    /// <param name="targetGrid"></param>
    /// <param name="nutrientSolution"></param>
    public void Spray(SoilCellBehavior targetGrid, FarmPuppetBot bot)
    {
        float sprayAmount = Mathf.Min(bot.reservoirInventory.water, SprayConfig.SprayAmountPerAction);
        sprayAmount = Mathf.Min(targetGrid.RemainingWaterCapacity, sprayAmount);
        targetGrid.NutrientLevels += bot.reservoirInventory.DrawOff(sprayAmount);
    }

    public void SprayUp(StationCellBehavior tile, FarmPuppetBot bot)
    {
        Vector4 nutrients = tile.tileType switch
        {
            TileType.SprayAStation => new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
            TileType.SprayBStation => new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            TileType.SprayCStation => new Vector4(0.0f, 0.0f, 1.0f, 0.0f),
            TileType.SprayDStation => new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
            _ => throw new InvalidOperationException("Trying to spray up while not on spray station, this should never happen?")
        };

        float sprayUpAmount = Mathf.Min(SprayConfig.WaterAmount,
            bot.reservoirCapacity - bot.reservoirInventory.water);
        NutrientSolution sprayUpSolution = new NutrientSolution(sprayUpAmount,
            nutrients * (sprayUpAmount * SprayConfig.NutrientConcentration));

        bot.reservoirInventory += sprayUpSolution;
    }
    
    /// <summary>
    /// plant takes up nutrient solution
    /// 
    /// </summary>
    /// <param name="targetPlant"></param>
    /// <param name="nutrientSolution"></param>
    public void Spray(PlantBehavior targetPlant, NutrientSolution nutrientSolution)
    {
        
    }

    /// <summary>
    /// Add seed level plants to the tile
    /// </summary>
    /// <param name="species"></param>
    /// <param name="targetGrid"></param>
    public void Plant(PlantConfig species, GridCellBehavior targetGrid)
    {
        
    }
    
    /// <summary>
    /// An overload of the normal plant action. Plant plant with specific parameters.
    /// This overload should only be used in golden finger (internal play testing)
    /// </summary>
    /// <param name="species"></param>
    /// <param name="plantInitInfo"></param>
    /// <param name="targetGrid"></param>
    public void Plant(PlantConfig species, PlantInitInfo plantInitInfo, SoilCellBehavior targetGrid)
    {
        int plantSlotIdx = targetGrid.plantCount;
        if (plantSlotIdx >= GLOBAL_CONSTANTS.MAX_PLANT_COUNT_PER_TILE)
        {
            Debug.LogWarning($"$Trying to plant new plant in grid " +
                             $"{targetGrid.gameObject.name} while the grid is already full");
            return;
        }

        Transform plantSlot = targetGrid.transform.GetChild(1).GetChild(plantSlotIdx);
        NetworkObject plantObj = BasicSpawner._runner.Spawn(plantPrefab, Vector3.zero, Quaternion.identity);
        plantObj.transform.SetParent(plantSlot);
        PlantBehavior nPlant = plantObj.GetComponent<PlantBehavior>();
        nPlant.config = species;
        nPlant.SetInitialProperties(plantInitInfo);
        nPlant.GetComponent<SpriteRenderer>().sprite = nPlant.config.plantSprites[0];
        nPlant.parentCell = targetGrid;
        targetGrid.plants.Add(nPlant);
        targetGrid.plantCount++;
        plantObj.transform.localScale = Vector3.one;
    }

    public void Sample(PlantBehavior targetPlant)
    {
        
    }

    public void Sample(SoilCellBehavior targetGrid, FarmPuppetBot puppet)
    {
        puppet.reservoirInventory = targetGrid.NutrientLevels.DrawOff(1.0f);
        InventoryUIManager.Instance.UpdateWaterInventoryUI(puppet.reservoirInventory, 1.0f);
        InventoryUIManager.Instance.ShowInventory();
    }
}
