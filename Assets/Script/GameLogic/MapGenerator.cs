using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    /*
     * For the tower config, the goal is to design a bi-directional mapping
     * between game states and text representations, so that any state can be loaded
     * from or saved to a text config. Defining such a mapping would also help with
     * procedural generation. As we want the text representation to be as human-readable
     * as possible, csv format is chosen currently, for its ease of editing and viewing
     * in google sheet above all other reasons.
     */
    //TODO: Instead of assuming the tower config is in the right format, we should implement sanity checks
    [SerializeField] private TextAsset towerConfig;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GridTheme grid2DTheme;

    [SerializeField] private List<GameObject> plantsPrefab;
    private Dictionary<Char, GameObject> _text2Plant;

    private void Awake()
    {
        if (plantsPrefab.Count != 4) Debug.LogError("test mode, please give 4 plant prefabs");
        _text2Plant = new Dictionary<Char, GameObject>
        {
            {'A', plantsPrefab[0]},
            {'B', plantsPrefab[1]},
            {'C', plantsPrefab[2]},
            {'D', plantsPrefab[3]},
        };
    }

    private void Start()
    {
        CreateTower();
    }

    private void CreateTower()
    {
        string separator = towerConfig.text.Split("\n")[1];
        string[] towerInfo = towerConfig.text.Split(separator + "\nFloor");
        string towerHeight = towerInfo[0].Split(",")[1];
        
        Debug.Log($"Creating a tower with height {towerHeight}");
        GameObject towerObj = new GameObject();
        towerObj.name = "Tower";
        Tower nTower = towerObj.AddComponent<Tower>();
        nTower.floors = new Floor[Int32.Parse(towerHeight)];
        for (int i = 1; i < towerInfo.Length; i++)
        {
            nTower.floors[i-1] = CreateFloor(towerInfo[i], i, nTower);
        }

        GameManager.Instance.parentTower = nTower;
    }

    private Floor CreateFloor(string floorInfo, int floorIdx, Tower parentTower)
    {
        Debug.Log(floorInfo);
        string[] floorData = floorInfo.Split("\n");
        string[] floorSize = floorData[0].Split(",")[1].Split("x");
        int floorSizeX = Int32.Parse(floorSize[0]);
        int floorSizeY = Int32.Parse(floorSize[1]);
        
        Debug.Log($"Creating floor {floorIdx} with size {floorSizeX}x{floorSizeY}");
        GameObject floorObj = new GameObject();
        floorObj.name = $"Floor{floorIdx}";
        Floor nFloor = floorObj.AddComponent<Floor>();
        floorObj.transform.parent = parentTower.gameObject.transform;
        nFloor.parentTower = parentTower;
        nFloor.floorNumber = floorIdx;
        nFloor.Cells = new GridCellBehavior[floorSizeX,floorSizeY];
        for (int y = 0; y < floorSizeY; y++)
        {
            string[] plantRowInfo = floorData[1 + y].Split(",");
            string[] waterRowInfo = floorData[1 + y + floorSizeY].Split(",");
            string[] nutritionRowInfo = floorData[1 + y + floorSizeY * 2].Split(",");
            for (int x = 0; x < floorSizeX; x++)
            {
                int xIdx = x + 1;
                string plant = plantRowInfo[xIdx];
                string water = waterRowInfo[xIdx];
                string nutrition = nutritionRowInfo[xIdx];
                nFloor.Cells[x,y] = CreateGrid(x, y, plant, water, nutrition, nFloor, floorSizeY + 1);
            }
        }

        return nFloor;
    }

    private GridCellBehavior CreateGrid(int x, int y, string plants, string water, string nutrition, Floor parentFloor, int floorOffsetY)
    {
        Debug.Log($"Creating grid {x}, {y} with plant: {plants}, water: {water}, and nutrition: {nutrition}");
        GameObject gridObj = Instantiate(tilePrefab, new Vector3(x, y + floorOffsetY * parentFloor.floorNumber, 0.0f), Quaternion.identity);
        gridObj.name = $"Grid {x},{y}";
        gridObj.GetComponent<SpriteRenderer>().color = (x + y + parentFloor.floorNumber) % 2 == 0 ? grid2DTheme.lightColor : grid2DTheme.darkColor;
        gridObj.transform.parent = parentFloor.gameObject.transform;
        
        GridCellBehavior nGrid = gridObj.AddComponent<GridCellBehavior>();
        nGrid.parentFloor = parentFloor;
        nGrid.gridX = x;
        nGrid.gridY = y;
        
        //TODO: maybe we want our infrastructure to support arbitrary plant in the same cell
        if (plants.Length > 4)
        {
            Debug.LogError("Current only support up to 4 plant in one cell");
            return nGrid;
        }
        Transform plantsSlot = gridObj.transform.GetChild(1);
        for (int i = 0; i < plants.Length; i++)
        {
            if (!Char.IsLetter(plants[i])) continue;
            Vector3 pos = plantsSlot.GetChild(i).position;
            Instantiate(_text2Plant[plants[i]], pos, Quaternion.identity, plantsSlot.GetChild(i));
        }

        nGrid.NutrientLevels = new NutrientSolution(float.Parse(water));
        string[] nutrients = nutrition.Split("|");
        nGrid.NutrientLevels.nutrients = new Vector4(
            float.Parse(nutrients[0]),
            float.Parse(nutrients[1]),
            float.Parse(nutrients[2]),
            float.Parse(nutrients[3]));
        
        return nGrid;
    }
}
