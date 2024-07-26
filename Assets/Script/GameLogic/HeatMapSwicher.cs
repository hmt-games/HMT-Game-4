using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatMapSwicher : MonoBehaviour
{
    private bool _heatMapOn = false;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!_heatMapOn)
                SwitchOnHeatMap();
            else 
                SwitchOffHeatMap();

        }
    }

    private void SwitchOffHeatMap()
    {
        foreach (Floor floor in GameManager.Instance.parentTower.floors)
        {
            for (int x = 0; x < floor.Cells.GetLength(0); x++)
            {
                for (int y = 0; y < floor.Cells.GetLength(1); y++)
                {
                    GameObject gridObj = floor.Cells[x, y].gameObject;
                    Transform heatMapObj = gridObj.transform.GetChild(0);
                    
                    heatMapObj.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
                    heatMapObj.GetChild(1).GetComponent<SpriteRenderer>().color = Color.green;
                    heatMapObj.GetChild(2).GetComponent<SpriteRenderer>().color = Color.blue;
                    heatMapObj.GetChild(3).GetComponent<SpriteRenderer>().color = Color.magenta;
                    
                    heatMapObj.gameObject.SetActive(false);
                    gridObj.transform.GetChild(1).gameObject.SetActive(true);
                }
            }
        }

        _heatMapOn = false;
    }

    private void SwitchOnHeatMap()
    {
        foreach (Floor floor  in GameManager.Instance.parentTower.floors)
        {
            float maxA = float.MinValue;
            float maxB = float.MinValue;
            float maxC = float.MinValue;
            float maxD = float.MinValue;
            
            for (int x = 0; x < floor.Cells.GetLength(0); x++)
            {
                for (int y = 0; y < floor.Cells.GetLength(1); y++)
                {
                    GridCellBehavior grid = floor.Cells[x, y];
                    Vector4 nutrients = grid.NutrientLevels.nutrients;
                    if (nutrients.x > maxA) maxA = nutrients.x;
                    if (nutrients.y > maxB) maxB = nutrients.y;
                    if (nutrients.z > maxC) maxC = nutrients.z;
                    if (nutrients.w > maxD) maxD = nutrients.w;
                }
            }
            
            for (int x = 0; x < floor.Cells.GetLength(0); x++)
            {
                for (int y = 0; y < floor.Cells.GetLength(1); y++)
                {
                    GridCellBehavior grid = floor.Cells[x, y];
                    Vector4 nutrients = grid.NutrientLevels.nutrients;
                    GameObject gridObj = grid.gameObject;
                    Transform heatMapObj = gridObj.transform.GetChild(0);

                    float h, s, v;
                    Color AColor = heatMapObj.GetChild(0).GetComponent<SpriteRenderer>().color;
                    Color.RGBToHSV(AColor, out h, out s, out v);
                    s = (nutrients.x / (maxA)) * s;
                    heatMapObj.GetChild(0).GetComponent<SpriteRenderer>().color = Color.HSVToRGB(h, s, v);
                    
                    Color BColor = heatMapObj.GetChild(1).GetComponent<SpriteRenderer>().color;
                    Color.RGBToHSV(BColor, out h, out s, out v);
                    s = (nutrients.y / (maxB)) * s;
                    heatMapObj.GetChild(1).GetComponent<SpriteRenderer>().color = Color.HSVToRGB(h, s, v);
                    
                    Color CColor = heatMapObj.GetChild(2).GetComponent<SpriteRenderer>().color;
                    Color.RGBToHSV(CColor, out h, out s, out v);
                    s = (nutrients.z / (maxC)) * s;
                    heatMapObj.GetChild(2).GetComponent<SpriteRenderer>().color = Color.HSVToRGB(h, s, v);
                    
                    Color DColor = heatMapObj.GetChild(3).GetComponent<SpriteRenderer>().color;
                    Color.RGBToHSV(DColor, out h, out s, out v);
                    s = (nutrients.w / (maxD)) * s;
                    heatMapObj.GetChild(3).GetComponent<SpriteRenderer>().color = Color.HSVToRGB(h, s, v);

                    heatMapObj.gameObject.SetActive(true);
                    gridObj.transform.GetChild(1).gameObject.SetActive(false);
                }
            }
        }

        _heatMapOn = true;
    }
}
