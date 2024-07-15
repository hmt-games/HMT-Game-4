using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

namespace ErikDraft {
    /// <summary>
    /// Represents a single cell in the grid. 
    /// Primarily contains a list of rootedPlants and the different resource levels.
    /// Also the root function that is used to call the different plant functions.
    /// </summary>
    public class GridCellBehavior : MonoBehaviour {


        /// <summary>
        /// The floor of a tower that this cell is on. 
        /// </summary>
        public Floor parentFloor;

        /// <summary>
        /// The x and y coordinates of the cell in the grid.
        /// Keep it as x,y instead of row, column because that will make it eaiser to think ahout for the agent interface.
        /// </summary>
        public int gridX;
        public int gridY;


        /// <summary>
        /// The plants that have roots in the tile's soil. Realistically they probably won't be fully public
        /// </summary>
        public List<PlantBehavior> rootedPlants;
        
        /// <summary>
        /// Plants that are on the tile above the surface.
        /// The basic distinction is that surface plants don't get the water events.
        /// </summary>
        public List<PlantBehavior> surfacePlants;

        /// <summary>
        /// Soil configurations will contain stull like water capacity and maybe implications for nutrient levels.
        /// 
        /// This will also probalby play in to renderer information.
        /// Could also impact bot mobility on the tile
        /// </summary>
        public SoilConfig soilConfig;

        /// <summary>
        /// How much water is in the cell's soil.
        /// </summary>
        //public float waterLevel;

        //public float[] compoundLevels;

        public WaterVolume waterVolume;

        private void Awake() {
            rootedPlants = new List<PlantBehavior>();
            waterVolume = WaterVolume.Empty;
        }

        public float RemainingWaterCapacity {
            get {
                return soilConfig.waterCapacity - waterVolume.water;
            }
        }

        /// <summary>
        /// This is called as a regular heartbeat of the cellular automata game.
        /// I've contemplated whether this should get a paramter of the local light level/color. I would probalby make it a simplified version of the actual Unity lighting system to reduce the complexity space.
        /// </summary>
        public void OnTick() {
            float rootTotal = 0;
            foreach(PlantBehavior plant in rootedPlants) {
                rootTotal += plant.RootMass;
            }
            ///Doll out nutrients / water volunme based on the relative root mass of each plant.
            WaterVolume aggregate = WaterVolume.Empty;
            foreach(PlantBehavior plant in rootedPlants) {
               aggregate += plant.OnTick(waterVolume * (plant.RootMass / rootTotal));
            }
        }

        /// <summary>
        /// This is called when water hits the cell.
        /// It should return a volume of water that was not absorbed by the cell or of it's plants to pass down farther.
        /// </summary>
        /// <param name="waterVolume"></param>
        public WaterVolume OnWater(WaterVolume waterVolume) {
            foreach(PlantBehavior plant in surfacePlants.OrderByDescending(p => p.Height)) {
                waterVolume = plant.OnWater(waterVolume);
            }
            if (waterVolume.water > 0) {
                (WaterVolume portion, WaterVolume remainder) split = waterVolume.Split(RemainingWaterCapacity);
                waterVolume.water += split.portion.water;
                
                for(int compound=0; compound < waterVolume.compounds.Length; compound++) {
                    AddCompoundAmount(compound, split.portion.compounds[compound], split.remainder);
                }
                if (split.remainder.water > 0) {
                    return split.remainder;
                }
                else {
                    return WaterVolume.Empty;
                }
            }
            else {
                return WaterVolume.Empty;
            }
        }

        private void AddCompoundAmount(int compound,  float level, WaterVolume residual) {
            residual.compounds[compound] += AddCompoundAmount(compound, level);
        }

        public float AddCompoundAmmount(CompoundType compound, float level) {
            return AddCompoundAmount((int)compound, level);
        }

        public float AddCompoundAmount(int compound, float level) {
            float remainingCapacity = soilConfig.compoundCapacities[compound] - waterVolume.compounds[compound];
            if (level > remainingCapacity) {
                waterVolume.compounds[compound] = soilConfig.compoundCapacities[compound];
                return level - remainingCapacity;
            }
            else {
                waterVolume.compounds[compound] += level;
                return 0;
            }
        }

        // Start is called before the first frame update
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }
    }
}
