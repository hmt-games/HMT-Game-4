using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    private int _currentActiveCameraFloor = 0;
    public List<CinemachineVirtualCamera> floorCams;

    private PlayerInputMapping _playerInputMapping;
    private InputAction _floorCamUp;
    private InputAction _floorCamDown;
    
    private void Awake()
    {
        if (Instance) Destroy(this);
        else Instance = this;

        floorCams = new List<CinemachineVirtualCamera>();
        _playerInputMapping = new PlayerInputMapping();
    }

    private void FloorCamUp(InputAction.CallbackContext ctx)
    {
        if (_currentActiveCameraFloor >= floorCams.Count - 1) return;

        floorCams[_currentActiveCameraFloor].m_Priority = 0;
        _currentActiveCameraFloor++;
        floorCams[_currentActiveCameraFloor].m_Priority = 100;
    }
    
    private void FloorCamDown(InputAction.CallbackContext ctx)
    {
        if (_currentActiveCameraFloor <= 0) return;

        floorCams[_currentActiveCameraFloor].m_Priority = 0;
        _currentActiveCameraFloor--;
        floorCams[_currentActiveCameraFloor].m_Priority = 100;
    }
    
    private void OnEnable()
    {
        _floorCamUp = _playerInputMapping.Player.FloorCamUp;
        _floorCamUp.Enable();
        _floorCamUp.performed += FloorCamUp;
        
        _floorCamDown = _playerInputMapping.Player.FloorCamDown;
        _floorCamDown.Enable();
        _floorCamDown.performed += FloorCamDown;
    }

    private void OnDisable()
    {
        _floorCamUp.Disable();
        _floorCamDown.Disable();
    }
}