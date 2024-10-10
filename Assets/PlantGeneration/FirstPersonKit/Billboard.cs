using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour
{
    public static Transform cam;
    public Vector3 freeRotation = Vector3.one;
    Vector3 eangles = Vector3.zero;

    void Start()
    {
        if (cam == null)
        {
            cam = Camera.main.GetComponent<Transform>();
        }
    }



    void LateUpdate()
    {
        transform.LookAt(cam);
        transform.Rotate(0, 180, 0);
        eangles = transform.eulerAngles;
        eangles.x *= freeRotation.x;
        eangles.y *= freeRotation.y;
        eangles.z *= freeRotation.z;
        transform.eulerAngles = eangles;
    }
}
