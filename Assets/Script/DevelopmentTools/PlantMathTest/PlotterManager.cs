using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlotterManager : MonoBehaviour
{
    [SerializeField] private int plotterIdx = 0;
    [SerializeField] private int maxVisibleValueAmount = -1;
    //[SerializeField] private GameObject plotterSelectUI;

    [SerializeField] private TMP_Dropdown gridIdxSelect;
    [SerializeField] private TMP_Dropdown typeSelect;
    [SerializeField] private TMP_Dropdown soilDataSelect;
    [SerializeField] private GameObject plantSelect;
    [SerializeField] private TMP_Dropdown plantIdxSelect;
    [SerializeField] private TMP_Dropdown plantDataSelect;

    private Dictionary<int, PlantMathDataLogger.TrackedPlantDataType> _plantSelectToDataType;
    private Dictionary<int, PlantMathDataLogger.TrackedSoilDataType> _soilSelectToDataType;

    private bool _plottingPlant = true;
    private GridBehaviorLocalTest _selectedSoil;
    private PlantMathDataLogger.TrackedSoilDataType _selectedSoilData;
    private List<PlantBehaviorLocalTest> _selectedPlants;
    private PlantBehaviorLocalTest _selectedPlant;
    public PlantMathDataLogger.TrackedPlantDataType _selectedPlantData;

    private Window_Graph _grapher;

    public void OnTick()
    {
        if (_plottingPlant)
        {
            PlotPlantGraph();
        }
        else
        {
            PlotSoilGraph();
        }
    }

    private void Awake()
    {
        _plantSelectToDataType = new Dictionary<int, PlantMathDataLogger.TrackedPlantDataType>();
        _soilSelectToDataType = new Dictionary<int, PlantMathDataLogger.TrackedSoilDataType>();
    }

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        PlantMathTestManager.Instance.AddPlotter(this);
        if (plotterIdx == 0) _grapher = Window_Graph.Instance0;
        else _grapher = Window_Graph.Instance1;
        
        // gridIdxSelect
        List<string> gridIdxOptions = new List<string>();
        for (int i = 0; i < PlantMathTestManager.Instance.simulatedGrid.Count; i++)
        {
            gridIdxOptions.Add($"Soil{i}");
        }
        gridIdxSelect.AddOptions(gridIdxOptions);
        gridIdxSelect.onValueChanged.AddListener(delegate { OnGridIdxSelectChanged(gridIdxSelect); });
        OnGridIdxSelectChanged(gridIdxSelect);
        
        // plantIdxSelect
        plantIdxSelect.onValueChanged.AddListener(delegate { OnPlantIdxSelectChange(plantIdxSelect); });
        OnPlantIdxSelectChange(plantIdxSelect);
        
        typeSelect.onValueChanged.AddListener(delegate { OnTypeSelectChange(typeSelect); });

        // soilDataSelect
        int valueCount = 0;
        string[] soilDataNames = Enum.GetNames(typeof(PlantMathDataLogger.TrackedSoilDataType));
        List<string> soilDataOptions = new List<string>();
        foreach (var soilData in Enum.GetValues(typeof(PlantMathDataLogger.TrackedSoilDataType)))
        {
            soilDataOptions.Add(soilDataNames[valueCount]);
            _soilSelectToDataType[valueCount] = (PlantMathDataLogger.TrackedSoilDataType)soilData;
            valueCount++;
        }
        soilDataSelect.AddOptions(soilDataOptions);
        
        // plantDataSelect
        valueCount = 0;
        string[] plantDataNames = Enum.GetNames(typeof(PlantMathDataLogger.TrackedPlantDataType));
        List<string> plantDataOptions = new List<string>();
        foreach (var plantData in Enum.GetValues(typeof(PlantMathDataLogger.TrackedPlantDataType)))
        {
            plantDataOptions.Add(plantDataNames[valueCount]);
            _plantSelectToDataType[valueCount] = (PlantMathDataLogger.TrackedPlantDataType)plantData;
            valueCount++;
        }
        plantDataSelect.AddOptions(plantDataOptions);
        plantDataSelect.onValueChanged.AddListener(delegate{ OnPlantDataSelectChange(plantDataSelect); });
        OnPlantDataSelectChange(plantDataSelect);
    }

    public void OnGridIdxSelectChanged(TMP_Dropdown change)
    {
        plantIdxSelect.options = new List<TMP_Dropdown.OptionData>();
        _selectedSoil = PlantMathTestManager.Instance.simulatedGrid[change.value];
        _selectedPlants = _selectedSoil.plants;

        List<string> plantIdxOptions = new List<string>();
        for (int i = 0; i < _selectedPlants.Count; i++)
        {
            plantIdxOptions.Add($"Plant{i}");
        }
        plantIdxSelect.AddOptions(plantIdxOptions);
    }
    
    public void OnPlantIdxSelectChange(TMP_Dropdown change)
    {
        _selectedPlant = _selectedPlants[plantIdxSelect.value];
    }

    public void OnTypeSelectChange(TMP_Dropdown change)
    {
        if (change.value == 0)
        {
            plantSelect.SetActive(true);
            _plottingPlant = true;
            soilDataSelect.GameObject().SetActive(false);
        }
        else
        {
            plantSelect.SetActive(false);
            _plottingPlant = false;
            soilDataSelect.GameObject().SetActive(true);
            PlotSoilGraph();
        }
    }

    public void OnPlantDataSelectChange(TMP_Dropdown change)
    {
        _selectedPlantData = _plantSelectToDataType[change.value];
        PlotPlantGraph();
    }

    private void PlotPlantGraph()
    {
        if (_selectedPlantData == PlantMathDataLogger.TrackedPlantDataType.Water
            || _selectedPlantData == PlantMathDataLogger.TrackedPlantDataType.NutrientA
            || _selectedPlantData == PlantMathDataLogger.TrackedPlantDataType.NutrientB
            || _selectedPlantData == PlantMathDataLogger.TrackedPlantDataType.NutrientC
            || _selectedPlantData == PlantMathDataLogger.TrackedPlantDataType.NutrientD)
        {
            List<float> dataPoints = PlantMathDataLogger.Instance.GetPlantData(_selectedPlant, PlantMathDataLogger.TrackedPlantDataType.Water);
            if (dataPoints.Count < 1) return;
            List<float> dataPointsA = PlantMathDataLogger.Instance.GetPlantData(_selectedPlant, PlantMathDataLogger.TrackedPlantDataType.NutrientA);
            List<float> dataPointsB = PlantMathDataLogger.Instance.GetPlantData(_selectedPlant, PlantMathDataLogger.TrackedPlantDataType.NutrientB);
            List<float> dataPointsC = PlantMathDataLogger.Instance.GetPlantData(_selectedPlant, PlantMathDataLogger.TrackedPlantDataType.NutrientC);
            List<float> dataPointsD = PlantMathDataLogger.Instance.GetPlantData(_selectedPlant, PlantMathDataLogger.TrackedPlantDataType.NutrientD);

            var combined =
                new[] { dataPoints, dataPointsA, dataPointsB, dataPointsC, dataPointsD }.SelectMany(list => list);
            float maxValue = combined.Max();
            float minValue = combined.Min();
            _grapher.ShowGraph(dataPoints, maxVisibleValueAmount, i => $"t{i}", f => $"{f:F1}", minValue, maxValue);
            _grapher.ShowGraphOnTop(dataPointsA, maxVisibleValueAmount, minValue, maxValue);
            _grapher.ShowGraphOnTop(dataPointsB, maxVisibleValueAmount, minValue, maxValue);
            _grapher.ShowGraphOnTop(dataPointsC, maxVisibleValueAmount, minValue, maxValue);
            _grapher.ShowGraphOnTop(dataPointsD, maxVisibleValueAmount, minValue, maxValue);
        }
        else
        {
            List<float> dataPoints = PlantMathDataLogger.Instance.GetPlantData(_selectedPlant, _selectedPlantData);
            if (dataPoints.Count < 1) return;
            _grapher.ShowGraph(dataPoints, maxVisibleValueAmount, i => $"t{i}", f => $"{f:F1}");
        }
    }

    private void PlotSoilGraph()
    {
        List<float> dataPoints = PlantMathDataLogger.Instance.GetSoilData(_selectedSoil, PlantMathDataLogger.TrackedSoilDataType.Water);
        if (dataPoints.Count < 1) return;
        List<float> dataPointsA = PlantMathDataLogger.Instance.GetSoilData(_selectedSoil, PlantMathDataLogger.TrackedSoilDataType.NutrientA);
        List<float> dataPointsB = PlantMathDataLogger.Instance.GetSoilData(_selectedSoil, PlantMathDataLogger.TrackedSoilDataType.NutrientB);
        List<float> dataPointsC = PlantMathDataLogger.Instance.GetSoilData(_selectedSoil, PlantMathDataLogger.TrackedSoilDataType.NutrientC);
        List<float> dataPointsD = PlantMathDataLogger.Instance.GetSoilData(_selectedSoil, PlantMathDataLogger.TrackedSoilDataType.NutrientD);
        
        var combined =
            new[] { dataPoints, dataPointsA, dataPointsB, dataPointsC, dataPointsD }.SelectMany(list => list);
        float maxValue = combined.Max();
        float minValue = combined.Min();
        _grapher.ShowGraph(dataPoints, maxVisibleValueAmount, i => $"t{i}", f => $"{f:F1}", minValue, maxValue);
        _grapher.ShowGraphOnTop(dataPointsA, maxVisibleValueAmount, minValue, maxValue);
        _grapher.ShowGraphOnTop(dataPointsB, maxVisibleValueAmount, minValue, maxValue);
        _grapher.ShowGraphOnTop(dataPointsC, maxVisibleValueAmount, minValue, maxValue);
        _grapher.ShowGraphOnTop(dataPointsD, maxVisibleValueAmount, minValue, maxValue);
    }
}
