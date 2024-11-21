using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataVisualization : MonoBehaviour
{
    [SerializeField] private GameObject toggleParent;

    private TMP_Text[,,] _gridTextField = null;
    // private bool _gridTextFieldActive = false;

    public static DataVisualization Instance;

    private void Awake()
    {
        if (Instance) Destroy(this.gameObject);
        else Instance = this;
    }

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

    }


    public void Init()
    {
        Tower tower = GameManager.Instance.parentTower;
        _gridTextField = new TMP_Text[tower.floors.Length, tower.width, tower.depth];
        for (int y = 0; y < _gridTextField.GetLength(0); y++)
        {
            GridCellBehavior[,] cells = tower.floors[y].Cells;
            for (int x = 0; x < _gridTextField.GetLength(1); x++)
            {
                for (int z = 0; z < _gridTextField.GetLength(2); z++)
                {
                    // Debug.LogWarning($"Set for {y}, {x}, {z}");
                    _gridTextField[y, x, z] = cells[x, z].transform.Find("Canvas").Find("DataText").GetComponent<TMP_Text>();
                    _gridTextField[y, x, z].gameObject.SetActive(false);
                }
            }
        }
    }

    public void OnWaterToggle(bool toggleState)
    {
        if (!toggleState)
        {
            SwitchGridTextOff();
            return;
        }
        
        for (int y = 0; y < _gridTextField.GetLength(0); y++)
        {
            for (int x = 0; x < _gridTextField.GetLength(1); x++)
            {
                for (int z = 0; z < _gridTextField.GetLength(2); z++)
                {
                    _gridTextField[y, x, z].gameObject.SetActive(true);
                    NutrientSolution nutrientSolution =
                        GameManager.Instance.parentTower.floors[y].Cells[x, z].NutrientLevels;
                    _gridTextField[y, x, z].text = nutrientSolution.water.ToString("0.0");
                }
            }
        }
    }

    public void OnNutrientToggle(bool toggleState)
    {
        if (!toggleState)
        {
            SwitchGridTextOff();
            return;
        }
        
        for (int y = 0; y < _gridTextField.GetLength(0); y++)
        {
            for (int x = 0; x < _gridTextField.GetLength(1); x++)
            {
                for (int z = 0; z < _gridTextField.GetLength(2); z++)
                {
                    _gridTextField[y, x, z].gameObject.SetActive(true);
                    Vector4 nutrient =
                        GameManager.Instance.parentTower.floors[y].Cells[x, z].NutrientLevels.nutrients;
                    string nutrientText = $"{nutrient.x:0.0}, {nutrient.y:0.0}\n{nutrient.z:0.0}, {nutrient.w:0.0}";
                    _gridTextField[y, x, z].text = nutrientText;
                }
            }
        }
    }

    private void SwitchGridTextOff()
    {
        foreach (var text in _gridTextField)
        {
            text.gameObject.SetActive(false);
        }
    }
}
