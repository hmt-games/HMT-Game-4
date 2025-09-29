using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using HMT.Puppetry;
using Unity.VisualScripting;

[Serializable]
public struct PlantStateData {
    public float rootMass;
    public float surfaceMass;
    public NutrientSolution nutrientLevels;
    public float energyLevel;
    
    public int age;
    public int currentStage;
    public PlantConfigSO config;

    public float[] healthHistory;
    public int currentHealthIndex { get; set; }

    public Sprite CurrentStageSprite {
        get {
            return config.plantSprites[currentStage];
        }
    }

    public static PlantStateData Empty {
        get {
            return new PlantStateData(null);
        }
    }

    public PlantStateData(PlantConfigSO config) {
        this.config = config;
        rootMass = 0f;
        surfaceMass = 0f;
        energyLevel = 0f;
        currentHealthIndex = 0;
        age = 0;
        currentStage = 0;

        if (config != null) {
            nutrientLevels = new NutrientSolution(config.metabolismRate, config.metabolismFactor);

            healthHistory = new float[config.maxHealthHistory];
            for (int i = 0; i < config.maxHealthHistory; i++) {
                healthHistory[i] = 1;
            }
        }
        else {
            healthHistory = new float[0];
            nutrientLevels = NutrientSolution.Empty;
        }
        
    }

    public PlantStateData(PlantConfigSO config, NutrientSolution initalNutrients) {
        this.config = config;
        rootMass = 0f;
        surfaceMass = 0f;
        nutrientLevels = initalNutrients;
        energyLevel = 0f;
        healthHistory = new float[config.maxHealthHistory];
        for (int i = 0; i < config.maxHealthHistory; i++) {
            healthHistory[i] = 1;
        }
        currentHealthIndex = 0;
        age = 0;
        currentStage = 0;
    }

    public PlantStateData(PlantConfigSO config, NutrientSolution initalNutrients, 
                          float initialRootMass, float initialSurfaceMass,
                          float energyLevel, 
                          float[] healthHistory, int currentHealthIndex,
                          int age, int currentStage) {
        this.config = config;
        rootMass = initialRootMass;
        surfaceMass = initialSurfaceMass;
        nutrientLevels = initalNutrients;
        this.energyLevel = energyLevel;
        this.healthHistory = healthHistory;
        this.currentHealthIndex = currentHealthIndex;
        this.age = age;
        this.currentStage = currentStage;
    }
    
    public PlantStateData(PlantConfigSO config, NutrientSolution initalNutrients, 
        float initialRootMass, float initialSurfaceMass,
        float energyLevel,
        int age, int currentStage) {
        this.config = config;
        rootMass = initialRootMass;
        surfaceMass = initialSurfaceMass;
        nutrientLevels = initalNutrients;
        this.energyLevel = energyLevel;
        healthHistory = new float[config.maxHealthHistory];
        for (int i = 0; i < config.maxHealthHistory; i++) {
            healthHistory[i] = 1;
        }
        currentHealthIndex = 0;
        this.age = age;
        this.currentStage = currentStage;
    }

    public PlantStateData(PlantConfigSO config, NutrientSolution initalNutrients,
                      float initialRootMass, float initialSurfaceMass,
                      float energyLevel, int age) {
        this.config = config;
        rootMass = initialRootMass;
        surfaceMass = initialSurfaceMass;
        nutrientLevels = initalNutrients;
        this.energyLevel = energyLevel;
        this.healthHistory = new float[config.maxHealthHistory];
        for (int i = 0; i < config.maxHealthHistory; i++) {
            healthHistory[i] = 1;
        }
        this.currentHealthIndex = 0;
        this.age = age;
        this.currentStage = 0;
    }


    public JObject HMTStateRep(HMTStateLevelOfDetail lod) {
        JObject rep = new JObject();
        rep["rootMass"] = rootMass;
        rep["surfaceMass"] = surfaceMass;
        rep["nutrientLevels"] = nutrientLevels.ToFlatJSON();
        rep["energyLevel"] = energyLevel;
        rep["age"] = age;
        rep["currentStage"] = currentStage;
        rep["config"] = config.name;
        if (lod == HMTStateLevelOfDetail.Full) {
            JArray healthHistoryArray = new JArray();
            foreach (float health in healthHistory) {
                healthHistoryArray.Add(health);
            }
            rep["healthHistory"] = healthHistoryArray;
            rep["currentHealthIndex"] = currentHealthIndex;
        }
        return rep;
    }
}
