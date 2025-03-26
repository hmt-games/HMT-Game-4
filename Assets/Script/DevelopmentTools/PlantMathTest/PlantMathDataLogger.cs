using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantMathDataLogger : MonoBehaviour
{
    public enum TrackedPlantDataType
    {
        PlantHealth,
        PlantEnergy
    }
    
    private List<PlantBehaviorLocalTest> _trackedPlants;
    private Dictionary<PlantBehaviorLocalTest, Dictionary<TrackedPlantDataType, List<float>>> _plantDataBanks;

    public static PlantMathDataLogger Instance;

    private void Awake()
    {
        Instance = this;
        
        _trackedPlants = new List<PlantBehaviorLocalTest>();
        _plantDataBanks = new Dictionary<PlantBehaviorLocalTest, Dictionary<TrackedPlantDataType, List<float>>>();
    }

    public void AddPlantToTrackList(PlantBehaviorLocalTest plant)
    {
        _trackedPlants.Add(plant);
        _plantDataBanks[plant] = new Dictionary<TrackedPlantDataType, List<float>>();
        // enumerate enum referencing: https://stackoverflow.com/questions/105372/how-to-enumerate-an-enum
        foreach (TrackedPlantDataType type in (TrackedPlantDataType[]) Enum.GetValues(typeof(TrackedPlantDataType)))
        {
            _plantDataBanks[plant][type] = new List<float>();
        }
    }

    public void OnTick()
    {
        foreach (PlantBehaviorLocalTest plant in _trackedPlants)
        {
            _plantDataBanks[plant][TrackedPlantDataType.PlantHealth].Add(plant.Health);
            _plantDataBanks[plant][TrackedPlantDataType.PlantEnergy].Add(plant.EnergyLevel);
            
            Window_Graph.Instance.ShowGraph(_plantDataBanks[plant][TrackedPlantDataType.PlantEnergy], -1, i => $"tick{i}", f => $"{f}");
        }
    }
}
