using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace ErikDraft {
    public class Floor : MonoBehaviour {

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
        public int floorNumber;

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

    public class Tower:MonoBehaviour {

        /// <summary>
        /// The floors in the tower. Indicies count up so 0 is the ground floor.
        /// </summary>
        public Floor[] floors;

        /// <summary>
        /// The number of cells in the x direction
        /// </summary>
        public int width;
        /// <summary>
        /// The number of cells in the z direction
        /// </summary>
        public int depth;


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
            for(int f = floors.Length-1; f >= 0; f--) {
                floors[f].OnTick();
            }
        }
    
    }

}