using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddToSoil : MonoBehaviour
{
    [SerializeField] private TMP_InputField waterInput;
    [SerializeField] private TMP_InputField AInput;
    [SerializeField] private TMP_InputField BInput;
    [SerializeField] private TMP_InputField CInput;
    [SerializeField] private TMP_InputField DInput;
    [SerializeField] private TMP_Dropdown soilDropdown;
    [SerializeField] private Button addBtn;

    private GridBehaviorLocalTest _soilSelected;

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        
        List<string> soilOptions = new List<string>();
        for (int i = 0; i < PlantMathTestManager.Instance.simulatedGrid.Count; i++)
        {
            soilOptions.Add($"Soil{i}");
        }
        soilDropdown.AddOptions(soilOptions);
        soilDropdown.onValueChanged.AddListener(delegate { OnSoilDropdownChanged(soilDropdown); });
        OnSoilDropdownChanged(soilDropdown);
    }

    public void OnSoilDropdownChanged(TMP_Dropdown change)
    {
        _soilSelected = PlantMathTestManager.Instance.simulatedGrid[change.value];
    }

    public void Add()
    {
        float water = float.Parse(waterInput.text);
        float A = float.Parse(AInput.text);
        float B = float.Parse(BInput.text);
        float C = float.Parse(CInput.text);
        float D = float.Parse(DInput.text);

        _soilSelected.NutrientLevels += new NutrientSolution(water, new Vector4(A, B, C, D));
    }
}
