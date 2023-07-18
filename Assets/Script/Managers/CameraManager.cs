using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using util.GridRepresentation;

public class CameraManager : MonoBehaviour
{
    public static CameraManager S;
    
    [SerializeField] private CinemachineVirtualCamera referenceVCam;
    [SerializeField] private LevelConfig levelConfig;

    private List<CinemachineVirtualCamera> _layerCams;
    private int _currentActiveLayer = 0;

    private void Awake()
    {
        if (!S) S = this;
        else Destroy(this.gameObject);

        _layerCams = new List<CinemachineVirtualCamera>();
        CopyLayerVCam();
    }

    private void CopyLayerVCam()
    {
        _layerCams.Add(referenceVCam);
        Vector3 refPos = referenceVCam.transform.localPosition;
        Quaternion refRot = referenceVCam.transform.localRotation;
        LensSettings refLens = referenceVCam.m_Lens;

        for (int layer = 1; layer < levelConfig.layerCount; layer++)
        {
            GameObject nObj = new GameObject($"vcam_{layer + 1}")
            {
                transform =
                {
                    localPosition = refPos + Vector3.down * layer * GridRepresentation.layerSpacing,
                    localRotation = refRot,
                    parent = transform
                }
            };

            CinemachineVirtualCamera nVCam = nObj.AddComponent<CinemachineVirtualCamera>();
            nVCam.Priority = 0;
            nVCam.m_Lens = refLens;
            
            _layerCams.Add(nVCam);
        }

        _layerCams[0].Priority = 999;
    }

    public void ChangeCameraToLayer(int layer)
    {
        if (layer < 0 || layer > levelConfig.layerCount)
        {
            Debug.LogError($"Layer {layer} is not a valid layer");
        }
        
        layer--;
        _layerCams[_currentActiveLayer].Priority = 0;
        _layerCams[layer].Priority = 999;
        _currentActiveLayer = layer;
    }
}
