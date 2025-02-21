using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Fusion;
using GameConstant;

public class GridCellBehavior : NetworkBehaviour
{


    /// <summary>
    /// The floor of a tower that this cell is on. 
    /// </summary>
    public Floor parentFloor;

    /// <summary>
    /// The x and y coordinates of the cell in the grid.
    /// Keep it as x,y instead of row, column because that will make it eaiser to think ahout for the agent interface.
    /// </summary>
    [Networked]
    public int gridX { get; set; }
    //[FormerlySerializedAs("gridY")]
    [Networked]
    public int gridZ { get; set; }

    
    public List<PlantBehavior> plants;

    /// <summary>
    /// Soil configurations will contain stull like water capacity and maybe implications for nutrient levels.
    /// 
    /// This will also probalby play in to renderer information.
    /// Could also impact bot mobility on the tile
    /// </summary>
    public SoilConfig soilConfig;

    public bool[] plantSlotOccupation = new bool[GLOBAL_CONSTANTS.MAX_PLANT_COUNT_PER_TILE];

    [Networked]
    public ref NutrientSolution NutrientLevels => ref MakeRef<NutrientSolution>(new NutrientSolution(0));

    private void Awake() {
        //NutrientLevels = NutrientSolution.Empty;
    }

    public float RemainingWaterCapacity {
        get {
            return soilConfig.capacities - NutrientLevels.water;
        }
    }

    /// <summary>
    /// This is called as a regular heartbeat of the cellular automata game.
    /// I've contemplated whether this should get a paramter of the local light level/color. I would probalby make it a simplified version of the actual Unity lighting system to reduce the complexity space.
    /// </summary>
    public void OnTick() {
        ///Reconile Excess Water
    
        float rootTotal = 0;
        foreach(PlantBehavior plant in plants) {
            rootTotal += plant.RootMass;
        }
        ///Doll out nutrients / water volunme based on the relative root mass of each plant.
        NutrientSolution aggregate = NutrientSolution.Empty;
        foreach(PlantBehavior plant in plants) {
           aggregate += plant.OnTick(NutrientLevels * (plant.RootMass / rootTotal));
        }

        if (aggregate.water > 0) {
            //If there is room in the soil for the water, add it all but you need to check that there is capacity for the nutrients.
            if (soilConfig.capacities > aggregate + NutrientLevels) {
                NutrientLevels += aggregate;
            }
            //If there is not room in the soil for the water, split the water and add the portion that fits.
            else {
                NutrientLevels += (aggregate - RemainingWaterCapacity);
            }
    
            NutrientSolution excess = NutrientSolution.Clamp0(NutrientLevels - soilConfig.capacities);
            NutrientLevels -= excess;
            StartCoroutine(Drain(excess));
        }
    
        ///Reconcile the aggregate with the capacities.
        ///If there is excess, pass it down the farm (or do you pass it out?)
    }


    /// <summary>
    /// Removes the Excess water / nutrients.
    /// Not sure when this should happen, currently it's an arbitrary delay at the end of tick but it should probably be better
    /// synchronoized and that.
    /// </summary>
    /// <param name="excess"></param>
    /// <returns></returns>
    private IEnumerator Drain(NutrientSolution excess) {
        yield return new WaitForSeconds(soilConfig.drainTime);
        
    }

    /// <summary>
    /// This is called when water hits the cell.
    /// It should return a volume of water that was not absorbed by the cell or of it's plants to pass down farther.
    /// </summary>
    /// <param name="waterVolume"></param>
    public NutrientSolution OnWater(NutrientSolution waterVolume) {
        foreach(PlantBehavior plant in plants.OrderByDescending(p => p.Height)) {
            waterVolume = plant.OnWater(waterVolume);
        }
        if (waterVolume.water > 0) {
            //If there is room in the soil for the water, add it all but you need to check that there is capacity for the nutrients.
            if (soilConfig.capacities > waterVolume + NutrientLevels) {
                NutrientLevels += waterVolume;
            }
            //If there is not room in the soil for the water, split the water and add the portion that fits.
            else {
                NutrientLevels += (waterVolume - RemainingWaterCapacity);
            }

            NutrientSolution excess = NutrientSolution.Clamp0(NutrientLevels - soilConfig.capacities);
            NutrientLevels -= excess;
            return excess;
        }

        return NutrientSolution.Empty;
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}