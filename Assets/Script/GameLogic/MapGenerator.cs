using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private GameObject tileObj;
    [SerializeField] private GridTheme grid2DTheme;
    
    private Dictionary<string, PlantConfig> _text2Plant;

    private void Start()
    {
        CreateTower();
    }

    private void CreateTower()
    {
        string[] tower = towerConfig.text.Split(",,");
        string towerHeight = tower[0].Split(",")[1];
        
        Debug.Log($"Creating a tower with height {towerHeight}");
        for (int i = 1; i < tower.Length; i++)
        {
            CreateFloor(tower[i], i);
        }
    }

    private void CreateFloor(string floorInfo, int floorIdx)
    {
        string[] floorData = floorInfo.Split("\n");
        string[] floorSize = floorData[1].Split(",")[1].Split("x");
        int floorSizeX = Int32.Parse(floorSize[0]);
        int floorSizeY = Int32.Parse(floorSize[1]);
        
        Debug.Log($"Creating floor {floorIdx} with size {floorSizeX}x{floorSizeY}");
        for (int y = 0; y < floorSizeY; y++)
        {
            string[] plantRowInfo = floorData[2 + y].Split(",");
            string[] waterRowInfo = floorData[2 + y + floorSizeY].Split(",");
            string[] nutritionRowInfo = floorData[2 + y + floorSizeY * 2].Split(",");
            for (int x = 0; x < floorSizeX; x++)
            {
                int xIdx = x + 1;
                string plant = plantRowInfo[xIdx];
                string water = waterRowInfo[xIdx];
                string nutrition = nutritionRowInfo[xIdx];
                CreateTile(x, y, plant, water, nutrition);
            }
        }
    }

    private void CreateTile(int x, int y, string plant, string water, string nutrition)
    {
        Debug.Log($"Creating tile {x}, {y} with plant: {plant}, water: {water}, and nutrition: {nutrition}");
    }
}
