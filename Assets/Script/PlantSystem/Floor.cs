using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Floor : NetworkBehaviour {

    /// <summary>
    /// The Cells on the floor
    /// </summary>
    public GridCellBehavior[,] Cells;
    /// <summary>
    /// The tower this floor is in
    /// </summary>
    public Tower parentTower;
    /// <summary>
    /// What floor in the tower this is (0 is the ground)
    /// </summary>
    [Networked]
    public int floorNumber { get; set; }
    [Networked]
    public int SizeX { get; set; }
    [Networked]
    public int SizeY { get; set; }


    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public NutrientSolution[,] OnWater(NutrientSolution[,] volumes) {
        for (int x = 0; x < Cells.GetLength(0); x++) {
            for (int y = 0; y < Cells.GetLength(1); y++) {
                volumes[x,y] = Cells[x, y].OnWater(volumes[x, y]);
            }
        }
        return volumes;
    }

    public void OnTick() {
        for (int x = 0; x < Cells.GetLength(0); x++) {
            for (int y = 0; y < Cells.GetLength(1); y++) {
                Cells[x, y].OnTick();
            }
        }
    }

}
