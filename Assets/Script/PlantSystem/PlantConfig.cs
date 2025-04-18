using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Plant")]
public class PlantConfig : ScriptableObject {

    /// <summary>
    /// The name of the plant species, should be equivalent to the config name in the map generator.
    /// </summary>
    public string speciesName;

    /// <summary>
    /// How much of each compound can the plan store
    /// This should be in the range 0 to positiveInfinity
    /// </summary>
    public float waterCapacity;
    /// <summary>
    /// The rate at which the plant can uptake each compound from the soil
    /// This is a percentage in the range 0 to 1
    /// This represents how efficiently the plant can absorb each compound type
    /// </summary>
    public float uptakeRate;

    /// <summary>
    /// The rate at which the plant draws nutrient solution from it's internal stores for metabolism
    /// </summary>
    public float metabolismRate;

    /// <summary>
    /// The amount each compound contributes to the plant's metabolism
    /// </summary>
   public Vector4 metabolismFactor;

    /// <summary>
    /// The point at which the plan transitions from growing root mass to growing height
    /// Ultimately I'd like to explore this being a more complex function but right now it is a simple linear transition based on age
    /// </summary>
    public float rootHeightTransition;

    /// <summary>
    /// The minimum amount of health necessary in a tick to grow
    /// </summary>
    public float growthToleranceThreshold;

    /// <summary>
    /// The threshold energy upon reaching will leach threshold * leachingFactor nutrients
    /// </summary>
    public float leachingEnergyThreshold;
    public Vector4 leachingFactor;

    /// <summary>
    /// Special plants that do not receive water should set this to true
    /// </summary>
    public bool onWaterCallbackBypass;

    public List<Sprite> plantSprites;

    //TODO: maybe cap at < 100%
    public float PercentToRoots(float age) {
        if (rootHeightTransition < 1) {
            return 1;
        }
        else {
            return 1 - Mathf.Clamp01((float)age / rootHeightTransition);
        }
    }
    
    public override string ToString()
    {
        return $"Capacity: {waterCapacity}\n" +
               $"UptakeRate: {uptakeRate}\n" +
               $"MetabolismFactor: {metabolismFactor}\n";
    }

}
