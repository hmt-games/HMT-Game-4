using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Permissions;
using UnityEngine;
using Fusion;

public class PlantBehavior : NetworkBehaviour {

        /// <summary>
        /// Plants can in principle reach to multiple cells to draw resources from, but need to figure out the best way to represent this
        /// </summary>
        GridCellBehavior parentCell;

        [Networked]
        private float _rootMass { get; set; } = 0;

        public float RootMass { 
            get {
                return _rootMass;
            }
            private set {
                _rootMass = Mathf.Max(0, value);
            } 
        }

        [Networked]
        private float _height { get; set; } = 0;

        public float Height {
            get {
                return _height;
            }
            private set {
                _height = Mathf.Max(0, value);
            }
        }

        public float WaterLevel {
            get {
                return NutrientLevels.water;
            }
        }

        [Networked]
        public float EnergyLevel { get; private set; } = 0;

        /// <summary>
        /// Health Level of the Plant is a sliding window average changes in EnergyLevel.
        /// 
        /// Ideally this would be normalzied to a range of -1 to 1 but will need to figure out the math.
        /// </summary>
        /// 
        [Networked]
        public float Health { get; private set; } = 0;
        /*
        public float Health { 
            get {
                healthTotal = 0;
                foreach (float f in healthHistory) {
                    healthTotal += f;
                }
                return healthTotal / healthHistory.Count;
            }
        }
        */

        [Networked]
        public float Age { get; private set; } = 0;
    
        //public float[] CompoundLevels { get; private set; } = new float[System.Enum.GetValues(typeof(NutrientType)).Length];

        /*
        [Networked]
        private NutrientSolution NutrientLevels { get; set; }
        */
        [Networked]
        private ref NutrientSolution NutrientLevels => ref MakeRef<NutrientSolution>(new NutrientSolution(0));

    public PlantConfig config;

        public int maxHealthHistory { get; set; } = 10;

        private float healthTotal { get; set; } = 0;

        //Need to think about how to sync this.
        //Maybe not syncthing this at all
        private Queue<float> healthHistory;
        
        // Start is called before the first frame update
        void Start() {
            
        }

        // Update is called once per frame
        void Update() {

        }

        public NutrientSolution OnTick(NutrientSolution allocation) {
            /// UPTAKE
            /// The moves an amount of each compound from the soil to the plant based on the uptake rate and the amount of the compound in the soil.
            /// either it moves the amount based on its uptake rate OR up to the ammount it can contain if it's full
            var uptake = Vector4.Min(Vector4.Scale(config.uptakeRate, allocation.nutrients) , config.capacities - NutrientLevels.nutrients);
            NutrientLevels.nutrients += uptake;
            allocation.nutrients -= uptake;

            /// METABOLISM
            /// Uses the uptake nutrients to contribute to maintaining the plant and converting to energy      
            var metabolismConsumption = Vector4.Min(NutrientLevels.nutrients, config.metabolismNeeds);
            EnergyLevel += Vector4.Dot(config.metabolismFactor, metabolismConsumption);
            NutrientLevels.nutrients -= metabolismConsumption;

            /// HEALTH
            /// The health calculation is a sliding window average of the metabolism rate
            if (healthHistory.Count >= maxHealthHistory) {
                healthHistory.Dequeue();
            }
            healthHistory.Enqueue(metabolismConsumption.Sum() / config.metabolismNeeds.Sum());

            if (Health > config.growthToleranceThreshold) {
                /// GROWTH / Consumption
                /// This consumes compunds to contribute to growth
                var growthConsumption = Vector4.Min(NutrientLevels.nutrients, config.growthConsumptionRateLimit);
                var growth = Vector4.Dot(config.growthFactor, growthConsumption);
                NutrientLevels.nutrients -= growthConsumption;
                RootMass += growth * config.PercentToRoots(Age);
                Height += growth * (1 - config.PercentToRoots(Age));
            }

            //update health value
            healthTotal = 0;
            foreach (float f in healthHistory)
            {
                healthTotal += f;
            }
            Health = healthTotal / healthHistory.Count;


        /// LEECH
        /// Originally I was thinking of having plants leech somehting back into the soil but realized it was more complicated.
        /// This should probably be a trait but it also might be something we could use for when a plant is dead.
        /// leeches some amount of each compound back into the soil
        //for(int compound = 0; compound < CompoundLevels.Length; compound++) {
        //    var leech = Mathf.Min(config.leechRate[compound] * CompoundLevels[compound], parentCell.soilConfig.compoundCapacities[compound] - allocation.nutrients[compound]);
        //    CompoundLevels[compound] -= leech;?";[]=----p['
        //    allocation.nutrients[compound] += leech;
        //}
        return allocation;
        }

        /// <summary>
        /// Handles when water lands on the plant.
        /// 
        /// By default this should be a no-op but is here so it can be overridden by possible future Traits.
        /// </summary>
        /// <param name="waterVolume"></param>
        /// <returns></returns>
        public NutrientSolution OnWater(NutrientSolution waterVolume) {
            return waterVolume;
        }
    }