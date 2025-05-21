using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameConstant;
using HMT.Puppetry;
using Newtonsoft.Json.Linq;

public class PlantBehavior : MonoBehaviour, IPuppetPerceivable {
    /// <summary>
    /// Plants can in principle reach to multiple cells to draw resources from, but need to figure out the best way to represent this
    /// </summary>
    public SoilCellBehavior parentCell;

    public string ObjectID { get; private set; } = IPuppet.GenerateUniquePuppetID("plant");

    private float _rootMass { get; set; } = 0;
    public float RootMass {
        get {
            return _rootMass;
        }
        private set {
            _rootMass = Mathf.Max(0, value);
        }
    }

    private float _surfaceMass { get; set; } = 0;
    public float SurfaceMass {
        get {
            return _surfaceMass;
        }
        set {
            _surfaceMass = Mathf.Max(0, value);
        }
    }

    public float WaterLevel {
        get {
            return NutrientLevels.water;
        }
    }
    public float EnergyLevel { get; private set; } = 0;

    public float Health { get; private set; } = 0;

    public float Age { get; private set; } = 0;

    public NutrientSolution NutrientLevels = NutrientSolution.Empty;

    public PlantConfig config;

    public int plantCurrentStage = 0;
    private int _plantMaxStage = 3;
    //TODO: make this configurable in JSON spec
    public List<float> stageTransitionThreshold = new List<float> { 0.0f, 1.5f, 3.0f, 5.3f };
    public bool hasFruit = false;

    public int maxHealthHistory = 10;

    private float healthTotal = 0;
    private Queue<float> healthHistory;

    public BoxCollider2D boxCollider2D;
    public SpriteRenderer spriteRenderer;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetInitialProperties(PlantInitInfo plantInitInfo) {
        _rootMass = plantInitInfo.RootMass;
        _surfaceMass = plantInitInfo.Height;
        EnergyLevel = plantInitInfo.EnergyLevel;
        healthHistory = new Queue<float>();
        for (int i = 0; i < maxHealthHistory; i++) {
            healthHistory.Enqueue(plantInitInfo.Health);
        }

        Age = plantInitInfo.Age;
        NutrientLevels = new NutrientSolution(plantInitInfo.Water, plantInitInfo.Nutrient);

        PlantNextStage();
    }


    public NutrientSolution OnTick(NutrientSolution allocation) {
        // UPTAKE
        float uptakeVolume = Mathf.Min(config.waterCapacity - NutrientLevels.water, config.uptakeRate);
        NutrientLevels += allocation.DrawOff(uptakeVolume);
        Debug.Log(uptakeVolume);

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
        //TODO: height cap stop grow
        if (Health > config.growthToleranceThreshold)
        {
            float growth = Mathf.Max(0.0f, tick_energy);
            RootMass += growth * config.PercentToRoots(Age);
            SurfaceMass += growth * (1 - config.PercentToRoots(Age));
        }
        PlantNextStage();

        //LEECH
        EnergyLevel += tick_energy;
        if (EnergyLevel >= config.leachingEnergyThreshold)
        {
            allocation.nutrients += EnergyLevel * config.leachingFactor;
            EnergyLevel = 0;
        }

        Age++;

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
        if (config.onWaterCallbackBypass) return waterVolume;
        return waterVolume; //TODO do something meaningful here
    }

    public void PlantNextStage() {
        if (plantCurrentStage == _plantMaxStage) return;

        while (SurfaceMass >= stageTransitionThreshold[plantCurrentStage + 1]) {
            plantCurrentStage++;
            if (plantCurrentStage == _plantMaxStage) break;
        }
        
        Debug.Log($"plant display stage {plantCurrentStage}");
        spriteRenderer.sprite = config.plantSprites[plantCurrentStage];
        boxCollider2D.size = spriteRenderer.sprite.bounds.size;
        if (plantCurrentStage == _plantMaxStage) {
            hasFruit = true;
        }
        else
        {
            hasFruit = false;
        }
    }

    public JObject HMTStateRep(HMTStateLevelOfDetail lod) {
        JObject state = new JObject();
        switch (lod) {
            case HMTStateLevelOfDetail.Full:
                state["nutrirents"] = NutrientLevels.ToFlatJSON();
                state["root_mass"] = RootMass;
                state["energy_level"] = EnergyLevel;
                goto case HMTStateLevelOfDetail.Visible;

            case HMTStateLevelOfDetail.Visible:
                state["species"] = config.speciesName;
                state["growth_stage"] = plantCurrentStage;
                state["surface_mass"] = SurfaceMass;
                state["has_fruit"] = hasFruit;
                state["health"] = Health;
                state["age"] = Age;
                goto case HMTStateLevelOfDetail.Seen;

            case HMTStateLevelOfDetail.Seen:
                goto case HMTStateLevelOfDetail.Unseen;

            case HMTStateLevelOfDetail.Unseen:
                break;

            case HMTStateLevelOfDetail.None:
            default:
                break;
        }

        return state;
    }
}