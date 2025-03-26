using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameConstant;
using UnityEngine;

public class PlantMathDataLogger : MonoBehaviour
{
    public enum TrackedPlantDataType
    {
        Health,
        Energy,
        RootMass,
        Height,
        Water,
        NutrientA,
        NutrientB,
        NutrientC,
        NutrientD,
        Age,
    }

    public enum TrackedSoilDataType
    {
        
    }
    
    private List<PlantBehaviorLocalTest> _trackedPlants;
    private Dictionary<PlantBehaviorLocalTest, Dictionary<TrackedPlantDataType, List<float>>> _plantDataBanks;
    private List<GridBehaviorLocalTest> _trackedSoil;
    private Dictionary<GridBehaviorLocalTest, Dictionary<TrackedSoilDataType, List<float>>> _soilDataBanks;

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
            _plantDataBanks[plant][TrackedPlantDataType.Health].Add(plant.Health);
            _plantDataBanks[plant][TrackedPlantDataType.Energy].Add(plant.EnergyLevel);
            _plantDataBanks[plant][TrackedPlantDataType.RootMass].Add(plant.RootMass);
            _plantDataBanks[plant][TrackedPlantDataType.Height].Add(plant.Height);
            _plantDataBanks[plant][TrackedPlantDataType.Water].Add(plant.WaterLevel);
            _plantDataBanks[plant][TrackedPlantDataType.NutrientA].Add(plant.NutrientLevels.nutrients.x);
            _plantDataBanks[plant][TrackedPlantDataType.NutrientB].Add(plant.NutrientLevels.nutrients.y);
            _plantDataBanks[plant][TrackedPlantDataType.NutrientC].Add(plant.NutrientLevels.nutrients.z);
            _plantDataBanks[plant][TrackedPlantDataType.NutrientD].Add(plant.NutrientLevels.nutrients.w);
            _plantDataBanks[plant][TrackedPlantDataType.Age].Add(plant.Age);

            Window_Graph.Instance.ShowGraph(_plantDataBanks[plant][TrackedPlantDataType.Energy], -1, i => $"tick{i}", f => $"{f}");
        }
    }

    public void Retract()
    {
        foreach (PlantBehaviorLocalTest plant in _trackedPlants)
        {
            Dictionary<TrackedPlantDataType, List<float>> dataBank = _plantDataBanks[plant];
            if (dataBank[TrackedPlantDataType.NutrientA].Count < 1) return;
            plant.SetInitialProperties(new PlantInitInfo(
                dataBank[TrackedPlantDataType.RootMass].RemoveLastReturnNewLast(),
                dataBank[TrackedPlantDataType.Height].RemoveLastReturnNewLast(),
                dataBank[TrackedPlantDataType.Energy].RemoveLastReturnNewLast(),
                dataBank[TrackedPlantDataType.Health].RemoveLastReturnNewLast(),
                dataBank[TrackedPlantDataType.Age].RemoveLastReturnNewLast(),
                dataBank[TrackedPlantDataType.Water].RemoveLastReturnNewLast(),
                new Vector4(
                    dataBank[TrackedPlantDataType.NutrientA].RemoveLastReturnNewLast(), 
                    dataBank[TrackedPlantDataType.NutrientB].RemoveLastReturnNewLast(), 
                    dataBank[TrackedPlantDataType.NutrientC].RemoveLastReturnNewLast(), 
                    dataBank[TrackedPlantDataType.NutrientD].RemoveLastReturnNewLast())
            ));
            
            Window_Graph.Instance.ShowGraph(_plantDataBanks[plant][TrackedPlantDataType.Energy], -1, i => $"tick{i}", f => $"{f}");
        }
    }
}

public static class ListExtensions
{
    public static T RemoveLastReturnNewLast<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
            throw new InvalidOperationException("The list is empty.");

        int lastIndex = list.Count - 1;
        T lastElement = list[lastIndex-1];
        list.RemoveAt(lastIndex);
        return lastElement;
    }
}
