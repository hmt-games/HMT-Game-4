using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlantSystem.Traits {

    public abstract class PlantTrait : ScriptableObject {

        public abstract string TraitName { get; }

        public virtual int Order { get; }

        public int MinStage = 0;


        /// <summary>
        /// Called when PlantData is set on the plant, for resetting any internal state
        /// </summary>
        /// <param name="plant"></param>
        public virtual void Setup(PlantBehavior plant) { }

        /// <summary>
        /// Called at the beginning of the OnTick process.
        /// 
        /// We might consider whether it makes sense to break out all of the substeps of the tick for separate functions.
        /// </summary>
        /// <param name="plant"></param>
        /// <returns></returns>
        public virtual bool OnTickPre(PlantBehavior plant, ref NutrientSolution allocation) {
            return true;
        }

        /// <summary>
        /// Called at the end of the OnTick process.
        /// </summary>
        /// <param name="plant"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        public virtual void OnTickPost(PlantBehavior plant, ref NutrientSolution allocation) { }

        public virtual void OnSpray(PlantBehavior plant, ref NutrientSolution nutrient) { }

        public virtual bool OnHarvest(PlantBehavior plant, FarmBot bot, ref PlantStateData altReturn) {
            return true;
        }

        public virtual bool OnPick(PlantBehavior plant, FarmBot bot, List<PlantStateData> altReturn) {
            return true;
        }

        public virtual bool OnTill(PlantBehavior plant, FarmBot bot, ref NutrientSolution altReturn) {
            return true;
        }

        public virtual bool OnStageTransition(PlantBehavior plant, int oldStage, int newStage) {
            return true;
        }

        public virtual bool OnBotEnter(PlantBehavior plant, FarmBot bot) {
            return true;
        }

    }

    public class  HeartyRoots: PlantTrait {
        
        override public string TraitName => "Hearty Roots";
        override public int Order => 0;

        public override void Setup(PlantBehavior plant) {
            MinStage = 1;
        }

        public override bool OnHarvest(PlantBehavior plant, FarmBot bot, ref PlantStateData altReturn) {
            PlantStateData currData = plant.GetPlantState();

            float percentage_to_stay = plant.config.stageTransitionThreshold[0] / plant.SurfaceMass;

            PlantStateData toStay = new PlantStateData(currData.config);
            toStay.rootMass = currData.rootMass * (1-percentage_to_stay);
            altReturn.rootMass = currData.rootMass * percentage_to_stay;

            toStay.nutrientLevels = currData.nutrientLevels * (percentage_to_stay);
            altReturn.nutrientLevels = currData.nutrientLevels * (1 - percentage_to_stay);

            toStay.energyLevel = currData.energyLevel * (percentage_to_stay);
            altReturn.energyLevel = currData.energyLevel * (1 - percentage_to_stay);

            toStay.surfaceMass = plant.config.stageTransitionThreshold[0];
            altReturn.surfaceMass = currData.surfaceMass - toStay.surfaceMass;

            toStay.healthHistory = currData.healthHistory.ToArray();
            toStay.currentHealthIndex = currData.currentHealthIndex;
            toStay.age = currData.age;
            toStay.currentStage = 0;
            toStay.config = currData.config;

            altReturn.healthHistory = currData.healthHistory.ToArray();
            altReturn.currentHealthIndex = currData.currentHealthIndex;
            altReturn.age = currData.age;
            altReturn.currentStage = currData.currentStage;
            altReturn.config = currData.config;

            plant.SetPlantState(toStay);
            return false;
        }
    }

    public class FilterLeaves : PlantTrait {
        override public string TraitName => "Filter Leaves";
        override public int Order => 0;

        [SerializeField]
        // There needs to be a mechanism to set this from the config
        private Vector4 filterFactor = new Vector4(0.1f, 0.1f, 0.1f, 0.1f);

        public override void Setup(PlantBehavior plant) {
            MinStage = plant.config.stageTransitionThreshold.Count / 2;
            filterFactor = new Vector4(.1f, .1f, .1f, .1f);
        }

        public override void OnSpray(PlantBehavior plant, ref NutrientSolution nutrient) {
            nutrient.nutrients = Vector4.Scale(nutrient.nutrients, filterFactor);
        }
    }

    public class SturdyTrunk : PlantTrait {
        override public string TraitName => "Sturdy Trunk";

        public override void Setup(PlantBehavior plant) {
            MinStage = plant.config.stageTransitionThreshold.Count / 2;
            base.Setup(plant);
        }

        public override bool OnBotEnter(PlantBehavior plant, FarmBot bot) {
            return false;
        }
    }
}
