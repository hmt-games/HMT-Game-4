using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using GameConstant;
using Unity.Mathematics;

public class GameActions : MonoBehaviour
{
    public static GameActions Instance;
    
    [SerializeField] private GameObject plantPrefab;

    public List<GameObject> inventory;

    private void Awake()
    {
        if (Instance) Destroy(this);
        else Instance = this;

        inventory = new List<GameObject>();
    }

    public void Plant(PlantConfig species, PlantInitInfo plantInitInfo, GridCellBehavior targetGrid)
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

    public void Harvest(PlantBehavior targetPlant)
    {
        //TODO: implement some sort of OnHarvest behavior for plants
        //TODO: this should take some ticks to perform, while showing a progress bar
        GridCellBehavior plantGrid = targetPlant.parentCell;
        plantGrid.plants.Remove(targetPlant);
        plantGrid.plantCount--;

        targetPlant.transform.SetParent(transform);
        inventory.Add(targetPlant.gameObject);
        targetPlant.gameObject.SetActive(false);
    }
}
