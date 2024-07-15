using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace ErikDraft {
    public static class ExtensionMethods {
        
        public static float Sum(this Vector4 vecA) {
            var ret = 0f;
            for (int i = 0; i < 4; i++) {
                ret += vecA[i];
            }
            return ret;
        }

    }
}
