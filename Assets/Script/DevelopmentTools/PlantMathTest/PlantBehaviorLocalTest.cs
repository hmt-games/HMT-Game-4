using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Fusion;
using GameConstant;

/// <summary>
/// The only difference between this and PlantBehavior we use in game is to
/// remove the [Networked] tag from properties. As without with we must network
/// spawn this object before we use it in local test.
/// Thus, the idea is after we modify the desired functions (mostly just the OnTick)
/// we just copy and paste it back to PlantBehavior.
/// All sprite rendering code is also removed since we don't need graphics
/// </summary>
public class PlantBehaviorLocalTest : MonoBehaviour
{

    /// <summary>
    /// Plants can in principle reach to multiple cells to draw resources from, but need to figure out the best way to represent this
    /// </summary>
    public GridCellBehavior parentCell;

    private float _rootMass { get; set; } = 0;
    public float RootMass { 
        get {
            return _rootMass;
        }
        private set {
            _rootMass = Mathf.Max(0, value);
        } 
    }


    private float _height { get; set; } = 0;
    public float Height {
        get {
            return _height;
        }
        private set {
            _height = Mathf.Max(0, value);
        }
    }   
    public float WaterLevel {
        get {
            return NutrientLevels.water;
        }
    }
    public float EnergyLevel { get; private set; } = 0;

    public float Health { get; set; } = 0;

    public float Age { get; private set; } = 0;

    public NutrientSolution NutrientLevels;

    public PlantConfigSO config;

    public int plantCurrentStage = 0;
    private int _plantMaxStage = 3;
    //TODO: make this configurable in JSON spec
    private List<float> _stageTransitionThreshold = new List<float> { 0.0f, 1.5f, 3.0f, 5.3f };
    public bool hasFruit = false;

    //TODO: move to plant config
    public int maxHealthHistory = 10;

    private float healthTotal = 0;
    private Queue<float> healthHistory;

    public void SetInitialProperties(PlantInitInfo plantInitInfo)
    {
        _rootMass = plantInitInfo.RootMass;
        _height = plantInitInfo.Height;
        EnergyLevel = plantInitInfo.EnergyLevel;
        healthHistory = new Queue<float>();
        for (int i = 0; i < maxHealthHistory; i++)
        {
            healthHistory.Enqueue(plantInitInfo.Health);
        }
        Health = healthHistory.Sum() / healthHistory.Count;

        Age = plantInitInfo.Age;
        NutrientLevels = new NutrientSolution(plantInitInfo.Water, plantInitInfo.Nutrient);
        PlantNextStage();
    }
    
    public NutrientSolution OnTick(NutrientSolution allocation) {
        // UPTAKE
        float uptakeVolume = Mathf.Min(config.waterCapacity - NutrientLevels.water, config.uptakeRate);
        NutrientLevels += allocation.DrawOff(uptakeVolume);

        //METABOLISM
        NutrientSolution metabolismDraw = NutrientLevels.DrawOff(config.metabolismRate);
        float tick_energy = Vector4.Dot(metabolismDraw.nutrients, config.metabolismFactor);
        Vector4 idealDraw = Vector4.one * config.metabolismRate;
        float tick_health = Mathf.Clamp01(tick_energy / Vector4.Dot(config.metabolismFactor.Positives(), idealDraw));
        
        //HEALTH
        if (healthHistory.Count == maxHealthHistory) healthHistory.Dequeue();
        healthHistory.Enqueue(tick_health);
        Health = healthHistory.Sum() / healthHistory.Count;

        //GROWTH
        if (Health > config.growthToleranceThreshold)
        {
            float growth = Mathf.Max(0.0f, tick_energy);
            RootMass += growth * config.PercentToRoots(Age);
            Height += growth * (1 - config.PercentToRoots(Age));
        }

        //LEECH
        EnergyLevel += tick_energy;
        if (EnergyLevel >= config.leachingEnergyThreshold)
        {
            allocation.nutrients += EnergyLevel * config.leachingFactor;
            EnergyLevel = 0;
        }

        Age++;

        Debug.Log(allocation);
        return allocation;
    }

    /// <summary>
    /// Handles when water lands on the plant.
    /// 
    /// By default this should be a no-op but is here so it can be overridden by possible future Traits.
    /// </summary>
    /// <param name="waterVolume"></param>
    /// <returns></returns>
    public NutrientSolution OnWater(NutrientSolution waterVolume) {
        //if (config.onWaterCallbackBypass) return waterVolume;
        return waterVolume; //TODO do something meaningful here
    }
    
    private void PlantNextStage()
    {
        if (plantCurrentStage == _plantMaxStage) return;

        while (Height >= _stageTransitionThreshold[plantCurrentStage + 1])
        {
            plantCurrentStage++;
            if (plantCurrentStage == _plantMaxStage) break;
        }
        
        BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();

        if (plantCurrentStage == _plantMaxStage)
        {
            hasFruit = true;
        }
    }
}