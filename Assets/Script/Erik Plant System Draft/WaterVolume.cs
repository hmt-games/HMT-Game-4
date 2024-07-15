using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

namespace ErikDraft {
    public struct WaterVolume {

        public float water;
        public float[] compounds;

        public static WaterVolume Empty = new WaterVolume(0);

        public WaterVolume(float water) {
            this.water = water;
            compounds = new float[System.Enum.GetValues(typeof(CompoundType)).Length];
        }

        public WaterVolume(WaterVolume waterVolume) {
            water = waterVolume.water;
            compounds = new float[System.Enum.GetValues(typeof(CompoundType)).Length];
            foreach (CompoundType type in System.Enum.GetValues(typeof(CompoundType))) {
                compounds[(int)type] = waterVolume.compounds[(int)type];
            }
        }

        public float GetCompoundAmmount(CompoundType type) {
            return compounds[(int)type];
        }

        public float GetCompoundConcentration(CompoundType type) {
            return compounds[(int)type] / water;
        }

        public static WaterVolume operator +(WaterVolume a, WaterVolume b) {
            WaterVolume result = new WaterVolume();
            result.water = a.water + b.water;
            for (int i = 0; i < a.compounds.Length; i++) {
                result.compounds[i] = a.compounds[i] + b.compounds[i];
            }
            return result;
        }

        public static WaterVolume operator +(WaterVolume a, float b) {
            WaterVolume result = new WaterVolume();
            result.water = a.water + b;
            return result;
        }

        public static WaterVolume operator -(WaterVolume a, float b) {
            WaterVolume result = new WaterVolume();
            var prev = a.water;
            result.water = a.water - b;
            for(int i = 0; i < a.compounds.Length; i++) {
                result.compounds[i] = a.compounds[i] * (result.water / prev);
            }
            return result;
        }

        public static WaterVolume operator -(WaterVolume a, WaterVolume b) {
            WaterVolume result = new WaterVolume();
            result.water = a.water - b.water;
            for (int i = 0; i < a.compounds.Length; i++) {
                result.compounds[i] = a.compounds[i] - b.compounds[i];
            }
            return result;
        }

        public static WaterVolume operator *(WaterVolume a, float b) {
            WaterVolume result = new WaterVolume();
            result.water = a.water * b;
            for (int i = 0; i < a.compounds.Length; i++) {
                result.compounds[i] = a.compounds[i] * b;
            }
            return result;
        }

        /// <summary>
        /// Splits the water volume into two parts, one of the specified size and the other the remainder.
        /// 
        /// The compounds in the water are split proportionally to the water volume to maintain their "concentration"
        /// This returns A tuple of the requested portion of water and any remainder.
        /// 
        /// TODO - maybe this should be a static method and use out parameters instead to avoid new object allocations
        /// </summary>
        /// <param name="portionSize"></param>
        /// <returns></returns>
        public (WaterVolume portion, WaterVolume remainder) Split(float portionSize) {
            if(portionSize >= water) {
                return (new WaterVolume(this), new WaterVolume());
            }
            else if(portionSize <= 0) {
                return (new WaterVolume(), new WaterVolume(this));
            }
            WaterVolume portion = new WaterVolume();
            WaterVolume remainder = new WaterVolume();
            portion.water = Mathf.Min(water, portionSize);
            remainder.water = water - portion.water;
            float ratio = portion.water / water;
            foreach (CompoundType type in System.Enum.GetValues(typeof(CompoundType))) {
                portion.compounds[(int)type] = compounds[(int)type] * ratio;
                remainder.compounds[(int)type] = compounds[(int)type] * (1 - ratio);
            }
            return (portion, remainder);
        }
    }

}