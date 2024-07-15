using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Permissions;
using UnityEngine;

namespace ErikDraft {

    public class PlantBehavior : MonoBehaviour {

        /// <summary>
        /// Plants can in principle reach to multiple cells to draw resources from, but need to figure out the best way to represent this
        /// </summary>
        GridCellBehavior parentCell;

        private float _rootMass=0;
        public float RootMass { 
            get {
                return _rootMass;
            }
            private set {
                _rootMass = Mathf.Max(0, value);
            } 
        }

        private float _height = 0;
        public float Height {
            get {
                return _height;
            }
            private set {
                _height = Mathf.Max(0, value);
            }
        }   
        public float WaterLevel { get; private set; } = 0;
        public float EnergyLevel { get; private set; } = 0;
        /// <summary>
        /// Health Level of the Plant is a sliding window average changes in EnergyLevel.
        /// 
        /// Ideally this would be normalzied to a range of -1 to 1 but will need to figure out the math.
        /// </summary>
        public float Health { 
            get {
                healthTotal = 0;
                foreach (float f in healthHistory) {
                    healthTotal += f;
                }
                return healthTotal / healthHistory.Count;
            }
        }

        public float Age { get; private set; } = 0;

        public float[] CompoundLevels { get; private set; } = new float[System.Enum.GetValues(typeof(CompoundType)).Length];

        public PlantConfig config;

        public int maxHealthHistory = 10;

        private float healthTotal = 0;
        private Queue<float> healthHistory;
        
        // Start is called before the first frame update
        void Start() {
            
        }

        // Update is called once per frame
        void Update() {

        }

        public WaterVolume OnTick(WaterVolume allocation) {
            ///UPTAKE
            ///The moves an amount of each compound from the soil to the plant based on the uptake rate and the amount of the compound in the soil.
            for (int compound = 0; compound < CompoundLevels.Length; compound++) { 
                var uptake = Mathf.Min(config.uptakeRate[compound] * allocation.compounds[compound], config.capacities[compound] - CompoundLevels[compound]);
                CompoundLevels[compound] += uptake;
                allocation.compounds[compound] -= uptake;
            }

            /// METABOLISM
            /// Uses the uptake compounds to contribute to maintaining the plant and converting to energy
            var metabolismActual = 0f;
            var metabolismMax = 0f;
            for (int compound = 0; compound < CompoundLevels.Length; compound++) {
                metabolismMax+= config.metabolismNeeds[compound] * config.metabolismFactor[compound];
                var metabolismConsumption = Mathf.Min(CompoundLevels[compound], config.metabolismNeeds[compound]);
                metabolismActual += metabolismConsumption * config.metabolismFactor[compound];
                EnergyLevel += config.metabolismFactor[compound] * metabolismConsumption;
                CompoundLevels[compound] -= metabolismConsumption;
            }

            /// HEALTH
            /// The health calculation is a sliding window average of the metabolism rate
            if (healthHistory.Count >= maxHealthHistory) {
                healthHistory.Dequeue();
            }
            healthHistory.Enqueue(metabolismActual / metabolismMax);

            if (Health > config.growthToleranceThreshold) {
                var growth = 0f;
                /// GROWTH / Consumption
                /// This consumes compunds to contribute to growth
                for (int compound = 0; compound < CompoundLevels.Length; compound++) {
                    var growthConsumption = Mathf.Clamp(CompoundLevels[compound], 0, config.growthConsumptionRateLimit[compound]);
                    growth += config.growthFactor[compound] * growthConsumption;
                    CompoundLevels[compound] -= growthConsumption;
                    RootMass += growth * config.PercentToRoots(Age);
                    Height += growth * (1 - config.PercentToRoots(Age));
                }
            }

            /// LEECH
            /// leeches some amount of each compound back into the soil
            //for(int compound = 0; compound < CompoundLevels.Length; compound++) {
            //    var leech = Mathf.Min(config.leechRate[compound] * CompoundLevels[compound], parentCell.soilConfig.compoundCapacities[compound] - allocation.compounds[compound]);
            //    CompoundLevels[compound] -= leech;
            //    allocation.compounds[compound] += leech;
            //}
            return allocation;

            //TODOSome calculation of height has to go here...
        }

        public WaterVolume OnWater(WaterVolume waterVolume) {
            return waterVolume;
        }
    }

}