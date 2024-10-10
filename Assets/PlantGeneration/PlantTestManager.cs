using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantTestManager : MonoBehaviour
{
    [SerializeField] private Plants2d plants2d;

    public void IterationPlus()
    {
        plants2d.IncreaseIterations();
    }

    public void IterationMinus()
    {
        plants2d.DecreaseIterations();
    }

    public void Regenerate()
    {
        plants2d.OnButtonPress();
    }
}
