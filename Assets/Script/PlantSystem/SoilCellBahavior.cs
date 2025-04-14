using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Fusion;
using GameConstant;

public class SoilCellBehavior : GridCellBehavior
{
    public List<PlantBehavior> plants;

    /// <summary>
    /// Soil configurations will contain stull like water capacity and maybe implications for nutrient levels.
    /// 
    /// This will also probalby play in to renderer information.
    /// Could also impact bot mobility on the tile
    /// </summary>
    public SoilConfig soilConfig;

    public int plantCount = 0;
    //TODO: use this
    public bool[] plantSlotOccupation = new bool[GLOBAL_CONSTANTS.MAX_PLANT_COUNT_PER_TILE];

    [Networked]
    public ref NutrientSolution NutrientLevels => ref MakeRef<NutrientSolution>(new NutrientSolution(0));

    private void Awake() {
        //NutrientLevels = NutrientSolution.Empty;
    }

    public float RemainingWaterCapacity {
        get {
            return soilConfig.waterCapacity - NutrientLevels.water;
        }
    }

    /// <summary>
    /// This is called as a regular heartbeat of the cellular automata game.
    /// I've contemplated whether this should get a paramter of the local light level/color. I would probalby make it a simplified version of the actual Unity lighting system to reduce the complexity space.
    /// </summary>
    public override void OnTick() {
        ///Reconile Excess Water
    
        float rootTotal = 0;
        foreach(PlantBehavior plant in plants) {
            rootTotal += plant.RootMass;
        }
        ///Doll out nutrients / water volunme based on the relative root mass of each plant.
        NutrientSolution aggregate = 
            NutrientLevels.water > soilConfig.waterCapacity 
                ? NutrientLevels.DrawOff(NutrientLevels.water - soilConfig.waterCapacity) 
                : NutrientSolution.Empty;
        foreach(PlantBehavior plant in plants) {
            aggregate += plant.OnTick(NutrientLevels * (plant.RootMass / rootTotal));
        }

        NutrientLevels = aggregate;

        if (NutrientLevels.water > soilConfig.waterCapacity)
        {
            NutrientSolution excess =
                NutrientLevels.DrawOff(Mathf.Min(soilConfig.drainRate, NutrientLevels.water - soilConfig.waterCapacity));
            //TODO
        }

        ///Reconcile the aggregate with the capacities.
        ///If there is excess, pass it down the farm (or do you pass it out?)
    }

    public override NutrientSolution OnWater(NutrientSolution volumes)
    {
        return volumes;
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}