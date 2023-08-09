using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Weather")]

public class Weather : ScriptableObject
{
    public List<float> start_Probabilities;

    public List<float> sunny_TransitMatrix;

    public List<float> cloudy_TransitMatrix;

    public List<float> rainy_TransitMatrix;
}
