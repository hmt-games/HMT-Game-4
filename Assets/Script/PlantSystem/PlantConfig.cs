using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Plant")]
public class PlantConfig : ScriptableObject {

    /// <summary>
    /// How much of each compound can the plan store
    /// This should be in the range 0 to positiveInfinity
    /// </summary>
    public Vector4 capacities;
    /// <summary>
    /// The rate at which the plant can uptake each compound from the soil
    /// This is a percentage in the range 0 to 1
    /// This represents how efficiently the plant can absorb each compound type
    /// </summary>
    public Vector4 uptakeRate;
    /// <summary>
    ///// The rate at which the plant can leech each compound back into the soil
    ///// This is a percentage in the range 0 to 1
    ///// </summary>
    //public float[] leechRate = new float[System.Enum.GetValues(typeof(NutrientType)).Length];
    
    /// <summary>
    /// The amount of each compound per tick needed to maintain the plant's health
    /// </summary>
    public Vector4 metabolismNeeds;


    /// <summary>
    /// The amount each compound contributes to the plant's metabolism
    /// </summary>
   public Vector4 metabolismFactor;

    /// <summary>
    /// The amount pulled from the plant's reserves to grow
    /// This is a real value used in a clamp function
    public Vector4 growthConsumptionRateLimit;
    
    /// <summary>
    /// The coefficients for the effect of each compound on the plant's growth if they are negtaive they inhibit growth
    /// This is a real value
    /// </summary>
    public Vector4 growthFactor;

    /// <summary>
    /// The point at which the plan transitions from growing root mass to growing height
    /// Ultimately I'd like to explore this being a more complex function but right now it is a simple linear transition based on age
    /// </summary>
    public int rootHeightTransition;

    /// <summary>
    /// The minimum amount of health necessary in a tick to grow
    /// </summary>
    public float growthToleranceThreshold;

    public float PercentToRoots(float age) {
        if (rootHeightTransition < 1) {
            return 1;
        }
        else {
            return Mathf.Clamp01((float)age / rootHeightTransition);
        }
    }

}
