using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using util.GameRepresentation;
using GameConstants;

/* <summary>
 * The plant state machine is in charge of every individual plant's life cycle,
 * it will check the plant's condition every `StateMachineTickTime` seconds, and
 * decide how the plant will change in the next tick, e.g. evolve, propagate, die, etc.
 * <contact> ziyul@andrew.cmu.edu
 */
public class PlantStateMachine : MonoBehaviour
{
    [Header("META")]
    public GridNode parentGrid;
    public GameObject modelInDisplay;
    public GameObject childModelObject;
    
    [Space(15)]
    [Header("PlantInfo")]
    public PlantType plantType = PlantType.None;
    public int plantStage = 0;
    public float dormantTime;
    
    // FSM internal use
    private bool _modelChanged = false;

    public void StartTick()
    {
        InvokeRepeating(
            nameof(CheckState), 
            dormantTime, 
            GlobalConstants.StateMachineTickTime);
    }
    
    // FSM loop
    private void CheckState()
    {
        plantStage = 0;
        GameManager.S.plantTheme.GetStageOfPlant(plantType, plantStage, out modelInDisplay);
        // _modelChanged = true;
        
        if (!_modelChanged)
        {
            if (childModelObject) Destroy(childModelObject);
            Transform nChildModel = Instantiate(modelInDisplay, Vector3.zero, Quaternion.identity, transform).transform;
            childModelObject = nChildModel.gameObject;
            nChildModel.localScale = GameManager.S.plantTheme.scaleBias;
            nChildModel.localRotation = Quaternion.Euler(GameManager.S.plantTheme.rotationBias);
            nChildModel.localPosition = GameManager.S.plantTheme.positionBias;
            _modelChanged = true;
        }
    }

    private void Update()
    {
        // if (_modelChanged)
        // {
        //     if (childModelObject) Destroy(childModelObject);
        //     Transform nChildModel = Instantiate(modelInDisplay, Vector3.zero, Quaternion.identity, transform).transform;
        //     childModelObject = nChildModel.gameObject;
        //     nChildModel.localScale = GameManager.S.plantTheme.scaleBias;
        //     nChildModel.localRotation = Quaternion.Euler(GameManager.S.plantTheme.rotationBias);
        //     nChildModel.localPosition = GameManager.S.plantTheme.positionBias;
        //     _modelChanged = false;
        // }
    }
}
