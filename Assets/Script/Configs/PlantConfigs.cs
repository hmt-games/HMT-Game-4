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
        public float dormantMinTime;
        public float formantMaxTime;
        public float minWaterLevel;
        public float maxWaterLevel;
        public List<NutritionType> nutritionNeeded;
    }

    public List<Plant> plants;
}
