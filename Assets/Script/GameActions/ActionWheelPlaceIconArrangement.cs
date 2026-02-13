using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionWheelPlaceIconArrangement : MonoBehaviour
{
    [SerializeField] private float childScale = 1.0f;
    [SerializeField] private float radius = 1.0f;

    private void OnValidate()
    {
        int childCount = transform.childCount;
        float radInc = 2 * Mathf.PI / childCount;
        float rad = 0.0f;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.position = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0.0f) * radius;
            child.localScale = new Vector3(childScale, childScale, childScale);
            rad += radInc;
        }
    }
}
