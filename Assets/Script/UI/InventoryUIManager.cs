using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject waterInventory;
    [SerializeField] private GameObject plantInventory;
    
    [Header("Water Inventory")]
    [SerializeField] private Slider waterSlider;
    [SerializeField] private TMP_Text waterTxt;
    [SerializeField] private Slider ASlider;
    [SerializeField] private TMP_Text ATxt;
    [SerializeField] private Slider BSlider;
    [SerializeField] private TMP_Text BTxt;
    [SerializeField] private Slider CSlider;
    [SerializeField] private TMP_Text CTxt;
    [SerializeField] private Slider DSlider;
    [SerializeField] private TMP_Text DTxt;

    public static InventoryUIManager Instance;

    private Animator _animator;

    private void Awake()
    {
        if (Instance) Destroy(this);
        else Instance = this;

        _animator = GetComponent<Animator>();
    }

    public void UpdateWaterInventoryUI(NutrientSolution waterVolume, float capacity)
    {
        if (capacity <= 0.0f)
        {
            waterSlider.value = 0.0f;
            waterTxt.text = "N/A";
        }
        else
        {
            waterSlider.value = waterVolume.water / capacity;
            waterTxt.text = $"{waterVolume.water:F1}/{capacity:F1}";
        }

        bool noWater = waterVolume.water <= 0.0;
        Vector4 nutrients = waterVolume.nutrients;
        float maxNutrientAmount = Mathf.Max(nutrients.z, nutrients.y, nutrients.z, nutrients.w);
        

        if (maxNutrientAmount == 0.0f)
        {
            ASlider.value = 0.0f;
            ATxt.text = "0";
            BSlider.value = 0.0f;
            BTxt.text = "0";
            CSlider.value = 0.0f;
            CTxt.text = "0";
            DSlider.value = 0.0f;
            DTxt.text = "0";
        }
        else
        {
            ASlider.value = noWater ? nutrients.x / maxNutrientAmount : nutrients.x / waterVolume.water;
            ATxt.text = noWater ? $"{nutrients.x:F1}" : $"{(nutrients.x / waterVolume.water):P0}";
            BSlider.value = noWater ? nutrients.y / maxNutrientAmount : nutrients.y / waterVolume.water;
            BTxt.text = noWater ? $"{nutrients.y:F1}" : $"{(nutrients.y / waterVolume.water):P0}";
            CSlider.value = noWater ? nutrients.z / maxNutrientAmount : nutrients.z / waterVolume.water;
            CTxt.text = noWater ? $"{nutrients.z:F1}" : $"{(nutrients.z / waterVolume.water):P0}";
            DSlider.value = noWater ? nutrients.w / maxNutrientAmount : nutrients.w / waterVolume.water;
            DTxt.text = noWater ? $"{nutrients.w:F1}" : $"{(nutrients.w / waterVolume.water):P0}";
        }

        waterInventory.SetActive(true);
        plantInventory.SetActive(false);
    }

    private void SetPlayerInventory()
    {
        PlayerPuppetBot playerBot = GameManager.Instance.player;
        if (playerBot == null)
        {
            UpdateWaterInventoryUI(NutrientSolution.Empty, -1.0f);
        }
        else
        {
            if (playerBot.PlantInventory.Count == 0)
            {
                UpdateWaterInventoryUI(playerBot.WaterInventory, playerBot.SolutionInventoryCapacity);
            }
        }
    }
    
    public void ShowInventory()
    {
        SetPlayerInventory();
        _animator.SetTrigger("show");
    }

    public void HideInventory()
    {
        _animator.SetTrigger("hide");
    }
}
