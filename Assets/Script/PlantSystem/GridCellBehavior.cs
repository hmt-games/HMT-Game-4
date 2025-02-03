using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Fusion;

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


    /// <summary>
    /// The plants that have roots in the tile's soil. Realistically they probably won't be fully public
    /// </summary>
    public List<PlantBehavior> rootedPlants;
    
    /// <summary>
    /// Plants that are on the tile above the surface.
    /// The basic distinction is that surface plants don't get the water events.
    /// </summary>
    public List<PlantBehavior> surfacePlants;

    /// <summary>
    /// Soil configurations will contain stull like water capacity and maybe implications for nutrient levels.
    /// 
    /// This will also probalby play in to renderer information.
    /// Could also impact bot mobility on the tile
    /// </summary>
    public SoilConfig soilConfig;

    /// <summary>
    /// How much water is in the cell's soil.
    /// </summary>
    //public float waterLevel;

    //public float[] compoundLevels;

    //public NutrientSolution NutrientLevels;

    [Networked]
    public ref NutrientSolution NutrientLevels => ref MakeRef<NutrientSolution>(new NutrientSolution(0));

    private void Awake() {
        rootedPlants = new List<PlantBehavior>();
        //NutrientLevels = NutrientSolution.Empty;
    }

    public float RemainingWaterCapacity {
        get {
            return soilConfig.capacities.water - NutrientLevels.water;
        }
    }

    /// <summary>
    /// This is called as a regular heartbeat of the cellular automata game.
    /// I've contemplated whether this should get a paramter of the local light level/color. I would probalby make it a simplified version of the actual Unity lighting system to reduce the complexity space.
    /// </summary>
    public void OnTickk() {
        ///Reconile Excess Water
    
        float rootTotal = 0;
        foreach(PlantBehavior plant in rootedPlants) {
            rootTotal += plant.RootMass;
        }
        ///Doll out nutrients / water volunme based on the relative root mass of each plant.
        NutrientSolution aggregate = NutrientSolution.Empty;
        foreach(PlantBehavior plant in rootedPlants) {
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

    public void OnTick()
    {
        
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
        foreach(PlantBehavior plant in surfacePlants.OrderByDescending(p => p.Height)) {
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
        else {
            return NutrientSolution.Empty;
        }

            
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}