using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameConstant;
using HMT.Puppetry;
using Newtonsoft.Json.Linq;
using PlantSystem.Traits;

[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class PlantBehavior : MonoBehaviour, IPuppetPerceivable, IPoolCallbacks {
    /// <summary>
    /// Plants can in principle reach to multiple cells to draw resources from, but need to figure out the best way to represent this
    /// </summary>
    public SoilCellBehavior parentCell { get; set; }

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

    public int Age { get; private set; } = 0;

    public NutrientSolution NutrientLevels = NutrientSolution.Empty;

    public PlantConfigSO config;

    public List<PlantTrait> Traits { get; private set; }

    public int plantCurrentStage = 0;
    private int _plantMaxStage = 3;
    //TODO: make this configurable in JSON spec
    //public List<float> stageTransitionThreshold = new List<float> { 0.0f, 1.5f, 3.0f, 5.3f };
    public bool hasFruit = false;

    private bool _inInventory = false;
    public bool InInventory {
        get {
            return _inInventory;
        }
        set {
            _inInventory = value;
            if (spriteRenderer != null) {
                spriteRenderer.enabled = !value;
            }
        }
    }

    public Sprite CurrentStageSprite {
        get {
            return config.plantSprites[plantCurrentStage];
        }
    }


    public float[] healthHistory;
    public int currentHealthIndex { get; private set; }

    private BoxCollider2D boxCollider2D;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetPlantState(new PlantStateData(config));
    }

    public void SetPlantState(PlantStateData state) {
        RootMass = state.rootMass;
        SurfaceMass = state.surfaceMass;
        NutrientLevels = state.nutrientLevels;
        EnergyLevel = state.energyLevel;
        Age = state.age;
        plantCurrentStage = state.currentStage;

        healthHistory = state.healthHistory;
        currentHealthIndex = state.currentHealthIndex;
        
        var tot = 0f;
        var count = 0;
        for (int i = 0; i < healthHistory.Length; i++) {
            if (!float.IsNaN(healthHistory[i])) {
                tot += healthHistory[i];
                count++;
            }
        }
        Health = count > 0 ? tot / count : float.NaN; // Calculate average health from history

        spriteRenderer.sprite = config.plantSprites[plantCurrentStage];
        boxCollider2D.size = spriteRenderer.sprite.bounds.size;
        CheckPlantStage();
    }

    public PlantStateData GetPlantState() {
        return new PlantStateData(config, NutrientLevels, RootMass, SurfaceMass, EnergyLevel, healthHistory, currentHealthIndex, Age, plantCurrentStage);
    }


    //public void SetInitialProperties(PlantInitInfo plantInitInfo) {
    //    _rootMass = plantInitInfo.RootMass;
    //    _surfaceMass = plantInitInfo.Height;
    //    EnergyLevel = plantInitInfo.EnergyLevel;
    //    healthHistory = new Queue<float>();
    //    for (int i = 0; i < maxHealthHistory; i++) {
    //        healthHistory.Enqueue(plantInitInfo.Health);
    //    }

    //    Age = plantInitInfo.Age;
    //    NutrientLevels = new NutrientSolution(plantInitInfo.Water, plantInitInfo.Nutrient);

    //    CheckPlantStage();
    //}


    public NutrientSolution OnTick(NutrientSolution allocation) {
        bool proceed = true;
        foreach(PlantTrait trait in Traits) {
            proceed &= trait.OnTickPre(this, ref allocation);
        }
        if (!proceed) {
            return allocation;
        }

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
        healthHistory[currentHealthIndex] = tick_health;
        currentHealthIndex = (currentHealthIndex + 1) % healthHistory.Length;
        var tot = 0f;
        var count = 0;
        for (int i = 0; i < healthHistory.Length; i++) {
            if (!float.IsNaN(healthHistory[i])) {
                tot += healthHistory[i];
                count++;
            }
        }
        Health = count > 0 ? tot / count : float.NaN; // Calculate average health from history


        //GROWTH
        //TODO: height cap stop grow
        if (Health > config.growthToleranceThreshold)
        {
            float growth = Mathf.Max(0.0f, tick_energy);
            RootMass += growth * config.PercentToRoots(Age);
            SurfaceMass += growth * (1 - config.PercentToRoots(Age));
        }
        CheckPlantStage();

        //LEECH
        EnergyLevel += tick_energy;
        if (EnergyLevel >= config.leachingEnergyThreshold)
        {
            allocation.nutrients += EnergyLevel * config.leachingFactor;
            EnergyLevel = 0;
        }

        Age++;

        foreach(PlantTrait trait in Traits) {
            trait.OnTickPost(this, ref allocation);
        }

        return allocation;
    }

    #region Bot Action Responses

    /// <summary>
    /// Handles when water lands on the plant.
    /// 
    /// By default this should be a no-op but is here so it can be overridden by possible future Traits.
    /// </summary>
    /// <param name="waterVolume"></param>
    /// <returns></returns>
    public NutrientSolution OnSpray(NutrientSolution waterVolume) {
        foreach (PlantTrait trait in Traits) {
            trait.OnSpray(this, ref waterVolume);
        }
        return waterVolume; //TODO do something meaningful here
    }

    public PlantStateData OnHarvest(FarmBot farmBot) {
        bool proceed = true;
        PlantStateData altReturn = PlantStateData.Empty;
        foreach(PlantTrait trait in Traits) {
            proceed &= trait.OnHarvest(this, farmBot, ref altReturn);
        }
        if (!proceed) {
            return altReturn;
        }
        else {
            parentCell.RemovePlant(this);
            PrefabPooler.Instance.ReleasePrefabInstance("plant", gameObject);
            return this.GetPlantState();
        }
    }

    public List<PlantStateData> OnPick(FarmBot farmBot) {
        if(!hasFruit)
            return new List<PlantStateData>();
        List<PlantStateData> fruits = new List<PlantStateData>();

        bool proceed = true;
        foreach(PlantTrait trait in Traits) {
            proceed &= trait.OnPick(this, farmBot, fruits);
        }

        if (!proceed) { 
            return fruits; 
        }
        else {
            int yield = config.GenerateYield();
            for (int i = 0; i < yield; i++) {
                fruits.Add(new PlantStateData(config));
            }

            ClearFruits();
            return fruits;
        }
    }

    public NutrientSolution OnTill (FarmBot farmBot) {
        bool proceed = true;
        NutrientSolution altReturn = NutrientSolution.Empty;
        foreach(PlantTrait trait in Traits) {
            proceed &= trait.OnTill(this, farmBot, ref altReturn);
        }
        if (!proceed) {
            return altReturn;
        }
        else {
            parentCell.RemovePlant(this);
            PrefabPooler.Instance.ReleasePrefabInstance("plant", gameObject);
            return NutrientLevels;
        }
    }

    public bool OnBotEnter(FarmBot farmBot) {
        bool proceed = true;
        foreach(PlantTrait trait in Traits) {
            proceed &= trait.OnBotEnter(this, farmBot);
        }
        return proceed;
    }

    #endregion

    public void CheckPlantStage() {
        if (plantCurrentStage == _plantMaxStage) return;

        var currentStage = plantCurrentStage;
        while (SurfaceMass >= config.stageTransitionThreshold[plantCurrentStage + 1]) {
            plantCurrentStage++;
            if (plantCurrentStage == _plantMaxStage) break;
        }

        if (currentStage != plantCurrentStage) {
            bool proceed = true;
            foreach(PlantTrait trait in Traits) {
                proceed &= trait.OnStageTransition(this, currentStage, plantCurrentStage);
            }
            if (!proceed) {
                return;
            }
        }

        Debug.Log($"plant display stage {plantCurrentStage}");
        spriteRenderer.sprite = config.plantSprites[plantCurrentStage];
        boxCollider2D.size = spriteRenderer.sprite.bounds.size;
        hasFruit = plantCurrentStage == _plantMaxStage;
    }

    public void ClearFruits() {
        if (hasFruit) {
            hasFruit = false;
            var currentStage = plantCurrentStage;
            plantCurrentStage = Mathf.Min(plantCurrentStage, _plantMaxStage - 1);
            if(currentStage != plantCurrentStage) {
                bool proceed = true;
                foreach(PlantTrait trait in Traits) {
                    proceed &= trait.OnStageTransition(this, currentStage, plantCurrentStage);
                }
                if (!proceed) {
                    return;
                }
            }
            spriteRenderer.sprite = config.plantSprites[plantCurrentStage];
            boxCollider2D.size = spriteRenderer.sprite.bounds.size;
            //TODO - Not sure about this change
            SurfaceMass = config.stageTransitionThreshold[plantCurrentStage];
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

    public void OnInstantiateFromPool() {
        return;
    }

    public void OnReleaseToPool() {
        config = null;
        parentCell = null;
        NutrientLevels = NutrientSolution.Empty;
        RootMass = 0;
        SurfaceMass = 0;
        EnergyLevel = 0;
        Health = 0;
        Age = 0;
        plantCurrentStage = 0;
        healthHistory = new float[0];
        currentHealthIndex = 0;
        hasFruit = false;
        _inInventory = false;
    }
}