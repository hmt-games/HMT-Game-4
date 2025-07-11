using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameConstant;
using HMT.Puppetry;
using Unity.Mathematics;
using GameConfig;
using Random = UnityEngine.Random;
using JetBrains.Annotations;

/// <summary>
/// In usage this class is taking the shape of how we used the NetworkMiddleware in Dice Adventure.
/// 
/// When we build the networking system that will probably come in handy.
/// </summary>
public class GameActions : MonoBehaviour
{
    public static GameActions Instance;

    [SerializeField] private GameObject plantPrefab;

    private PlantInitInfo _plantInitInfo =
        new PlantInitInfo(1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 5.0f,
            new Vector4(5.0f, 5.0f, 5.0f, 5.0f));

    public struct Defaults {
        public const float SampleVolumePerAction = 1.0f;
        public const float SprayAmountPerAction = 10.0f;
    }


    private void Awake()
    {
        if (Instance) Destroy(this);
        else Instance = this;
    }

    #region OldFarmPuppetBot Actions

    public void Till(SoilCellBehavior tile, OldFarmPuppetBot bot)
    {
        NutrientSolution nutrientSolution = NutrientSolution.Empty;

        foreach (PlantBehavior plant in tile.plants)
        {
            nutrientSolution += plant.NutrientLevels;
            Destroy(plant.gameObject);
        }

        //tile.PlantCount = 0;
        tile.OnWater(nutrientSolution);
    }

    public void Pick(PlantBehavior targetPlant, OldFarmPuppetBot bot)
    {
        List<PlantStateData> fruits = targetPlant.OnPick();
        for(int i = 0; i < fruits.Count; i++) {
            if (bot.Inventory.PlantInventory.Count >= bot.Inventory.PlantInventoryCapacity) break;
            bot.Inventory.PlantInventory.Add(fruits[i]);
        }

        //targetPlant.SurfaceMass = targetPlant.stageTransitionThreshold[2];
        //targetPlant.plantCurrentStage = 0;
        //targetPlant.CheckPlantStage();

        //// add fruit to bot inventory
        ////TODO: considering object pooling plant objects to save head time
        //int yield = Random.Range(PickConfig.harvestAmountMin, PickConfig.harvestAmountMax + 1);
        //for (int i = 0; i < yield; i++)
        //{
        //    if (bot.Inventory.PlantInventory.Count > bot.Inventory.PlantInventoryCapacity) break;

        //    GameObject nPlantObj =
        //        Instantiate(plantPrefab, new Vector3(9999, 9999, -9999), quaternion.identity);
        //    PlantBehavior nPlant = nPlantObj.GetComponent<PlantBehavior>();
        //    nPlant.config = targetPlant.config;
        //    _plantInitInfo.Nutrient = nPlant.config.metabolismFactor;
        //    _plantInitInfo.Water = nPlant.config.metabolismRate;
        //    nPlant.SetInitialProperties(_plantInitInfo);
        //    nPlant.GetComponent<SpriteRenderer>().sprite = nPlant.config.plantSprites[0];

        //    bot.Inventory.PlantInventory.Add(nPlant);
        //}
    }

    public bool RequestPick(SoilCellBehavior tile, OldFarmPuppetBot bot)
    {
        List<PlantStateData> fertilePlants = new List<PlantStateData>();
        foreach (PlantBehavior plant in tile.plants)
        {
            if (plant.hasFruit) fertilePlants.Add(plant.GetPlantState());
        }

        if (fertilePlants.Count == 0) return false;

        PlantSelectionUIManager.Instance.ShowSelection(fertilePlants, bot, StartPick);
        return true;
    }

    private void StartPick(PlantStateData plant, OldFarmPuppetBot bot)
    {
        //StartCoroutine(bot.StartPick(plant));
    }
    
    public bool RequestHarvest(SoilCellBehavior tile, OldFarmPuppetBot bot)
    {
        Debug.Log("request Harvest");
        List<PlantBehavior> plants = tile.plants;

        if (plants.Count == 0) return false;

        //PlantSelectionUIManager.Instance.ShowSelection(plants, bot, StartHarvest);
        return true;
    }

    private void StartHarvest(PlantBehavior plant, OldFarmPuppetBot bot)
    {
        StartCoroutine(bot.StartHarvest(plant));
    }

    public void Harvest(PlantBehavior plant, OldFarmPuppetBot bot)
    {

        bot.Inventory.PlantInventory.Add(plant.OnHarvest());

        //SoilCellBehavior soil = plant.parentCell;
        ////soil.PlantCount -= 1;
        //soil.plants.Remove(plant);
        //plant.transform.SetParent(bot.transform, true);
        //plant.transform.localPosition = Vector3.one * 9999.0f;
    }

    public bool RequestPlant(OldFarmPuppetBot bot)
    {
        List<PlantStateData> plants = bot.Inventory.PlantInventory;

        if (plants.Count == 0) return false;

        //PlantSelectionUIManager.Instance.ShowSelection(plants, bot, StartPlant);
        return true;
    }

    private void StartPlant(PlantStateData plant, OldFarmPuppetBot bot)
    {
        StartCoroutine(bot.StartPlant(plant));
    }

    public void Plant(SoilCellBehavior tile, PlantStateData plant, OldFarmPuppetBot bot)
    {
        
        tile.AddPlant(plant);
        //bot.Inventory.PlantInventory.Remove(plant);
        //plant.parentCell = tile;
        //tile.plants.Add(plant);
        //tile.PlantCount += 1;

        //plant.transform.SetParent(tile.transform.GetChild(1).GetChild(tile.PlantCount - 1), true);
        //plant.transform.localPosition = Vector3.zero;


    }

    /// <summary>
    /// distribute nutrient solution based on relative surface mass of plants
    /// </summary>
    /// <param name="targetGrid"></param>
    /// <param name="nutrientSolution"></param>
    public void Spray(SoilCellBehavior targetGrid, OldFarmPuppetBot bot)
    {
        float sprayAmount = Mathf.Min(bot.Inventory.ReservoirInventory.water, SprayConfig.SprayAmountPerAction);
        sprayAmount = Mathf.Min(targetGrid.RemainingWaterCapacity, sprayAmount);
        targetGrid.NutrientLevels += bot.Inventory.ReservoirInventory.DrawOff(sprayAmount);
    }

    public void SprayUp(StationCellBehavior tile, OldFarmPuppetBot bot)
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
            bot.Inventory.ReservoirCapacity - bot.Inventory.ReservoirInventory.water);
        NutrientSolution sprayUpSolution = new NutrientSolution(sprayUpAmount,
            nutrients * (sprayUpAmount * SprayConfig.NutrientConcentration));

        bot.Inventory.ReservoirInventory += sprayUpSolution;
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
    /// An overload of the normal plant action. Plant plant with specific parameters.
    /// This overload should only be used in golden finger (internal play testing)
    /// </summary>
    /// <param name="species"></param>
    /// <param name="plantInitInfo"></param>
    /// <param name="targetGrid"></param>
    //public void Plant(PlantConfigSO species, PlantInitInfo plantInitInfo, SoilCellBehavior targetGrid)
    //{
    //    int plantSlotIdx = targetGrid.PlantCount;
    //    if (plantSlotIdx >= GLOBAL_CONSTANTS.MAX_PLANT_COUNT_PER_TILE)
    //    {
    //        Debug.LogWarning($"$Trying to plant new plant in grid " +
    //                         $"{targetGrid.gameObject.name} while the grid is already full");
    //        return;
    //    }

    //    Transform plantSlot = targetGrid.transform.GetChild(1).GetChild(plantSlotIdx);
    //    GameObject plantObj = Instantiate(plantPrefab, plantSlot, false);
    //    plantObj.transform.localPosition = Vector3.zero;
    //    PlantBehavior nPlant = plantObj.GetComponent<PlantBehavior>();
    //    nPlant.config = species;
    //    nPlant.SetInitialProperties(plantInitInfo);
    //    nPlant.GetComponent<SpriteRenderer>().sprite = nPlant.config.plantSprites[0];
    //    nPlant.parentCell = targetGrid;
    //    targetGrid.plants.Add(nPlant);
    //    //targetGrid.PlantCount++;
    //}

    public void Sample(SoilCellBehavior targetGrid, OldFarmPuppetBot puppet)
    {
        puppet.Inventory.ReservoirInventory = targetGrid.NutrientLevels.DrawOff(1.0f);
        InventoryUIManager.Instance.UpdateWaterInventoryUI(puppet.Inventory.ReservoirInventory, 1.0f);
        InventoryUIManager.Instance.ShowInventory();
    }

    public void Score(OldFarmPuppetBot bot)
    {
        if (bot.Inventory.PlantInventoryCapacity == 0) return;

        Dictionary<string, int> submittedPlant = new Dictionary<string, int>();
        for (int i = bot.Inventory.PlantInventory.Count - 1; i > -1; i--)
        {
            PlantStateData plant = bot.Inventory.PlantInventory[i];
            if (plant.age == 0)
            {
                string plantName = plant.config.speciesName;
                submittedPlant.TryAdd(plantName, 0);
                submittedPlant[plantName] += 1;
                bot.Inventory.PlantInventory.RemoveAt(i);
                //Destroy(plant.gameObject);
            }
        }
        
        GameManager.Instance.SubmitPlant(submittedPlant);
    }

    #endregion

    #region FarmBot Actions

    public void DumpInventory(FarmBot bot) {
        bot.Inventory.Clear();
    }

    public void Score(FarmBot bot) {
        if (bot.Inventory.PlantInventoryCapacity == 0) return;

        Dictionary<string, int> submittedPlant = new Dictionary<string, int>();
        for (int i = bot.Inventory.PlantInventory.Count - 1; i > -1; i--) {
            PlantStateData plant = bot.Inventory.PlantInventory[i];
            if (plant.age == 0) {
                string plantName = plant.config.speciesName;
                submittedPlant.TryAdd(plantName, 0);
                submittedPlant[plantName] += 1;
                bot.Inventory.PlantInventory.RemoveAt(i);
            }
        }

        GameManager.Instance.SubmitPlant(submittedPlant);
    }

    public NutrientSolution FillBotReservoir(FarmBot bot, NutrientSolution solution, bool specificAmount = false) {
        if (bot.Inventory.HasReservoir) {
            if (specificAmount) {
                bot.Inventory.AddNutrientSolution(solution);
            }
            else {
                bot.Inventory.AddNutrientSolution(solution * bot.Inventory.ReservoirCapacity);
            }
        }
        return solution;
    }

    public void FillBotInventory(FarmBot bot, PlantConfigSO plantConfig) {
        if (bot.Inventory.HasPlantInventory) {
            while (bot.Inventory.PlantInventory.Count < bot.Inventory.PlantInventoryCapacity) {
                bot.Inventory.PlantInventory.Add(new PlantStateData(plantConfig));
            }
        }
    }

    public void SwitchBotMode(FarmBot bot, BotModeSO newMode) {
        if (bot.BotModeConfig == newMode) return;
        bot.ChangeMode(newMode);
    }

    public void DropInventory(FarmBot bot, GridCellBehavior tile) {
        if (tile.AllowsDrops && !tile.hasInventoryDrop) {
            tile.AddInventoryBox(bot.Inventory);
            bot.Inventory.Clear();
        }
        else {
            Debug.LogWarning($"Trying to drop inventory on a tile that does not allow drops: {tile.ObjectID}");
        }
    }

    public void PickupInventory(FarmBot bot, GridCellBehavior tile) {
        if (tile.hasInventoryDrop) {
            bot.Inventory.TopOff(ref tile.InventoryDrop);
            if (tile.InventoryDrop.IsEmpty) {
                tile.RemoveInventoryBox();
            }
        }
        else {
            Debug.LogWarning($"Trying to pickup inventory from a tile that does not have an inventory drop: {tile.ObjectID}");
        }
    }

    /* 
    * We have unresolved design issues about how sampling should work.
    * 
    * Options:
    *  1. Sampling is just drawing a small amount of water into the bot's reservoir.
    *      a. This means you can't do it back to back without turing a sample in otherwise the samples mix
    *      b. You turn in the sample by dropping it a station and it goes to a central data store
    *  2. Sampling creates a "sample object" that lives in the plan inventory space
    *      a. you can turn in all of your samples at an anlysis station to report them to a data store
    *      b. we would need to slightly redesign the inventory to allow for this kind of mixed use slot
    *      c. would allow for back to back sampling
    *  3. Sampling takes the sample and immediately phones the result in to a central data store
    *      a. Does not require a sample object or turning things in
    *      
    *   For now this implements Option 3, though the old way implemented Option 1.
    */
    public void Sample(FarmBot bot, SoilCellBehavior tile, float sampleVolume) {
        SampleStore.Instance.AddSample(new SoilSample(
            tile.NutrientLevels.DrawOff(sampleVolume), Time.time, GameManager.Instance.currentTick,
            tile.parentFloor.floorNumber, tile.gridX, tile.gridZ, bot.PuppetID));
    }

    public void Spray(FarmBot bot, SoilCellBehavior tile, float volume) {
        if (bot.Inventory.HasReservoir) {
            tile.OnWater(bot.Inventory.ReservoirInventory.DrawOff(volume));
        }
        else {
            Debug.LogWarning($"Trying to spray on a tile without a reservoir: {tile.ObjectID}");
        }
    }

    public void Pick(FarmBot bot, SoilCellBehavior tile, int plantIdx) {
        PlantBehavior targetPlant = tile.plants[plantIdx];

        List<PlantStateData> fruits = targetPlant.OnPick();
        for (int i = 0; i < fruits.Count; i++) {
            if (bot.Inventory.PlantInventory.Count >= bot.Inventory.PlantInventoryCapacity) break;
            bot.Inventory.PlantInventory.Add(fruits[i]);
        }
    }

    public void Plant(FarmBot bot, SoilCellBehavior soilTile, int inventoryIdx) {
        if (inventoryIdx < 0) {
            inventoryIdx = 0;
        }
        if (inventoryIdx >= bot.Inventory.PlantInventory.Count) {
            Debug.LogWarning($"Trying to plant a plant with an invalid index: {inventoryIdx}");
            return;
        }
        PlantStateData plant = bot.Inventory.PlantInventory[inventoryIdx];

        soilTile.AddPlant(plant);
        bot.Inventory.PlantInventory.RemoveAt(inventoryIdx);
    }

    public void Harvest(FarmBot bot, SoilCellBehavior soilTile, int targetPlantIdx) {
        PlantBehavior targetPlant = soilTile.plants[targetPlantIdx];
        bot.Inventory.PlantInventory.Add(targetPlant.OnHarvest());
    }

    public void Till(FarmBot bot, SoilCellBehavior soilTile) {
        NutrientSolution nutrientSolution = NutrientSolution.Empty;
        foreach (PlantBehavior plant in soilTile.plants) {
            nutrientSolution += plant.OnTill();
        }
        soilTile.OnWater(nutrientSolution);
    }

    #endregion
}
