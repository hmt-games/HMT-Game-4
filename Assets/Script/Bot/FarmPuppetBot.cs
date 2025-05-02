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
    protected bool _actionProgressing = false;
    
    protected Animator _animator;

    #endregion

    #region External States

    public float reservoirCapacity = 100.0f;
    public int plantInventoryCapacity = 8;
    public List<PlantBehavior> plantInventory;
    public NutrientSolution reservoirInventory;
    
    /// TODO: This needs to be put in some kind of config
    public int SensorRange = 1;

    #endregion

    #region Bot Init

    public void InitBot(int floor, int x, int y, int sensorRange = 1)
    {
        _botInfo.FloorIdx = floor;
        _botInfo.CellIdx = new Vector2Int(x, y);
        SensorRange = sensorRange;
    }
    
    protected void Awake()
    {
        _animator = GetComponent<Animator>();
        progressBar.gameObject.SetActive(false);
        
        plantInventory = new List<PlantBehavior>();
        reservoirInventory = NutrientSolution.Empty;
    }

    #endregion

    public override HashSet<string> SupportedActions { get; protected set; }
        = new() { "move", "moveto", "useStation" };

    private void ChangeActionSet(HashSet<string> newActions) => SupportedActions = newActions;

    private BotModeSO _botModeConfig;
    
    public override void ExecuteAction(PuppetCommand command)
    {
        if (!SupportedActions.Contains(command.Action))
        {
            command.SendIllegalActionResponse($"Action {command.Action} not supported by the bot currently");
            return;
        }
        
        switch (command.Action)
        {
            case "move":
                Move(command);
                break;
            case "useStation":
                UseStation(command);
                break;
            case "sample":
                StartCoroutine(Sample(command));
                break;
            case "spray":
                StartCoroutine(Spray(command));
                break;
            case "pick":
                Pick(command);
                break;
            case "plant":
                Plant(command);
                break;
            default:
                command.SendIllegalActionResponse();
                Debug.LogWarning("execute action hit a unimplemented case");
                break;
        }
    }

    #region Bot Action Implementation

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
        StartCoroutine(StartProgressTimer(actionTime));
        
        while (_actionProgressing)
        {
            yield return null;
        }
        
        GameActions.Instance.Plant(GetCurrentTile() as SoilCellBehavior, plant, this);
        CurrentCommand = null;
    }

    private void Pick(PuppetCommand command)
    {
        GridCellBehavior grid = GetCurrentTile();
        if (grid.tileType != TileType.Soil)
        {
            command.SendIllegalActionResponse("harvest can only target soil tile");
            return;
        }

        if (!GameActions.Instance.RequestPick(grid as SoilCellBehavior, this))
        {
            command.SendIllegalActionResponse("none of the target tile's plants has fruit");
        }
        else
        {
            CurrentCommand = command;
        }
    }

    public IEnumerator StartPick(PlantBehavior plant)
    {
        float actionTime = ActionTickTimeCost.Harvest * GameManager.Instance.secondPerTick;
        StartCoroutine(StartProgressTimer(actionTime));
        
        while (_actionProgressing)
        {
            yield return null;
        }
        
        GameActions.Instance.Pick(plant, this);
        CurrentCommand = null;
    }

    private void UseStation(PuppetCommand command)
    {
        TileType tileType = GetCurrentTile().tileType;
        if (tileType == TileType.Soil)
        {
            command.SendIllegalActionResponse("Cannot perform useStation on soil tiles");
            return;
        }

        CurrentCommand = command;

        SwitchBotMode();

        if (tileType != TileType.SprayAStation && tileType != TileType.SprayBStation
            && tileType != TileType.SprayCStation && tileType != TileType.SprayDStation)
            CurrentCommand = null;
    }

    private void SwitchBotMode()
    {
        StationCellBehavior station = GetCurrentTile() as StationCellBehavior;
        BotModeSO newBotMode = station.UseStation(this);
        if (newBotMode != null)
        {
            _botModeConfig = newBotMode;
            SupportedActions = new HashSet<string>(_botModeConfig.supportedActions);
            _animator.SetTrigger(_botModeConfig.botModeName);
            _botInfo.CurrentBotMode = _botModeConfig.botMode;
            
            // reset inventory
            SetBotInventory(_botModeConfig.reservoirCapacity, _botModeConfig.plantInventoryCapacity);
        }
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
        StartCoroutine(StartProgressTimer(actionTime));
        
        while (_actionProgressing)
        {
            yield return null;
        }
        
        GameActions.Instance.SprayUp(GetCurrentTile().GetComponent<StationCellBehavior>(), this);
        CurrentCommand = null;
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
        StartCoroutine(StartProgressTimer(actionTime));
        
        while (_actionProgressing)
        {
            yield return null;
        }

        GameActions.Instance.Spray(grid as SoilCellBehavior, this);
        CurrentCommand = null;
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
        StartCoroutine(StartProgressTimer(actionTime));
        
        while (_actionProgressing)
        {
            yield return null;
        }

        CurrentCommand = command;
        GameActions.Instance.Sample(grid as SoilCellBehavior, this);
        CurrentCommand = null;
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
        JObject Params = command.Params;
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
            
            _botInfo.CellIdx += direction;
            transform.position = target;
        }
        
        CurrentCommand = null;
        _walking = false;
    }

    #endregion

    public override void ExecuteCommunicate(PuppetCommand command)
    {
        throw new System.NotImplementedException();
    }

    #region HMT State

    public override JObject GetState(PuppetCommand command)
    {
        JObject ret = new JObject();

        ret["info"] = HMTStateRep(HMTStateLevelOfDetail.Full);
        
        List<JObject> percept = new List<JObject>();
        int xMin = Mathf.Max(0, _botInfo.CellIdx.x - SensorRange);
        int xMax = Mathf.Min(CurrentFloor.SizeX - 1, _botInfo.CellIdx.x + SensorRange);
        int yMin = Mathf.Max(0, _botInfo.CellIdx.y - SensorRange);
        int yMax = Mathf.Min(CurrentFloor.SizeY - 1, _botInfo.CellIdx.y + SensorRange);
        Debug.LogFormat("<color=yellow>Bot is At</color> {0}", _botInfo.CellIdx);
        Debug.LogFormat("<color=cyan>GetState for Tiles</color> ({0}, {1}) to ({2}, {3})", xMin, yMin, xMax, yMax);
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
                resp["actions"] = new JArray(SupportedActions);
                goto case HMTStateLevelOfDetail.Visible;

            case HMTStateLevelOfDetail.Visible:
                resp["x"] = _botInfo.CellIdx.x;
                resp["y"] = _botInfo.CellIdx.x;
                resp["floor"] = _botInfo.FloorIdx;
                resp["mode"] = _botInfo.CurrentBotMode.ToString();
                if (CurrentCommand != null) {
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
        _actionProgressing = true;

        float timeElapsed = 0.0f;
        while (timeElapsed < time)
        {
            yield return null;
            timeElapsed += Time.deltaTime;
            progressBar.value = Mathf.Clamp01(timeElapsed / time);
        }

        _actionProgressing = false;
        progressBar.gameObject.SetActive(false);
    }
}
