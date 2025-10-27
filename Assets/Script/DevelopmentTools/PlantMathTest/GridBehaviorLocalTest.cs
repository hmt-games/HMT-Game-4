using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameConstant;

/// <summary>
/// The only difference between this and GridCellBehavior we use in game is to
/// remove the [Networked] tag from properties. As without with we must network
/// spawn this object before we use it in local test.
/// Thus, the idea is after we modify the desired functions (mostly just the OnTick)
/// we just copy and paste it back to GridCellBehavior.
/// </summary>
public class GridBehaviorLocalTest : MonoBehaviour
{
    /// <summary>
    /// The floor of a tower that this cell is on. 
    /// </summary>
    public Floor parentFloor;

    /// <summary>
    /// The x and y coordinates of the cell in the grid.
    /// Keep it as x,y instead of row, column because that will make it eaiser to think ahout for the agent interface.
    /// </summary>
    public int gridX { get; set; }
    //[FormerlySerializedAs("gridY")]
    public int gridZ { get; set; }

    
    public List<PlantBehaviorLocalTest> plants;

    /// <summary>
    /// Soil configurations will contain stull like water capacity and maybe implications for nutrient levels.
    /// This will also probalby play in to renderer information.
    /// Could also impact bot mobility on the tile
    /// </summary>
    public SoilConfigSO soilConfig;

    //TODO: use this
    public bool[] plantSlotOccupation = new bool[GLOBAL_CONSTANTS.MAX_PLANT_COUNT_PER_TILE];

    public NutrientSolution NutrientLevels;

    
    public float RemainingWaterCapacity {
        get {
            return soilConfig.waterCapacity - NutrientLevels.water;
        }
    }

    /// <summary>
    /// This is called as a regular heartbeat of the cellular automata game.
    /// I've contemplated whether this should get a paramter of the local light level/color. I would probalby make it a simplified version of the actual Unity lighting system to reduce the complexity space.
    /// </summary>
    public void OnTick() {
        ///Reconile Excess Water
    
        float rootTotal = 0;
        foreach(PlantBehaviorLocalTest plant in plants) {
            rootTotal += plant.RootMass;
        }
        ///Doll out nutrients / water volunme based on the relative root mass of each plant.
        NutrientSolution aggregate = 
            NutrientLevels.water > soilConfig.waterCapacity 
                ? NutrientLevels.DrawOff(NutrientLevels.water - soilConfig.waterCapacity) 
                : NutrientSolution.Empty;
        foreach(PlantBehaviorLocalTest plant in plants) {
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


    /// <summary>
    /// Removes the Excess water / nutrients.
    /// Not sure when this should happen, currently it's an arbitrary delay at the end of tick but it should probably be better
    /// synchronoized and that.
    /// </summary>
    /// <param name="excess"></param>
    /// <returns></returns>
    private IEnumerator Drain(NutrientSolution excess) {
        yield return new WaitForSeconds(soilConfig.drainRate);
        
    }

    /// <summary>
    /// This is called when water hits the cell.
    /// It should return a volume of water that was not absorbed by the cell or of it's plants to pass down farther.
    /// </summary>
    /// <param name="waterVolume"></param>
    public NutrientSolution OnWater(NutrientSolution waterVolume) {
        foreach(PlantBehaviorLocalTest plant in plants.OrderByDescending(p => p.Height)) {
            waterVolume = plant.OnWater(waterVolume);
        }
        if (waterVolume.water > 0) {
            //If there is room in the soil for the water, add it all but you need to check that there is capacity for the nutrients.
            if (soilConfig.waterCapacity > waterVolume + NutrientLevels) {
                NutrientLevels += waterVolume;
            }
            //If there is not room in the soil for the water, split the water and add the portion that fits.
            else {
                NutrientLevels += (waterVolume - RemainingWaterCapacity);
            }

            NutrientSolution excess = NutrientSolution.Clamp0(NutrientLevels - soilConfig.waterCapacity);
            NutrientLevels -= excess;
            return excess;
        }

        return NutrientSolution.Empty;
    }
}