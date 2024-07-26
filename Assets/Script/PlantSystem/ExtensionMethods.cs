using UnityEngine;

public static class ExtensionMethods {
        
    public static float Sum(this Vector4 vecA) {
        var ret = 0f;
        for (int i = 0; i < 4; i++) {
            ret += vecA[i];
        }
        return ret;
    }

}