using UnityEngine;

public static class ExtensionMethods {
        
    public static float Sum(this Vector4 vecA) {
        var ret = 0f;
        for (int i = 0; i < 4; i++) {
            ret += vecA[i];
        }
        return ret;
    }

    public static Vector4 ElementClamp (this Vector4 vecA, Vector4 vecB) {
        var ret = new Vector4();
        for (int i = 0; i < 4; i++) {
            if (vecB[i] > 0) {
                ret[i] = Mathf.Clamp(vecA[i], 0, vecB[i]);
            }
            else {
                ret[i] = Mathf.Clamp(vecA[i], vecB[i], 0);
            }   
        }
        return ret;
    }

    public static Vector4 Positives(this Vector4 vec)
    {
        Vector4 ret = new Vector4();
        for (int i = 0; i < 4; i++)
        {
            ret[i] = Mathf.Max(0.0f, vec[i]);
        }

        return ret;
    }
}