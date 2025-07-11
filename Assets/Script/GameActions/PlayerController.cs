using HMT.Puppetry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {

    public FarmBot FocusBot { get; private set; } = null;
    public byte actionPriority = 128;
    public LayerMask gridMask;
    public Color playerColor;

    private PlayerInputMapping _playerInputMapping;

    InputAction _actMove;
    InputAction _actSelect;
    InputAction _actInteract;


    private void OnEnable() {
        _playerInputMapping = new PlayerInputMapping();

        _actMove = _playerInputMapping.Player.Move;
        _actMove.performed += Move;
        _actSelect = _playerInputMapping.Player.Select;
        _actSelect.performed += Select;
        //_actInteract = _playerInputMapping.Player.Interact;
        //_actInteract.performed += Interact;
        _actMove.Enable();
        _actSelect.Enable();
        //_actInteract.Enable();
    }

    private void Select(InputAction.CallbackContext ctx) {
        // all clicking should be done on tiles, we can then get the focus bot based on the tile clicked
        RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), Mathf.Infinity, gridMask);
        if (hit.collider != null) {
            GridCellBehavior cell = hit.collider.gameObject.GetComponent<GridCellBehavior>();
            if (cell == null) return;
            
            if(FocusBot == null) {
                if(cell.botOnGrid) {
                    if (FocusBot == null) return;
                }
                else {
                    // if the cell is empty, we can do nothing
                    return;
                }
            }
            else {

            }

        }
    }

    private void Move(InputAction.CallbackContext ctx) {
        if(FocusBot == null) return;

        Vector2 direction = ctx.ReadValue<Vector2>();

        string directionString = direction.x > 0 ? "right" :
                                 direction.x < 0 ? "left" :
                                 direction.y > 0 ? "up" :
                                 direction.y < 0 ? "down" : "";

        HMTPuppetManager.Instance.EnqueueCommand(
            new PuppetCommand(FocusBot.PuppetID, 
                "move",
                new Newtonsoft.Json.Linq.JObject {
                    { "direction", directionString },    
                },
                actionPriority));
    }

    private void Interact(InputAction.CallbackContext ctx) {
       
    }

    private void OnDisable() {
        _actMove.Disable();
        _actSelect.Disable();
        _actInteract.Disable();
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
