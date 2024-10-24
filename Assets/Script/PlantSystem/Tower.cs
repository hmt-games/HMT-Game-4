using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Tower:NetworkBehaviour {

    /// <summary>
    /// The floors in the tower. Indicies count up so 0 is the ground floor.
    /// </summary>
    public Floor[] floors;

    /// <summary>
    /// The number of cells in the x direction
    /// </summary>
    [Networked]
    public int width { get; set; }
    /// <summary>
    /// The number of cells in the z direction
    /// </summary>
    [Networked]
    public int depth { get; set; }


    IEnumerator OnWater(float intialVolumePerTile) {
        NutrientSolution[,] volumes = new NutrientSolution[floors[0].Cells.GetLength(0), floors[1].Cells.GetLength(1)];
        for(int x = 0; x < volumes.GetLength(0); x++) {
            for (int y = 0; y < volumes.GetLength(1); y++) {
                volumes[x, y] = new NutrientSolution(intialVolumePerTile);
            }
        }
        for(int f = floors.Length - 1; f >= 0; f--) {
            volumes = floors[f].OnWater(volumes);
            ///TODO in principle this should have some delay between floors
            yield return null;
        }
        yield break;
    }
            
    public void OnTick() {
            for (int f = floors.Length - 1; f >= 0; f--)
            {
                floors[f].OnTick();
            }

    }
    
}
