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
    private bool _dormant = true;
    private bool _modelChanged = false;
    private int _enduranceTime = 0;
    private PlantConfigs.Plant _plantInfo;
    private bool propagated = false;
    public float _energy = 0.0f;

    public void StartTick()
    {
        InvokeRepeating(
            nameof(Tick), 
            dormantTime, 
            GlobalConstants.StateMachineTickTime);
    }
    
    // FSM loop
    private void Tick()
    {
        if (_dormant)
        {
            InitPlant();
            return;
        }
        
        // water nutrition check & deduct respective amount on gridNode;
        // update plant energy & endurance level;
        CheckConditions();
        
        // stay the same, evolve, propagate, die, ready to harvest
        // based on energy level and propagation rule
        CheckActions();
    }

    private void InitPlant()
    {
        plantStage = 0;
        GameManager.S.plantTheme.GetStageOfPlant(plantType, plantStage, out modelInDisplay);
        _modelChanged = true;

        if (GameManager.S.plantConfigs.GetPlantInfo(plantType, out _plantInfo))
        {
            _enduranceTime = _plantInfo.endurance;
        }
        else
        {
            Debug.LogError($"Plant {gameObject.name} has an invalid plant type, please check");
        }

        _dormant = false;
        _energy = 0.0f;
    }

    private void CheckConditions()
    {
        float water = parentGrid.waterLevel;
        Dictionary<NutritionType, float> nutrition = parentGrid.nutrition;

        // water level check & deduction
        if (water > _plantInfo.maxWaterLevel || water < _plantInfo.minWaterLevel)
        {
            _enduranceTime--;
        }
        else _energy += 0.1f;
        parentGrid.waterLevel = Math.Max(0.0f, water - 0.1f);
        
        // nutrition check & deduction
        foreach (NutritionType nutritionNeeded in _plantInfo.nutritionNeeded)
        {
            float thisNutrition = nutrition[nutritionNeeded];
            if (thisNutrition <= 0.0f) _enduranceTime--;
            else _energy += 0.1f;
            parentGrid.nutrition[nutritionNeeded] = Math.Max(0.0f, thisNutrition - 0.1f);
        }
    }

    // stay the same, evolve, propagate, die, ready to harvest
    private void CheckActions()
    {
        if (!propagated && _energy > _plantInfo.propagateEnergyThreshold) Propagate();
        if (_enduranceTime <= 0) Die();
    }

    private void Evolve()
    {
        throw new NotImplementedException();
    }

    private void Propagate()
    {
        var propagatePos = util.Propagate.Propagate.PropagatePlant(_plantInfo.propagateType, parentGrid.coordinate);
        foreach (var layerPos in propagatePos)
        {
            int layer = parentGrid.layer + layerPos.Key;
            foreach (var pos in layerPos.Value)
            {
                if (GameManager.S.CheckCoordValid(pos, layer))
                {
                    Debug.Log(pos);
                    GameManager.S.SpawnPlant(pos, layer, plantType);
                }
            }
        }

        propagated = true;
    }

    private void Die()
    {
        throw new NotImplementedException();
    }

    private void Harvest()
    {
        throw new NotImplementedException();
    }

    private void Update()
    {
        if (_modelChanged)
        {
            if (childModelObject) Destroy(childModelObject);
            Transform nChildModel = Instantiate(modelInDisplay, Vector3.zero, Quaternion.identity, transform).transform;
            childModelObject = nChildModel.gameObject;
            nChildModel.localScale = GameManager.S.plantTheme.scaleBias;
            nChildModel.localRotation = Quaternion.Euler(GameManager.S.plantTheme.rotationBias);
            nChildModel.localPosition = GameManager.S.plantTheme.positionBias;
            _modelChanged = false;
        }
    }
}
