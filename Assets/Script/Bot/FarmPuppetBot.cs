using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HMT.Puppetry;
using Newtonsoft.Json.Linq;
using GameConstant;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using GameConfig;

public class FarmPuppetBot : PuppetBehavior
{
    #region Internal States

    protected struct BotInfo {
        public int FloorIdx;
        public Vector2Int CellIdx;
        public BotMode CurrentBotMode;
    }

    protected BotInfo _botInfo;

    protected bool _walking = false;
    protected Vector3 _targetPos = Vector3.zero;
    
    [SerializeField] protected Slider progressBar;
    
    protected Animator _animator;

    #endregion

    #region External States

    public float reservoirCapacity = 100.0f;
    public int plantInventoryCapacity = 8;
    public List<PlantBehavior> plantInventory;
    public NutrientSolution reservoirInventory;
    
    #endregion

    #region Bot Init

    public void InitBot(int floor, int x, int y, int sensorRange = 1)
    {
        _botInfo.FloorIdx = floor;
        _botInfo.CellIdx = new Vector2Int(x, y);
        //CurrentFloor.AddBotToFloor(this);
    }
    
    protected void Awake()
    {
        puppetIDPrefix = "farm_bot";
        _animator = GetComponent<Animator>();
        progressBar.gameObject.SetActive(false);
        
        plantInventory = new List<PlantBehavior>();
        reservoirInventory = NutrientSolution.Empty;

        _botModeConfig = GameManager.Instance.DefaultBotMode;

        RegisterAction("move", Move);
        RegisterAction("interact", Interact);

        RegisterAction("harvest", ActionNotImplemented);
        RegisterAction("move_to", ActionNotImplemented);
        RegisterAction("pick_up", ActionNotImplemented);
        RegisterAction("put_down", ActionNotImplemented);
        // RegisterAction("till", ActionNotImplemented);

    }

    #endregion

    public override HashSet<string> FullActionSet { get; } = new()
        {
            "move", "move_to", "interact", "sample", "spray", "harvest", "pluck", "pick_up", "put_down", "plant", "till"
        };

    private BotModeSO _botModeConfig;

    #region Bot Action Implementation
    
    private void SwitchBotMode()
    {
        StationCellBehavior station = GetCurrentTile() as StationCellBehavior;
        BotModeSO newBotMode = station.Interact(this);
        if (newBotMode != null)
        {
            _botModeConfig = newBotMode;
            //CurrentActionSet = new HashSet<string>(_botModeConfig.supportedActions);
            _animator.SetTrigger(_botModeConfig.botModeName);
            _botInfo.CurrentBotMode = _botModeConfig.botMode;
            
            // reset inventory
            SetBotInventory(_botModeConfig.reservoirCapacity, _botModeConfig.plantInventoryCapacity);
            ClearActionSet();
            RegisterAction("move", Move);
            RegisterAction("interact", Interact);
            RegisterAction("move_to", ActionNotImplemented);
            RegisterAction("pick_up", ActionNotImplemented);
            RegisterAction("put_down", ActionNotImplemented);

            foreach (string action in _botModeConfig.supportedActions) {
                switch (action) {
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
                        RegisterAction("harvest", ActionNotImplemented);
                        break;
                    case "till":
                        RegisterAction("till", Till);
                        break;
                }
            }

        }
    }

    private IEnumerator Till(PuppetCommand command)
    {
        GridCellBehavior grid = GetCurrentTile();
        if (grid.tileType != TileType.Soil)
        {
            command.SendIllegalActionResponse("till can only target soil tile");
            yield break;
        }
        
        CurrentCommand = command;
        float actionTime = ActionTickTimeCost.Till * GameManager.Instance.secondPerTick;
        yield return StartProgressTimer(actionTime);

        if (CurrentCommand.Command == PuppetCommandType.STOP) {
            CurrentCommand = PuppetCommand.IDLE;
            yield break;
        }

        GameActions.Instance.Till(grid as SoilCellBehavior, this);
        CurrentCommand = PuppetCommand.IDLE;
    }

    private void Plant(PuppetCommand command)
    {
        GridCellBehavior grid = GetCurrentTile();
        if (grid.tileType != TileType.Soil)
        {
            command.SendIllegalActionResponse("plant can only target soil tile");
            return;
        }

        if ((grid as SoilCellBehavior).plantCount >= GLOBAL_CONSTANTS.MAX_PLANT_COUNT_PER_TILE)
        {
            command.SendIllegalActionResponse("tile is at max capacity");
            return;
        }
        
        if (!GameActions.Instance.RequestPlant(this))
        {
            command.SendIllegalActionResponse("none of the plant in inventory is seed");
        }
        else
        {
            CurrentCommand = command;
        }
    }

    public IEnumerator StartPlant(PlantBehavior plant)
    {
        float actionTime = ActionTickTimeCost.PlantPerStage * GameManager.Instance.secondPerTick;
        yield return StartProgressTimer(actionTime);
        if(CurrentCommand.Command == PuppetCommandType.IDLE || CurrentCommand.Command == PuppetCommandType.STOP) {
            CurrentCommand = PuppetCommand.IDLE;
            yield break;
        }

        GameActions.Instance.Plant(GetCurrentTile() as SoilCellBehavior, plant, this);
        CurrentCommand = PuppetCommand.IDLE;
    }

    private void Pick(PuppetCommand command)
    {
        GridCellBehavior grid = GetCurrentTile();
        if (grid.tileType != TileType.Soil)
        {
            command.SendIllegalActionResponse("harvest can only target soil tile");
            return;
        }

        // for human players
        if (command.ActionParams == null)
        {
            if (!GameActions.Instance.RequestPick(grid as SoilCellBehavior, this))
            {
                command.SendIllegalActionResponse("none of the target tile's plants has fruit");
            }
            else
            {
                CurrentCommand = command;
            }
        }
        //for bots
        else
        {
            SoilCellBehavior tile = grid as SoilCellBehavior;
            int plantIdx = (int)command.ActionParams["target"];
            
            if (plantIdx > tile.plantCount - 1)
            {
                command.SendIllegalActionResponse("Target Index for pluck out of bound of tile plant list");
                return;
            }
            
            PlantBehavior plant = tile.plants[plantIdx];
            if (!plant.hasFruit)
            {
                command.SendIllegalActionResponse("Target plant does not bear fruit");
                return;
            }

            CurrentCommand = command;
            StartCoroutine(StartPick(plant));
        }
    }

    public IEnumerator StartPick(PlantBehavior plant)
    {
        float actionTime = ActionTickTimeCost.Harvest * GameManager.Instance.secondPerTick;
        yield return StartProgressTimer(actionTime);
        if (CurrentCommand.Command == PuppetCommandType.STOP) {
            CurrentCommand = PuppetCommand.IDLE;
            yield break;
        }
        
        GameActions.Instance.Pick(plant, this);
        CurrentCommand = PuppetCommand.IDLE;
    }

    private void Interact(PuppetCommand command)
    {
        TileType tileType = GetCurrentTile().tileType;
        if (tileType == TileType.Soil)
        {
            command.SendIllegalActionResponse("Cannot perform interact on soil tiles");
            return;
        }

        CurrentCommand = command;

        SwitchBotMode();

        if (tileType != TileType.SprayAStation && tileType != TileType.SprayBStation
            && tileType != TileType.SprayCStation && tileType != TileType.SprayDStation)
            CurrentCommand = PuppetCommand.IDLE;
    }

    private void SetBotInventory(float _reservoirCapacity, int _plantInventoryCapacity)
    {
        reservoirCapacity = _reservoirCapacity;
        plantInventoryCapacity = _plantInventoryCapacity;

        reservoirInventory = reservoirCapacity == 0 ? NutrientSolution.Empty : reservoirInventory.DrawOff(reservoirCapacity);
        plantInventory = plantInventoryCapacity == 0
            ? new List<PlantBehavior>()
            : plantInventory.Take(plantInventoryCapacity).ToList();
    }

    public IEnumerator SprayUp()
    {
        float actionTime = ActionTickTimeCost.SprayUp * GameManager.Instance.secondPerTick;
        yield return StartProgressTimer(actionTime);

        if (CurrentCommand.Command == PuppetCommandType.STOP) {
            CurrentCommand = PuppetCommand.IDLE;
            yield break;
        }
        
        GameActions.Instance.SprayUp(GetCurrentTile().GetComponent<StationCellBehavior>(), this);
        CurrentCommand = PuppetCommand.IDLE;
    }

    private IEnumerator Spray(PuppetCommand command)
    {
        GridCellBehavior grid = GetCurrentTile();
        if (grid.tileType != TileType.Soil)
        {
            command.SendIllegalActionResponse("can only spray to soil tile");
            yield break;
        }
        
        CurrentCommand = command;
        float actionTime = ActionTickTimeCost.Spray * GameManager.Instance.secondPerTick;
        yield return StartProgressTimer(actionTime);

        if (CurrentCommand.Command == PuppetCommandType.STOP) {
            CurrentCommand = PuppetCommand.IDLE;
            yield break;
        }

        GameActions.Instance.Spray(grid as SoilCellBehavior, this);
        CurrentCommand = PuppetCommand.IDLE;
    }

    private IEnumerator Sample(PuppetCommand command)
    {
        if (_botInfo.CurrentBotMode != BotMode.Sample)
        {
            command.SendIllegalActionResponse("bot is not sample bot or sample not supported");
            yield break;
        }

        if (reservoirInventory != NutrientSolution.Empty)
        {
            command.SendIllegalActionResponse("water inventory must be empty before sample");
            yield break;
        }

        GridCellBehavior grid = GetCurrentTile();
        if (grid.tileType != TileType.Soil)
        {
            command.SendIllegalActionResponse("must sample on a soil grid");
            yield break;
        }
        
        CurrentCommand = command;
        float actionTime = ActionTickTimeCost.Sample * GameManager.Instance.secondPerTick;
        yield return StartProgressTimer(actionTime);
        if (CurrentCommand.Command == PuppetCommandType.STOP) {
            CurrentCommand = PuppetCommand.IDLE;
            yield break;
        }

        GameActions.Instance.Sample(grid as SoilCellBehavior, this);
        CurrentCommand = PuppetCommand.IDLE;
    }

    public void DumpInventory()
    {
        plantInventory = new List<PlantBehavior>();
        reservoirInventory = NutrientSolution.Empty;
        //TODO: refresh the inventory UI here
    }
    
    private void Move(PuppetCommand command) {
        if (_walking) {
            command.SendIllegalActionResponse("Bot is already moving");
            return;
        }
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

        if(!ValidTargetPosition(direct + _botInfo.CellIdx)) {
            command.SendIllegalActionResponse("Attempting to move bot out of bounds");
            return;
        }

        CurrentCommand = command;
        StartCoroutine(MoveCoroutine(direct));
    }

    IEnumerator MoveCoroutine(Vector2Int direction) {
        _walking = true;
        Vector3 target = CurrentFloor.Cells[_botInfo.CellIdx.x + direction.x, _botInfo.CellIdx.y + direction.y].transform.position;
        if (GameManager.Instance.secondPerTick > 0) {
            float moveDuration = Vector3.Distance(transform.position, target) * GameManager.Instance.secondPerTick / (_botModeConfig == null ? 1.0f : _botModeConfig.movementSpeed);
            float startTime = Time.time;
            while (Time.time - startTime < moveDuration) {
                transform.position = Vector3.Lerp(transform.position, target, (Time.time - startTime) / moveDuration);
                yield return null;
            }            
        }
        _botInfo.CellIdx += direction;
        transform.position = target;

        CurrentCommand = PuppetCommand.IDLE;
        _walking = false;
    }

    #endregion

    public override void ExecuteCommunicate(PuppetCommand command)
    {
        throw new System.NotImplementedException();
    }

    #region HMT State

    public override JObject GetInfo(PuppetCommand command) {
        JObject ret = base.GetInfo(command);
        ret["mode"] = _botInfo.CurrentBotMode.ToString();
        return ret;
    }

    public override JObject GetState(PuppetCommand command)
    {
        JObject ret = new JObject();

        ret["info"] = HMTStateRep(HMTStateLevelOfDetail.Full);
        
        List<JObject> percept = new List<JObject>();
        int xMin = Mathf.Max(0, _botInfo.CellIdx.x - _botModeConfig.sensingRange.x / 2);
        int xMax = Mathf.Min(CurrentFloor.SizeX - 1, _botInfo.CellIdx.x + _botModeConfig.sensingRange.x / 2);
        int yMin = Mathf.Max(0, _botInfo.CellIdx.y - _botModeConfig.sensingRange.y / 2);
        int yMax = Mathf.Min(CurrentFloor.SizeY - 1, _botInfo.CellIdx.y + _botModeConfig.sensingRange.y / 2);
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

    public override JObject HMTStateRep(HMTStateLevelOfDetail level)
    {
        JObject resp = new JObject();
        switch (level) {
            case HMTStateLevelOfDetail.Full:
                
                resp["current_actions"] = new JArray(CurrentActionSet);
                resp["full_actions"] = new JArray(FullActionSet);
                resp["command_queue"] = new JArray(_currentQueue.Select(c => c.HMTStateRep()));
                resp["sensing_range_x"] = _botModeConfig.sensingRange.x;
                resp["sensing_range_y"] = _botModeConfig.sensingRange.y;
                resp["reservoir"] = reservoirInventory.ToFlatJSON();
                resp["inventory"] = new JArray(plantInventory.Select(p => p.HMTStateRep(HMTStateLevelOfDetail.Visible)));
                goto case HMTStateLevelOfDetail.Visible;

            case HMTStateLevelOfDetail.Visible:
                resp["puppet_id"] = PuppetID;
                resp["x"] = _botInfo.CellIdx.x;
                resp["y"] = _botInfo.CellIdx.y;
                resp["floor"] = _botInfo.FloorIdx;
                resp["mode"] = _botInfo.CurrentBotMode.ToString();
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

    #region Helper Functions

    protected Floor CurrentFloor {
        get { return GameManager.Instance.parentTower.floors[_botInfo.FloorIdx]; }
    }

    protected bool ValidTargetPosition(Vector2 position)
    {
        return position.x < CurrentFloor.SizeX && position.y < CurrentFloor.SizeY && position.x >= 0 && position.y >= 0;
    }
    
    protected GridCellBehavior GetCurrentTile()
    {
        return CurrentFloor.Cells[_botInfo.CellIdx.x, _botInfo.CellIdx.y];
    }

    #endregion
    
    protected IEnumerator StartProgressTimer(float time)
    {
        progressBar.value = 0;
        progressBar.gameObject.SetActive(true);

        if (GameManager.Instance.secondPerTick > 0) {
            float startTime = Time.time;
            while (Time.time - startTime < time) {
                progressBar.value = Mathf.Clamp01((Time.time - startTime) / time);
                if (CurrentCommand.Command == PuppetCommandType.IDLE || CurrentCommand.Command == PuppetCommandType.STOP) {
                    progressBar.gameObject.SetActive(false);
                    yield break;
                }
                yield return null;
            }
        }
        progressBar.gameObject.SetActive(false);
    }
}
