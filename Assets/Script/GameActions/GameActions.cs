using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using GameConstant;
using HMT.Puppetry;
using Unity.Mathematics;
using GameConfig;

public class GameActions : MonoBehaviour
{
    public static GameActions Instance;
    
    [SerializeField] private GameObject plantPrefab;

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

    /// <summary>
    /// Plant is removed from its tile and added to actors inventory
    /// Target plant must have surface mass
    /// </summary>
    /// <param name="targetPlant"></param>
    public void Harvest(PlantBehavior targetPlant, PuppetBehavior bot)
    {
        targetPlant.SurfaceMass = targetPlant.stageTransitionThreshold[2];
        targetPlant.plantCurrentStage = 0;
        targetPlant.PlantNextStage();

        //TODO: add fruit to bot inventory
    }

    public bool RequestHarvest(SoilCellBehavior tile, PuppetBehavior bot)
    {
        Debug.Log("request Harvest");
        List<PlantBehavior> fertilePlants = new List<PlantBehavior>();
        foreach (PlantBehavior plant in tile.plants)
        {
            Debug.Log("all to list");
            if (plant.hasFruit) fertilePlants.Add(plant);
        }

        if (fertilePlants.Count == 0) return false;

        PlantSelectionUIManager.Instance.ShowSelection(fertilePlants, bot, StartHarvest);
        return true;
    }

    private void StartHarvest(PlantBehavior plant, PuppetBehavior bot)
    {
        StartCoroutine(bot.StartHarvest(plant));
    }

    /// <summary>
    /// distribute nutrient solution based on relative surface mass of plants
    /// </summary>
    /// <param name="targetGrid"></param>
    /// <param name="nutrientSolution"></param>
    public void Spray(SoilCellBehavior targetGrid, PuppetBehavior bot)
    {
        float sprayAmount = Mathf.Min(bot.WaterInventory.water, SprayConfig.SprayAmountPerAction);
        sprayAmount = Mathf.Min(targetGrid.RemainingWaterCapacity, sprayAmount);
        targetGrid.NutrientLevels += bot.WaterInventory.DrawOff(sprayAmount);
    }

    public void SprayUp(StationCellBehavior tile, PuppetBehavior bot)
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
            bot.SolutionInventoryCapacity - bot.WaterInventory.water);
        NutrientSolution sprayUpSolution = new NutrientSolution(sprayUpAmount,
            nutrients * (sprayUpAmount * SprayConfig.NutrientConcentration));

        bot.WaterInventory += sprayUpSolution;
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

    public void Sample(SoilCellBehavior targetGrid, PuppetBehavior puppet)
    {
        puppet.WaterInventory = targetGrid.NutrientLevels.DrawOff(1.0f);
        InventoryUIManager.Instance.UpdateWaterInventoryUI(puppet.WaterInventory, 1.0f);
        InventoryUIManager.Instance.ShowInventory();
    }
}
