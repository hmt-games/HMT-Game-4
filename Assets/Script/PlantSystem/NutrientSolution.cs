using System;
using UnityEngine;
using GameConstant;
using Fusion;
using Newtonsoft.Json.Linq;

[Serializable]
public struct NutrientSolution : INetworkStruct {

    /// <summary>
    /// The actual volume of water
    /// </summary>
    public float water;
    /// <summary>
    /// The amount of each nutrient type in the water
    /// </summary>
    public Vector4 nutrients;

    public static NutrientSolution Empty => new(0);

    public NutrientSolution(float water) {
        this.water = water;
        nutrients = Vector4.zero;
    }

    public NutrientSolution(float water, Vector4 nutrients) {
        this.water = water;
        this.nutrients = nutrients;
    }

    public NutrientSolution(NutrientSolution waterVolume) {
        water = waterVolume.water;
        nutrients = waterVolume.nutrients;
    }

    public float GetNutrient(NutrientType type) {
        return nutrients[(int)type];
    }

    public float GetNutrientConcentration(NutrientType type) {
        return nutrients[(int)type] / water;
    }

    public static NutrientSolution operator +(NutrientSolution a, NutrientSolution b) {
        NutrientSolution result = new NutrientSolution();
        result.water = a.water + b.water;
        result.nutrients = a.nutrients + b.nutrients;
        return result;
    }

    /// <summary>
    /// Adds an amount of pure water to the volume. This effectively dillutes the nutrients in the water.
    /// </summary>
    /// <param name="a">A base water volume</param>
    /// <param name="b">A new amount of pure water to add.</param>
    /// <returns></returns>
    public static NutrientSolution operator +(NutrientSolution a, float b) {
        NutrientSolution result = new NutrientSolution();
        result.water = a.water + b;
        return result;
    }

    /// <summary>
    /// Subtracts an amount of water from the volume. This will preserve the relative concentration of nutrients in the water.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static NutrientSolution operator -(NutrientSolution a, float b) {
        NutrientSolution result = new NutrientSolution();
        var prev = a.water;
        result.water = a.water - b;
        result.nutrients = a.nutrients * (result.water / prev);
        return result;
    }

    public static NutrientSolution operator -(NutrientSolution a, NutrientSolution b) {
        NutrientSolution result = new NutrientSolution();
        result.water = a.water - b.water;
        result.nutrients = a.nutrients - b.nutrients;
        return result;
    }

    public static bool operator ==(NutrientSolution a, NutrientSolution b)
    {
        if (a.water != b.water) return false;
        if (a.nutrients != b.nutrients) return false;
        return true;
    }
    
    public static bool operator !=(NutrientSolution a, NutrientSolution b)
    {
        return !(a == b);
    }

    public static NutrientSolution operator *(NutrientSolution a, float b) {
        NutrientSolution result = new NutrientSolution();
        result.water = a.water * b;
        result.nutrients = a.nutrients * b;
        return result;
    }

    public static bool operator >(NutrientSolution a, NutrientSolution b) {
        return a.water > b.water;
    }

    public static bool operator <(NutrientSolution a, NutrientSolution b) {
        return a.water < b.water;
    }

    public static bool operator >(float a, NutrientSolution b) {
        return a > b.water;
    }

    public static bool operator <(float a, NutrientSolution b) {
        return a < b.water;
    }

    public static NutrientSolution Clamp0(NutrientSolution a) {
        NutrientSolution result = new NutrientSolution();
        result.water = Mathf.Max(a.water, 0);
        result.nutrients = Vector4.Max(a.nutrients, Vector4.zero);
        return result;
    }

    public static NutrientSolution NutrientDiff(NutrientSolution a, NutrientSolution b) {
        NutrientSolution result = new NutrientSolution();
        result.water = 0;
        result.nutrients = a.nutrients - b.nutrients;
        return result;
    }

    /// <summary>
    /// Splits the water volume into two parts, one of the specified size and the other the remainder.
    /// 
    /// The nutrients in the water are split proportionally to the water volume to maintain their "concentration"
    /// This returns A tuple of the requested portion of water and any remainder.
    /// 
    /// TODO - maybe this should be a static method and use out parameters instead to avoid new object allocations
    /// </summary>
    /// <param name="portionSize"></param>
    /// <returns></returns>
    public (NutrientSolution portion, NutrientSolution remainder) Split(float portionSize) {
        if (portionSize >= water) {
            return (new NutrientSolution(this), new NutrientSolution());
        }
        else if (portionSize <= 0) {
            return (new NutrientSolution(), new NutrientSolution(this));
        }
        NutrientSolution portion = new NutrientSolution();
        NutrientSolution remainder = new NutrientSolution();
        portion.water = Mathf.Min(water, portionSize);
        remainder.water = water - portion.water;
        float ratio = portion.water / water;
        foreach (NutrientType type in System.Enum.GetValues(typeof(NutrientType))) {
            portion.nutrients[(int)type] = nutrients[(int)type] * ratio;
            remainder.nutrients[(int)type] = nutrients[(int)type] * (1 - ratio);
        }
        return (portion, remainder);
    }

    public NutrientSolution DrawOff(float portionSize) {
        if (portionSize <= 0.0f || water <= 0) return Empty;
        NutrientSolution result = new NutrientSolution();
        result.water = Mathf.Min(water, portionSize);
        result.nutrients = nutrients * (result.water / water);
        water -= result.water;
        nutrients -= result.nutrients;
        return result;
    }

    public NutrientSolution DrawOffPercentage(float percent) {
        if (percent < 0.0 || percent > 1.0) throw new ArgumentException("Invalid percentage");
        NutrientSolution result = this * percent;
        water -= result.water;
        nutrients -= result.nutrients;
        return result;
    }

    public JObject ToFlatJSON() {
        return new JObject
        {
            {"w", water },
            {"a", nutrients.x },
            {"b", nutrients.y },
            {"c", nutrients.z },
            {"d", nutrients.w }
        };
    }

    public override string ToString() {
        return $"{water}, {nutrients.x}, {nutrients.y}, {nutrients.z}, {nutrients.w}";
    }
}