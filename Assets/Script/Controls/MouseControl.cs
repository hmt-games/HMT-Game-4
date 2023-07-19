using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using util.GridRepresentation;

public class MouseControl : MonoBehaviour
{
    private Camera _mainCamera;
    [SerializeField] private LayerMask layerMask;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask))
        {
            GameObject gridObj = raycastHit.transform.gameObject;
            GirdInfo girdInfo = gridObj.GetComponent<GirdInfo>();
            Debug.Log($"{girdInfo.coordinate} at layer {girdInfo.layer}");
        }
        else
        {
            Debug.Log($"nothing found {layerMask.value}");
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
        }
    }
}
