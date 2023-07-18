using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput _playerInput;
    private InputAction _layer1;
    private InputAction _layer2;
    private InputAction _layer3;
    private InputAction _layer4;
    private InputAction _layer5;

    private CameraManager _cameraManager;

    private void Awake()
    {
        _playerInput = new PlayerInput();
    }

    private void Start()
    {
        _cameraManager = CameraManager.S;
        
        _layer1.performed += context => { _cameraManager.ChangeCameraToLayer(1); };
        _layer2.performed += context => { _cameraManager.ChangeCameraToLayer(2); };
        _layer3.performed += context => { _cameraManager.ChangeCameraToLayer(3); };
        _layer4.performed += context => { _cameraManager.ChangeCameraToLayer(4); };
        _layer5.performed += context => { _cameraManager.ChangeCameraToLayer(5); };
    }

    private void OnEnable()
    {
        _layer1 = _playerInput.Player.Layer1;
        _layer2 = _playerInput.Player.Layer2;
        _layer3 = _playerInput.Player.Layer3;
        _layer4 = _playerInput.Player.Layer4;
        _layer5 = _playerInput.Player.Layer5;
        _layer1.Enable();
        _layer2.Enable();
        _layer3.Enable();
        _layer4.Enable();
        _layer5.Enable();
    }

    private void OnDisable()
    {
        _layer1.Disable();
        _layer2.Disable();
        _layer3.Disable();
        _layer4.Disable();
        _layer5.Disable();
    }
}
