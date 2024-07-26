using UnityEngine;

[CreateAssetMenu(menuName = "Config/Soil")]
public class SoilConfig: ScriptableObject {

    /// <summary>
    /// The maximum amount of water and each compound the soil can hold
    /// 
    /// TODO we need to make a custom property drawer for NutrientSolutions
    /// </summary>
    public NutrientSolution capacities;

    public float drainTime;
}