using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameConstant;
using Unity.VisualScripting;

public class GameActionGoldenFinger : MonoBehaviour
{
    public static GameActionGoldenFinger Instance;

    [SerializeField] private Transform actionWheel;
    [SerializeField] private Transform GF_Config;
    [SerializeField] private Transform GF_Info;
    private TMP_Text _infoText;

    private GridCellBehavior _selectedGrid;

    #region PlantFields

    private Transform _plantConfig;
    private TMP_Dropdown _speciesSelect;
    private TMP_InputField _rootMass;
    private TMP_InputField _height;
    private TMP_InputField _energyLevel;
    private TMP_InputField _health;
    private TMP_InputField _age;
    private TMP_InputField _water;
    private TMP_InputField _nutrients;

    #endregion

    #region Plant

    public void PlantSelected()
    {
        _plantConfig.gameObject.SetActive(true);
        GF_Info.gameObject.SetActive(true);
        actionWheel.gameObject.SetActive(false);
    }
    
    public void DisplaySpeciesInfo()
    {
        GF_Info.gameObject.SetActive(true);
        string optionText = _speciesSelect.options[_speciesSelect.value].text;
        _infoText.text = MapGeneratorJSON.Instance.plantConfigs[optionText].ToString();
    }

    public void PlantSubmit()
    {
        PlantConfig species = MapGeneratorJSON.Instance.plantConfigs[_speciesSelect.options[_speciesSelect.value].text];
        PlantInitInfo plantInitInfo = new PlantInitInfo(
            float.Parse(_rootMass.text),
            float.Parse(_height.text),
            float.Parse(_energyLevel.text),
            float.Parse(_health.text),
            float.Parse(_age.text),
            float.Parse(_water.text),
            String2Vector4(_nutrients.text));
        
        GameActions.Instance.Plant(species, plantInitInfo, _selectedGrid);
        
        _plantConfig.gameObject.SetActive(false);
        GF_Info.gameObject.SetActive(false);
    }

    #endregion

    #region Setup

    private void Awake()
    {
        if (Instance) Destroy(this);
        else Instance = this;

        _plantConfig = GF_Config.Find("Plant");
        _speciesSelect = _plantConfig.Find("Species").Find("Dropdown").GetComponent<TMP_Dropdown>();
        _rootMass = _plantConfig.Find("RootMass").GetChild(1).GetComponent<TMP_InputField>();
        _height = _plantConfig.Find("Height").GetChild(1).GetComponent<TMP_InputField>();
        _energyLevel = _plantConfig.Find("EnergyLevel").GetChild(1).GetComponent<TMP_InputField>();
        _health = _plantConfig.Find("Health").GetChild(1).GetComponent<TMP_InputField>();
        _age = _plantConfig.Find("Age").GetChild(1).GetComponent<TMP_InputField>();
        _water = _plantConfig.Find("Water").GetChild(1).GetComponent<TMP_InputField>();
        _nutrients = _plantConfig.Find("Nutrients").GetChild(1).GetComponent<TMP_InputField>();

        _infoText = GF_Info.Find("Info").GetComponent<TMP_Text>();
        
        GF_Info.gameObject.SetActive(false);
        for (int i = 0; i < GF_Config.childCount; i++)
        {
            GF_Config.GetChild(i).gameObject.SetActive(false);
        }
        actionWheel.gameObject.SetActive(false);
    }

    public void Init()
    {
        _speciesSelect.options = new List<TMP_Dropdown.OptionData>();
        foreach (var kvp in MapGeneratorJSON.Instance.plantConfigs)
        {
            _speciesSelect.options.Add(new TMP_Dropdown.OptionData(kvp.Key));
        }
    }

    #endregion

    #region System

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GetGridObjectAtMouse();
        }
    }
    
    /// <summary>
    /// TODO: very weird bug, this function cannot hit objects with layer Grid
    /// can only hit object that is NOT in the layer Grid
    /// Huh????????
    /// </summary>
    void GetGridObjectAtMouse()
    {
        //int gridLayerMask = 1 << LayerMask.NameToLayer("Grid");
        LayerMask gridLayerMask = LayerMask.NameToLayer("Grid");
        Debug.Log((int)gridLayerMask);

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 origin = Camera.main.ScreenToWorldPoint(mousePos);
        Debug.Log(origin);
        origin.z -= 0.5f;
        Vector3 direction = new Vector3(0.0f, 0.0f, 1.0f);
        Debug.Log(mousePos);

        Ray ray = new Ray(origin, direction);
        Debug.DrawRay(origin, direction, Color.red, 3.0f);

        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, gridLayerMask);
        //RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

        if (hit.collider != null)
        {
            Debug.Log(hit.transform.gameObject.name);
            _selectedGrid = hit.collider.gameObject.GetComponent<GridCellBehavior>();
            actionWheel.position = _selectedGrid.transform.position;
            actionWheel.gameObject.SetActive(true);
        }
    }

    #endregion
    
    private Vector4 String2Vector4(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            Debug.LogError("Input string is null or empty.");
            return Vector4.zero;
        }

        input = input.Replace(" ", "");

        string[] tokens = input.Split(',');
        if (tokens.Length != 4)
        {
            Debug.LogError("Input string must have exactly 4 comma-separated values.");
            return Vector4.zero;
        }

        if (!float.TryParse(tokens[0], out float x) ||
            !float.TryParse(tokens[1], out float y) ||
            !float.TryParse(tokens[2], out float z) ||
            !float.TryParse(tokens[3], out float w))
        {
            Debug.LogError("One or more components could not be parsed to a float.");
            return Vector4.zero;
        }

        return new Vector4(x, y, z, w);
    }
}
