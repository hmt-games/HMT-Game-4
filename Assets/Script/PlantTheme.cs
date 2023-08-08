using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using util.GameRepresentation;

/* <summary>
 * SO for configuring plant 3D models by designers in editor. This is also
 * used by the plant spawning system to retrieve the correct 3D models during
 * run time. To accomplish this, a custom PlantInfo is used with accompanying
 * public methods that mimics a read only dictionary.
 * <contact> ziyul@andrew.cmu.edu
 */
[CreateAssetMenu(menuName = "Theme/Plant")]
public class PlantTheme : ScriptableObject
{
    [Serializable]
    public class PlantInfo
    {
        public PlantType plantType;
        public List<GameObject> plantStages;
    }

    // bias is necessary bc modeling param is not perfectly to scale
    public Vector3 scaleBias;
    public Vector3 rotationBias;
    public Vector3 positionBias;
    public List<PlantInfo> plantInfos;

    private bool TryGetPlant(PlantType plantType, out List<GameObject> plantStages)
    {
        foreach (PlantInfo _plantInfo in plantInfos)
        {
            if (_plantInfo.plantType == plantType)
            {
                plantStages = _plantInfo.plantStages;
                return true;
            }
        }

        plantStages = null;
        return false;
    }
    
    /* Call this to get all 3D models of a plant in a list.
     * returns false and out null if key not found.
     */
    public bool GetAllStagesOfPlant(PlantType plantType, out List<GameObject> allStages)
    {
        if (!TryGetPlant(plantType, out allStages)) return false;
        else return true;
    }
    
    /* Call this to get a specific stage's model.
     * returns false and out null if key not found.
     */
    public bool GetStageOfPlant(PlantType plantType, int stage, out GameObject model)
    {
        if (!TryGetPlant(plantType, out var allStages))
        {
            model = null;
            return false;
        }
        else
        {
            model = allStages[stage];
            return true;
        }
    }
}