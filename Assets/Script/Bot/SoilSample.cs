using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SoilSample {
    
    public readonly NutrientSolution nutrientSolution;
    public readonly ulong gameTick;
    public readonly Vector3Int tileAddress;
    public readonly string samplerName;

    // We need the HMT Stated Visible data for each plant
    // need an internal version of PlantStateData that doesn't ahve the full health history, and reduced config to an index
    public readonly List<VisiablePlantStateSnapshot> plantData;




    public SoilSample(NutrientSolution nutrientSolution, List<PlantBehavior> plants, ulong gameTick, int floorIndex, int tileX, int tileY, string samplerName) {
        this.nutrientSolution = nutrientSolution;
        this.gameTick = gameTick;
        this.tileAddress = new Vector3Int(tileX, tileY, floorIndex);
        this.samplerName = samplerName;
        plantData = new List<VisiablePlantStateSnapshot>();
        foreach (var plant in plants) {
            plantData.Add(plant.GetPlantState().ToVisibleSnapshot());
        }
    }


    public SoilSample(NutrientSolution nutrientSolution, List<PlantBehavior> plants, ulong gameTick, int floorIndex, Vector2Int tileAddress, string samplerName) {
        this.nutrientSolution = nutrientSolution;
        this.gameTick = gameTick;
        this.tileAddress = new Vector3Int(tileAddress.x, tileAddress.y, floorIndex);
        this.samplerName = samplerName;
        plantData = new List<VisiablePlantStateSnapshot>();
        foreach (var plant in plants) {
            plantData.Add(plant.GetPlantState().ToVisibleSnapshot());
        }
    }
}
