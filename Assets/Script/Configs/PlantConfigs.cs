using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using util.GameRepresentation;

/* <summery>
 * SO for configuring plant properties in editor.
 * Details of plant specs refers to design doc.
 * <contact> ziyul@andrew.cmu.edu
 */
[CreateAssetMenu(menuName = "Config/Plant")]
public class PlantConfigs : ScriptableObject
{
    [Serializable]
    public class Plant
    {
        public PlantType plantType;
        public float dormantMinTime;   // turns as seed
        public float dormantMaxTime;
        public float minWaterLevel;
        public float maxWaterLevel;
        public int endurance;          // turns before die if condition not met
        public List<NutritionType> nutritionNeeded;
        public PropagateType propagateType;
        public float propagateEnergyThreshold;
    }

    public List<Plant> plants;

    public bool GetPlantInfo(PlantType plantType, out Plant plant)
    {
        foreach (Plant _plant in plants)
        {
            if (_plant.plantType == plantType)
            {
                plant = _plant;
                return true;
            }
        }

        plant = null;
        return false;
    }
}
