using HMT.Puppetry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using GameConstant;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public FarmBot FocusBot { get; private set; } = null;
    public byte actionPriority = 128;
    public LayerMask gridMask;
    public Color playerColor;

    [Header("Action Page")]
    [SerializeField] private GameObject actionPageParent;
    [SerializeField] private TMP_Text actionPageDescriptionTxt;
    [SerializeField] private Button harvestBtn;
    [SerializeField] private Button plantBtn;
    [SerializeField] private Button pickBtn;
    [SerializeField] private Button sampleBtn;
    [SerializeField] private Button sprayBtn;
    [SerializeField] private Button tillBtn;
    [SerializeField] private Button putDownBtn;
    [SerializeField] private Button pickUpBtn;
    [SerializeField] private Button movetoBtn;
    [SerializeField] private Button inspectBtn;
    [SerializeField] private Button focusBtn;
    [SerializeField] private TMP_Text focusTxt;

    [Header("Selection Page")]
    [SerializeField] private GameObject selectionPageParent;
    [SerializeField] private TMP_Text selectionPageDescriptionTxt;
    [SerializeField] private List<Button> selectionGrids;
    [SerializeField] private Sprite selectionGridDefaultSprite;
    private List<Image> _selectionGridSprites;
    [SerializeField] private Button confirmBtn;
    [SerializeField] private Button cancelBtn;

    [Header("QuickAction")]
    [SerializeField] private TMP_Text quickActionTxt;
    [SerializeField] private Image quickActionImg;

    private PlayerInputMapping _playerInputMapping;

    private InputAction _actMove;
    private InputAction _actSelect;
    private InputAction _actInteract;
    private InputAction _actQuickAction;
    private System.Action<InputAction.CallbackContext> _currentQuickActionBinding;

    private GridCellBehavior _currentGrid;
    private Transform _currentTransform;
    private string _currentActionString = "";

    private List<int> _selection;

    private bool _actionPageActive = false;
    private bool _selectionPageActive = false;

    public static PlayerController Instance;

    private void Select(InputAction.CallbackContext ctx) {
        RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), Mathf.Infinity, gridMask);
        if (hit.collider != null) {
            if (_currentTransform != null)
            {
                //Deselect();
                return;
            }

            _currentTransform = hit.transform;
            _currentGrid = _currentTransform.GetComponent<GridCellBehavior>();

            actionPageDescriptionTxt.text = _currentTransform.name;
            SetAvailableAction();
            ShowActionPage();
        }
        else
        {
            //Deselect();
        }
    }

    private void SetAvailableAction()
    {
        if (FocusBot == null || FocusBot.BotModeConfig == null) DeactivateAllActions();
        else
        {
            harvestBtn.interactable = FocusBot.BotModeConfig.ActionSupported("harvest");
            plantBtn.interactable = FocusBot.BotModeConfig.ActionSupported("plant");
            pickBtn.interactable = FocusBot.BotModeConfig.ActionSupported("pick");
            sampleBtn.interactable = FocusBot.BotModeConfig.ActionSupported("sample");
            sprayBtn.interactable = FocusBot.BotModeConfig.ActionSupported("spray");
            tillBtn.interactable = FocusBot.BotModeConfig.ActionSupported("till");
            putDownBtn.interactable = FocusBot.BotModeConfig.ActionSupported("drop");
            pickUpBtn.interactable = FocusBot.BotModeConfig.ActionSupported("take");
            movetoBtn.interactable = FocusBot.BotModeConfig.ActionSupported("move_to");
        }
        focusBtn.interactable = FocusBot != null || _currentGrid.botOccupant != null;
    }

    private void DeactivateAllActions()
    {
        harvestBtn.interactable = false;
        plantBtn.interactable = false;
        pickBtn.interactable = false;
        sampleBtn.interactable = false;
        sprayBtn.interactable = false;
        tillBtn.interactable = false;
        putDownBtn.interactable = false;
        pickUpBtn.interactable = false;
        movetoBtn.interactable = false;
    }

    private void ShowActionPage()
    {
        actionPageParent.transform.position = Camera.main.WorldToScreenPoint(_currentTransform.position);
        actionPageParent.SetActive(true);
        _actionPageActive = true;
    }

    private void HideActionPage()
    {
        actionPageParent.SetActive(false);
        _currentTransform = null;
        _currentGrid = null;
        _actionPageActive = false;
    }

    private void ShowSelectionPage()
    {
        HideActionPage();

        selectionPageParent.transform.position = actionPageParent.transform.position;
        selectionPageParent.SetActive(true);
        _selectionPageActive = true;
    }

    public void Deselect()
    {
        if (_selectionPageActive)
        {
            _selection = new List<int>(GLOBAL_CONSTANTS.MAX_PLANT_COUNT_PER_TILE);
            _currentActionString = "";
            selectionPageParent.SetActive(false);
            _selectionPageActive = false;
            
            actionPageParent.SetActive(true);
            _actionPageActive = true;
        }
        if (_actionPageActive)
        {
            _currentTransform = null;
            _currentGrid = null;
            _actionPageActive = false;
            actionPageParent.SetActive(false);
        }
    }

    public void ChangeBotFocus()
    {
        if (FocusBot == null)
        {
            FocusBot = _currentGrid.botOccupant;
            FocusBot.FocusBot(playerColor);
            focusTxt.text = "Unfocus";
            
            PlayerNetworkLocalSync.Instance.SendLocalFocusBot(FocusBot);
        }
        else
        {
            FocusBot.UnfocusBot();
            FocusBot = null;
            focusTxt.text = "Focus";
            
            PlayerNetworkLocalSync.Instance.SendLocalUnFocusBot();
        }
        
        HideActionPage();
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
        PlayerNetworkLocalSync.Instance.SendLocalMove(directionString);
    }

    private void Interact(InputAction.CallbackContext ctx) {
       
    }

    private void SelectionPageRequiredBtnPressed(string actionString)
    {
        _currentActionString = actionString;
        ShowSelectionPage();
    }

    private void SelectionPageButtonPressed(int idx)
    {
        Button btnPressed = selectionGrids[idx];
        GameObject checkmarkObj = btnPressed.transform.GetChild(0).gameObject;
        
        if (_selection.Contains(idx))
        {
            checkmarkObj.SetActive(false);
            _selection.Remove(idx);
        }
        else
        {
            checkmarkObj.SetActive(true);
            _selection.Add(idx);
        }

        confirmBtn.interactable = _selection.Count != 0;
    }

    public void SelectionConfirmed()
    {
        foreach (int idx in _selection)
        {
            Debug.Log($"{_currentActionString} {idx}");
            HMTPuppetManager.Instance.EnqueueCommand(
                new PuppetCommand(FocusBot.PuppetID, 
                    _currentActionString,
                    new Newtonsoft.Json.Linq.JObject {
                        { "target", idx },    
                    },
                    actionPriority));

            JObject matchStateJson = new JObject
            {
                {"actionString", _currentActionString},
                {"param", idx}
            };
            
            PlayerNetworkLocalSync.Instance.SendLocalParamsAction(_currentActionString, idx);
        }
        
        Deselect();
        Deselect();
    }

    private void ChangeBotMode(InputAction.CallbackContext ctx)
    {
        (FocusBot.CurrentTile as StationCellBehavior).Interact(FocusBot);
        Debug.Log("changed");
    }

    #region Bot Actions

    public void HarvestSelected()
    {
        if (_currentGrid is not SoilCellBehavior soil) return;

        _currentActionString = "harvest";
        selectionPageDescriptionTxt.text = "harvest";

        int activatedBtn = 0;
        for (int i = 0; i < soil.PlantCount; i++)
        {
            PlantBehavior plant = soil.plants[i];
            if (plant.hasFruit)
            {
                Sprite plantSprite = plant.config.plantSprites[3];
                selectionGrids[activatedBtn].interactable = true;
                _selectionGridSprites[activatedBtn].sprite = plantSprite;
                ScaleToFit(_selectionGridSprites[activatedBtn]);
                activatedBtn++;
            }
        }

        for (; activatedBtn < selectionGrids.Count; activatedBtn++)
        {
            selectionGrids[activatedBtn].interactable = false;
            _selectionGridSprites[activatedBtn].sprite = selectionGridDefaultSprite;
        }

        ShowSelectionPage();
    }

    #endregion

    #region Quick Action Cue

    public void ChangeQuickActionCue()
    {
        if (FocusBot == null) return;
        
        // clear previous bindings
        if (_currentQuickActionBinding != null)
        {
            _actQuickAction.performed -= _currentQuickActionBinding;
            _currentQuickActionBinding = null;
        }
        
        GridCellBehavior currentGrid = FocusBot.CurrentTile;
        if (currentGrid is StationCellBehavior)
        {
            quickActionImg.gameObject.SetActive(false);
            quickActionTxt.gameObject.SetActive(true);
            
            StationCellBehavior station = currentGrid as StationCellBehavior;
            switch (station.config.interaction)
            {
                case StationInteraction.Score:
                    quickActionTxt.text = "Score";
                    break;
                case StationInteraction.Trash:
                    quickActionTxt.text = "Trash";
                    break;
                case StationInteraction.SwitchBotMode:
                    quickActionTxt.text = "Change\nMode";
                    _actQuickAction.performed += ChangeBotMode;
                    _currentQuickActionBinding = ChangeBotMode;
                    Debug.Log("added");
                    break;
            }
        }

        else
        {
            
        }
    }

    #endregion

    #region Setup

    private void Awake()
    {
        _selection = new List<int>(GLOBAL_CONSTANTS.MAX_PLANT_COUNT_PER_TILE);
        _selectionGridSprites = new List<Image>(GLOBAL_CONSTANTS.MAX_PLANT_COUNT_PER_TILE);
        for (int i = 0; i < selectionGrids.Count; i++)
        {
            Button btn = selectionGrids[i];
            var i1 = i;
            btn.onClick.AddListener(delegate { SelectionPageButtonPressed(i1); });

            _selectionGridSprites.Add(btn.transform.GetComponent<Image>());
        }
        
        harvestBtn.onClick.AddListener(delegate { SelectionPageRequiredBtnPressed("harvest"); });
        pickUpBtn.onClick.AddListener(delegate { SelectionPageRequiredBtnPressed("pick"); });
        plantBtn.onClick.AddListener(delegate { SelectionPageRequiredBtnPressed("plant"); });

        Instance = this;
    }

    private void OnEnable() {
        _playerInputMapping = new PlayerInputMapping();

        _actMove = _playerInputMapping.Player.Move;
        _actMove.performed += Move;
        _actSelect = _playerInputMapping.Player.Select;
        _actSelect.performed += Select;
        _actQuickAction = _playerInputMapping.Player.QuickAction;
        
        _actMove.Enable();
        _actSelect.Enable();
        _actQuickAction.Enable();
    }
    
    private void OnDisable() {
        _actMove.Disable();
        _actSelect.Disable();
        _actQuickAction.Disable();
    }

    #endregion

    #region Helpers

    private void ScaleToFit(Image image)
    {
        RectTransform rectTransform = image.rectTransform;

        if (image.sprite == null) return;

        float spriteWidth = image.sprite.rect.width;
        float spriteHeight = image.sprite.rect.height;

        float rectWidth = rectTransform.rect.width;
        float rectHeight = rectTransform.rect.height;

        float scale = Mathf.Min(rectWidth / spriteWidth, rectHeight / spriteHeight);

        // Apply scale to the Image transform
        image.preserveAspect = true;
        image.SetNativeSize();
        image.rectTransform.localScale = new Vector3(scale, scale, 1f);
    }

    #endregion
}
