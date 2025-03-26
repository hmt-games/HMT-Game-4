using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlantMathUIManager : MonoBehaviour
{
    [SerializeField] private int plotterIdx = 0;
    //[SerializeField] private GameObject plotterSelectUI;

    [SerializeField] private TMP_Dropdown gridIdxSelect;
    [SerializeField] private TMP_Dropdown typeSelect;
    [SerializeField] private TMP_Dropdown soilDataSelect;
    [SerializeField] private GameObject plantSelect;
    [SerializeField] private TMP_Dropdown plantIdxSelect;
    [SerializeField] private TMP_Dropdown plantDataSelect;

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        
    }
}
