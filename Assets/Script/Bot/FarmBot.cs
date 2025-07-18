using GameConstant;
using HMT.Puppetry;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WebSocketSharp;

[RequireComponent(typeof(Animator))]
public class FarmBot : PuppetBehavior, IPoolCallbacks {

    //public enum BotActionState {
    //    Idle,
    //    Moving,
    //    Interacting,
    //    Harvesting,
    //    Planting,
    //    Spraying,
    //    Tilling,
    //    TakingInventory,
    //    DroppingInventory,
    //    Sampling,
    //    Picking
    //}

    #region Inspector Properties

    [Header("Subobject Pointers")]
    [SerializeField]
    protected SpriteRenderer faceSprite;
    [SerializeField]
    protected SpriteRenderer bodySprite;
    [SerializeField]
    protected SpriteRenderer hatSprite;
    [SerializeField]
    protected SpriteRenderer selectionCircleBase;
    [SerializeField]
    protected SpriteRenderer selectionCircleStripe;
    [SerializeField]
    [Tooltip("This should be the outer wraper object of the Progress Bar")]
    protected GameObject progressBar;
    [SerializeField]
    [Tooltip("This should be the inner fill object of the Progress Bar")]
    protected GameObject progressBarFill;
    [SerializeField]
    protected GameObject emoteBubble;
    [SerializeField]
    protected SpriteRenderer emote;

    #endregion

    #region Public Fields
    public BotInventory Inventory { get; private set; }
    //public BotActionState ActionState { get; private set; } = BotActionState.Idle;

    public BotModeSO BotModeConfig { get => _botModeConfig; }

    #endregion


    #region Protected Fields

    protected int FloorIdx;
    protected Vector2Int CellIdx;
    protected Animator _animator;
    protected BotModeSO _botModeConfig;

    #endregion

    private void Awake() {
        puppetIDPrefix = "farm_bot";
        _animator = GetComponent<Animator>();
        if (_animator == null) {
            Debug.LogError("Animator component is missing on FarmBot.");
        }
        if (faceSprite == null || bodySprite == null || hatSprite == null || selectionCircleBase == null ||
            progressBar == null || progressBarFill == null || emoteBubble == null || emote == null) {
            Debug.LogError("One or more subobject pointers are not assigned in FarmBot.");
        }
    }

    protected override void Start() {
        base.Start();
        selectionCircleBase.gameObject.SetActive(false);
        selectionCircleStripe.gameObject.SetActive(false);
        progressBar.SetActive(false);
        emoteBubble.SetActive(false);
        Inventory = BotInventory.None;
        RegisterStandardActions();
    }

    private void RegisterStandardActions() {
        RegisterAction("move", Move);
        RegisterAction("move_to", PuppetActionNotImplemented);
        RegisterAction("interact", Interact);
        RegisterAction("take", PickUp);
        RegisterAction("pick_up", PickUp);
        RegisterAction("drop", PutDown);
        RegisterAction("put_down", PickUp);
    }

    // Update is called once per frame
    void Update() {

    }

    #region Bot Actions

    private void Move(PuppetCommand command) {

        //TODO not sure we need this? IT should be impossible for the dispatcher to get this far if the bot is already acting
        //if (ActionState == BotActionState.Moving) {
        //    command.SendIllegalActionResponse("Bot is already moving");
        //    return;
        //}
        JObject Params = command.ActionParams;
        string direction = Params["direction"].ToString();
        if (direction.IsNullOrEmpty()) {
            command.SendMissingParametersResponse(new JObject {
                {"direction", new JArray{"up", "down", "left", "right"}}
            });
            return;
        }

        Vector2Int direct = direction switch {
            "up" => new Vector2Int(0, 1),
            "down" => new Vector2Int(0, -1),
            "left" => new Vector2Int(-1, 0),
            "right" => new Vector2Int(1, 0),
            _ => Vector2Int.zero
        };

        Vector2Int targetPos = direct + CellIdx;
        if (!ValidTargetPosition(targetPos)) {
            command.SendIllegalActionResponse("Attempting to move bot out of bounds");
            return;
        }

        // check if there is already a bot on target tile
        GridCellBehavior targetGrid = CurrentFloor.Cells[targetPos.x, targetPos.y];
        if (targetGrid.botOnGrid) {
            command.SendIllegalActionResponse("Attempting to move on tile that also has a bot on it");
            return;
        }

        CurrentCommand = command;
        StartCoroutine(MoveCoroutine(direct));
    }

    IEnumerator MoveCoroutine(Vector2Int direction) {
        CurrentTile.botOnGrid = false;
        GridCellBehavior targetGrid =
            CurrentFloor.Cells[CellIdx.x + direction.x, CellIdx.y + direction.y];
        //targetGrid.botOnGrid = true;

        //ActionState = BotActionState.Moving;
        Vector3 target = targetGrid.transform.position;
        if (GameManager.Instance.secondPerTick > 0) {
            float moveDuration = Vector3.Distance(transform.position, target) * GameManager.Instance.secondPerTick / _botModeConfig.movementSpeed;
            float startTime = Time.time;
            while (Time.time - startTime < moveDuration) {
                transform.position = Vector3.Lerp(transform.position, target, (Time.time - startTime) / moveDuration);
                yield return null;
            }
        }
        CellIdx += direction;
        transform.position = target;
        CurrentTile.botOccupant = this;
        CurrentTile.botOnGrid = true;

        CurrentCommand = PuppetCommand.IDLE;
        //ActionState = BotActionState.Idle;
    }

    private IEnumerator Interact(PuppetCommand command) {
        if (CurrentTile is StationCellBehavior stationCell) {
            if (stationCell.IsCompatible(this)) {
                //ActionState = BotActionState.Interacting;
                //do the wait time thing

                yield return WaitAnimation("interact", stationCell.config.interactionTime);

                if (CurrentCommand.Command != PuppetCommandType.IDLE && CurrentCommand.Command != PuppetCommandType.STOP) {
                    stationCell.Interact(this);
                }

                //ActionState = BotActionState.Idle;
            }
            else {
                command.SendIllegalActionResponse("Bot is not compatible with this station cell.");
            }
        }
        else {
            command.SendIllegalActionResponse("Bot can only interact with station tiles.");
        }
        
    }

    private IEnumerator PickUp(PuppetCommand command) {
        if (!CurrentTile.hasInventoryDrop) {
            command.SendIllegalActionResponse("Tile does not have an inventory to pick up");
            yield break;
        }

        switch (Inventory.Mode, CurrentTile.InventoryDrop.Mode) {
            case (InventoryMode.PlantInventory, InventoryMode.Reservoir):
            case (InventoryMode.Reservoir, InventoryMode.PlantInventory):
                command.SendIllegalActionResponse("Inventory box is incompatible with bot mode.");
                yield break;

        }

        CurrentCommand = command;
        yield return WaitAnimation("take");

        if (CurrentCommand.Command == PuppetCommandType.STOP) {
            CurrentCommand = PuppetCommand.IDLE;
            yield break;
        }

        GameActions.Instance.PickupInventory(this, CurrentTile);
        CurrentCommand = PuppetCommand.IDLE;
    }

    private IEnumerator PutDown(PuppetCommand command) {
        if (!CurrentTile.AllowsDrops) {
            command.SendIllegalActionResponse("Tile does not allow inventory drops");
            yield break;
        }
        if (CurrentTile.hasInventoryDrop) {
            command.SendIllegalActionResponse("Tile already has a box on it.");
            yield break;
        }
        if (Inventory.IsEmpty) {
            command.SendIllegalActionResponse("Bot does not have an inventory to drop");
            yield break;
        }

        CurrentCommand = command;
        yield return WaitAnimation("drop");

        if (CurrentCommand.Command == PuppetCommandType.STOP) {
            CurrentCommand = PuppetCommand.IDLE;
            yield break;
        }

        GameActions.Instance.DropInventory(this, CurrentTile);
        CurrentCommand = PuppetCommand.IDLE;
    }

    private IEnumerator Sample(PuppetCommand command) {
        if(CurrentTile is SoilCellBehavior soilCell) {

            CurrentCommand = command;
            yield return WaitAnimation("sample");

            if (CurrentCommand.Command == PuppetCommandType.STOP) {
                CurrentCommand = PuppetCommand.IDLE;
                yield break;
            }

            //ideally this sampleVolume would come from the botConfig
            GameActions.Instance.Sample(this, soilCell, GameActions.Defaults.SampleVolumePerAction);
            CurrentCommand = PuppetCommand.IDLE;
        }
        else {
            command.SendIllegalActionResponse("Bot can only sample soil cells.");
            yield break;
        }
    }

    private IEnumerator Spray(PuppetCommand command) {
        if (CurrentTile is SoilCellBehavior soilCell) {
            if (!Inventory.HasReservoir) {
                command.SendIllegalActionResponse("Bot does not have a reservoir to spray.");
                yield break;
            }

            CurrentCommand = command;
            yield return WaitAnimation("spray");
            if (CurrentCommand.Command == PuppetCommandType.STOP) {
                CurrentCommand = PuppetCommand.IDLE;
                yield break;
            }

            GameActions.Instance.Spray(this, soilCell, GameActions.Defaults.SprayAmountPerAction);
            CurrentCommand = PuppetCommand.IDLE;

        }
        else {
            command.SendIllegalActionResponse("Bot can only spray soil cells.");
        }
    }

    private IEnumerator Pick(PuppetCommand command) {
        if(CurrentTile is SoilCellBehavior soilCell) {
            //must have inventory space
            if (!Inventory.HasPlantInventory) {
                command.SendIllegalActionResponse("Bot does not have a plant inventory to pick plants into.");
                yield break;
            }

            //need to check for a target param
            JObject Params = command.ActionParams;
            int target = Params.TryGetDefault("target",-1);
            if(target < 0) {
                for(int i =0; i < soilCell.plants.Count; i++) {
                    if (soilCell.plants[i].hasFruit) {
                        target = i;
                        break;
                    }
                }
            }
            if (target < 0) {
                command.SendIllegalActionResponse($"Invalid target plant {target}.");
                yield break;
            }

            CurrentCommand = command;
            yield return WaitAnimation("pick");
            if (CurrentCommand.Command == PuppetCommandType.STOP) {
                CurrentCommand = PuppetCommand.IDLE;
                yield break;
            }

            GameActions.Instance.Pick(this, soilCell, target);
            CurrentCommand = PuppetCommand.IDLE;
        }
        else {
            command.SendIllegalActionResponse("Bot can only pick plants from soil cells.");
            yield break;
        }
    }

    private IEnumerator Plant(PuppetCommand command) {
        if(CurrentTile is SoilCellBehavior soilCell) {
            if(!Inventory.HasPlantInventory) {
                command.SendIllegalActionResponse("Bot does not have a plant inventory to plant from.");
                yield break;
            }
            if (Inventory.IsEmpty) {
                command.SendIllegalActionResponse("Bot does not have any plants to plant.");
                yield break;
            }
            if(soilCell.PlantCount >= GLOBAL_CONSTANTS.MAX_PLANT_COUNT_PER_TILE) {
                command.SendIllegalActionResponse("Soil cell already has maximum number of plants.");
                yield break;
            }

            JObject Params = command.ActionParams;
            int target = Params.TryGetDefault("target", -1);
            if(target < 0) {
                target = 0;
            }

            CurrentCommand = command;
            yield return WaitAnimation("plant");
            if (CurrentCommand.Command == PuppetCommandType.STOP) {
                CurrentCommand = PuppetCommand.IDLE;
                yield break;
            }
            
            GameActions.Instance.Plant(this, soilCell, target);
            CurrentCommand = PuppetCommand.IDLE;
        }
        else {
            command.SendIllegalActionResponse("Bot can only plant in soil cells.");
            yield break;
        }
    }

    private IEnumerator Harvest(PuppetCommand command) {
        if(CurrentTile is SoilCellBehavior soilCell) {
            if (!Inventory.HasPlantInventory) {
                command.SendIllegalActionResponse("Bot does not have a plant inventory to harvest into.");
                yield break;
            }
            if(Inventory.IsFull) {
                command.SendIllegalActionResponse("Bot's plant inventory is full.");
                yield break;
            }
            if (soilCell.PlantCount == 0) {
                command.SendIllegalActionResponse("Soil cell does not have any plants to harvest.");
                yield break;
            }
            //need to check for a target param
            JObject Params = command.ActionParams;
            int target = Params.TryGetDefault("target", -1);
            if (target < 0) {
                target = 0;
            }
            if (target >= soilCell.plants.Count) {
                command.SendIllegalActionResponse($"Invalid target plant {target}.");
                yield break;
            }
            CurrentCommand = command;
            yield return WaitAnimation("harvest");
            if (CurrentCommand.Command == PuppetCommandType.STOP) {
                CurrentCommand = PuppetCommand.IDLE;
                yield break;
            }
            GameActions.Instance.Harvest(this, soilCell, target);
            CurrentCommand = PuppetCommand.IDLE;
        }
        else {
            command.SendIllegalActionResponse("Bot can only harvest plants from soil cells.");
            yield break;
        }
    }

    private IEnumerator Till(PuppetCommand command) {
        if(CurrentTile is SoilCellBehavior soilCell) {
            //Till has no legality conditions? 
            //You're allowed to till an empty tile?

            CurrentCommand = command;
            yield return WaitAnimation("till");
            if (CurrentCommand.Command == PuppetCommandType.STOP) {
                CurrentCommand = PuppetCommand.IDLE;
                yield break;
            }
            GameActions.Instance.Till(this, soilCell);
            CurrentCommand = PuppetCommand.IDLE;
        }
        else {
            command.SendIllegalActionResponse("Bot can only till soil cells.");
            yield break;
        }
    }

    private IEnumerator WaitAnimation(string animation) {
        float duration = _botModeConfig.GetActionTime(animation) * GameManager.Instance.secondPerTick;
        yield return WaitAnimation(animation, duration);
    }

    private IEnumerator WaitAnimation(string animation, float duration) {
        _animator.SetTrigger(animation);

        if (GameManager.Instance.secondPerTick > 0) {
            progressBar.SetActive(true);
            progressBarFill.transform.localScale = new Vector3(0, 1, 1);
            float startTime = Time.time;
            while (Time.time - startTime < duration) {
                progressBarFill.transform.localScale = new Vector3((Time.time - startTime) / duration, 1, 1); 
                if (CurrentCommand.Command == PuppetCommandType.IDLE || CurrentCommand.Command == PuppetCommandType.STOP) {
                    progressBar.gameObject.SetActive(false);
                    yield break;
                }
                yield return null;
            }
        }
        progressBar.SetActive(false);
    }

    #endregion

    #region Public Functions

    public void ChangeMode(BotModeSO newBotMode) {
        _botModeConfig = newBotMode;
        hatSprite.sprite = newBotMode.hatSprite;

        //NOTE there is a possibility for plants and or nutrients to be lost here.
        Inventory = Inventory.Resize(newBotMode.reservoirCapacity, newBotMode.plantInventoryCapacity);

        ClearActionSet();
        RegisterStandardActions();
        foreach (var actionEntry in _botModeConfig.supportedActions) {
            switch (actionEntry.action) {
                case "sample":
                    RegisterAction("sample", Sample);
                    break;
                case "spray":
                    RegisterAction("spray", Spray);
                    break;
                case "pick":
                case "pluck":
                    RegisterAction("pluck", Pick);
                    break;
                case "plant":
                    RegisterAction("plant", Plant);
                    break;
                case "harvest":
                    RegisterAction("harvest", Harvest);
                    break;
                case "till":
                    RegisterAction("till", Till);
                    break;
            }
        }
    }



    public void FocusBot(Color baseColor) {
        FocusBot(baseColor, Color.clear);
    }

    /// <summary>
    /// TODO this might need more parameters eventually.
    /// </summary>
    /// <param name="color"></param>
    public void FocusBot(Color baseColor, Color stripeColor) {
        selectionCircleBase.color = baseColor;
        selectionCircleBase.gameObject.SetActive(true);
        selectionCircleStripe.color = stripeColor;
        selectionCircleStripe.gameObject.SetActive(true);
    }

    public void UnfocusBot() {
        selectionCircleBase.gameObject.SetActive(false);
        selectionCircleStripe.gameObject.SetActive(false);
    }

    #endregion

    #region Utility Functions

    protected Floor CurrentFloor {
        get { return GameManager.Instance.parentTower.floors[FloorIdx]; }
    }

    protected bool ValidTargetPosition(Vector2 position) {
        return position.x < CurrentFloor.SizeX && position.y < CurrentFloor.SizeY && position.x >= 0 && position.y >= 0;
    }

    protected GridCellBehavior CurrentTile => CurrentFloor.Cells[CellIdx.x, CellIdx.y];

    #endregion

    #region PuppetBehavior Implementation

    public override HashSet<string> FullActionSet { get; } = new(GLOBAL_CONSTANTS.ACTION_NAMES);

    public override void ExecuteCommunicate(PuppetCommand command) {
        throw new System.NotImplementedException();
    }

    public override JObject GetInfo(PuppetCommand command) {
        JObject ret = base.GetInfo(command);
        ret["mode"] = BotModeConfig.name;
        return ret;
    }


    public override JObject GetState(PuppetCommand command) {
        JObject ret = new JObject();

        ret["info"] = HMTStateRep(HMTStateLevelOfDetail.Full);

        List<JObject> percept = new List<JObject>();
        int xMin = Mathf.Max(0, CellIdx.x - _botModeConfig.sensingRange.x / 2);
        int xMax = Mathf.Min(CurrentFloor.SizeX - 1, CellIdx.x + _botModeConfig.sensingRange.x / 2);
        int yMin = Mathf.Max(0, CellIdx.y - _botModeConfig.sensingRange.y / 2);
        int yMax = Mathf.Min(CurrentFloor.SizeY - 1, CellIdx.y + _botModeConfig.sensingRange.y / 2);
        //Debug.LogFormat("<color=yellow>Bot is At</color> {0}", _botInfo.CellIdx);
        //Debug.LogFormat("<color=cyan>GetState for Tiles</color> ({0}, {1}) to ({2}, {3})", xMin, yMin, xMax, yMax);
        for (int x = xMin; x <= xMax; x++) {
            for (int y = yMin; y <= yMax; y++) {
                percept.Add(CurrentFloor.Cells[x, y].HMTStateRep(HMTStateLevelOfDetail.Visible));
            }
        }
        ret["percept"] = new JArray(percept);

        //TODO: we could add "communications" or something as well as a flag in the GetState command for additional details
        return ret;
    }

    public override JObject HMTStateRep(HMTStateLevelOfDetail level) {
        JObject resp = new JObject();
        switch (level) {
            case HMTStateLevelOfDetail.Full:

                resp["current_actions"] = new JArray(CurrentActionSet);
                resp["full_actions"] = new JArray(FullActionSet);
                resp["command_queue"] = new JArray(_currentQueue.Select(c => c.HMTStateRep()));
                resp["sensing_range_x"] = _botModeConfig.sensingRange.x;
                resp["sensing_range_y"] = _botModeConfig.sensingRange.y;
                resp["reservoir"] = Inventory.ReservoirInventory.ToFlatJSON();
                resp["inventory"] = new JArray(Inventory.PlantInventory.Select(p => p.HMTStateRep(HMTStateLevelOfDetail.Visible)));
                goto case HMTStateLevelOfDetail.Visible;

            case HMTStateLevelOfDetail.Visible:
                resp["puppet_id"] = PuppetID;
                resp["x"] = CellIdx.x;
                resp["y"] = CellIdx.y;
                resp["floor"] = FloorIdx;
                resp["mode"] = BotModeConfig.name;
                if (CurrentCommand.Command != PuppetCommandType.IDLE) {
                    resp["current_command"] = CurrentCommand.HMTStateRep();
                }
                else {
                    resp["current_command"] = null;
                }
                goto case HMTStateLevelOfDetail.Seen;

            case HMTStateLevelOfDetail.Seen:
            case HMTStateLevelOfDetail.Unseen:
            case HMTStateLevelOfDetail.None:
            default:
                break;
        }
        return resp;
    }

    #endregion

    #region PrefabPool Functions
    public void OnInstantiateFromPool() {
        //feels weird to call Start but seems fine?
        Start();
    }

    public void OnReleaseToPool() {
        HMTPuppetManager.Instance.RemovePuppet(this);
        StopAllCoroutines();
        ClearActionSet();
        _currentQueue.Clear();
        CurrentCommand = PuppetCommand.IDLE;
        Inventory = BotInventory.None;
        UnfocusBot();
        _animator.SetTrigger("idle");
    }


    #endregion
}
