using System;
using System.Collections;
using System.Collections.Generic;
using HMT.Puppetry;
using Newtonsoft.Json.Linq;
using GameConstant;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class PlayerPuppetBot : PuppetBehavior
{
    [SerializeField] private Slider progressBar;
    private bool _actionProgressing = false;
    
    protected struct BotInfo {
        public int FloorIdx;
        public Vector2Int CellIdx;
        public BotType CurrentBotType;
    }

    /// <summary>
    /// TODO: This needs to be put in some kind of config
    /// </summary>
    public int SensorRange = 1;

    private BotInfo _botInfo;
    private bool _walking = false;
    private Vector3 _targetPos = Vector3.zero;
    
    private Dictionary<KeyCode, string> _keyCode2MoveParams = new Dictionary<KeyCode, string>
    {
        { KeyCode.A, "left" },
        { KeyCode.D, "right" },
        { KeyCode.W, "up" },
        { KeyCode.S, "down" }
    };

    private Animator _animator;

    /// <summary>
    /// init bot
    /// </summary>
    /// <param name="floor"> floor the bot is on </param>
    /// <param name="x"> spawn x location of bot </param>
    /// <param name="y"> spawn x location of bot </param>
    public void InitBot(int floor, int x, int y)
    {
        _botInfo.FloorIdx = floor;
        _botInfo.CellIdx = new Vector2Int(x, y);
        SensorRange = 1;
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        progressBar.gameObject.SetActive(false);
    }

    Floor CurrentFloor {
        get { return GameManager.Instance.parentTower.floors[_botInfo.FloorIdx]; }
    }

    public override HashSet<string> SupportedActions =>
        new()
        {
            "pick", "harvest", "spray", "plant",
            "sample", "move", "useStation"
        };

    public override void ExecuteAction(PuppetCommand command)
    {
        //Debug.LogFormat("Default Puppet Bot Execute Action:{0}", command.json.ToString());
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
            default:
                command.SendIllegalActionResponse();
                break;
        }
    }

    protected virtual void Update()
    {
        MoveInput();
        UseStationInput();
        PerformBotAction();
    }

    #region Player Input

    // human player movement input
    private void MoveInput()
    {
        foreach (var kvp in _keyCode2MoveParams)
        {
            if (Input.GetKeyDown(kvp.Key))
            {
                JObject moveParams = new JObject { { "direction", kvp.Value } };
                PuppetCommand moveCmd = new PuppetCommand(PuppetID, "move", moveParams);
                HMTPuppetManager.Instance.EnqueueCommand(moveCmd);
            }
        }
    }

    private void UseStationInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PuppetCommand cmd = new PuppetCommand(PuppetID, "useStation");
            HMTPuppetManager.Instance.EnqueueCommand(cmd);
        }
    }

    private void PerformBotAction()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            switch (_botInfo.CurrentBotType)
            {
                case BotType.Sample:
                    PuppetCommand cmd = new PuppetCommand(PuppetID, "sample");
                    HMTPuppetManager.Instance.EnqueueCommand(cmd);
                    break;
            }
        }
    }

    #endregion

    private IEnumerator StartProgressTimer(float time)
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

    private void UseStation(PuppetCommand command)
    {
        TileType tileType = CurrentFloor.Cells[_botInfo.CellIdx.x, _botInfo.CellIdx.y].tileType;
        if (tileType == TileType.Soil)
        {
            command.SendIllegalActionResponse("Cannot perform useStation on soil tiles");
            return;
        }

        CurrentCommand = command;
        switch (tileType)
        {
            case TileType.HarvestStation:
                _animator.SetTrigger("TransHarvest");
                _botInfo.CurrentBotType = BotType.Harvest;
                break;
            case TileType.PluckStation:
                _animator.SetTrigger("TransPluck");
                _botInfo.CurrentBotType = BotType.Pluck;
                break;
            case TileType.TillStation:
                _animator.SetTrigger("TransTill");
                _botInfo.CurrentBotType = BotType.Till;
                break;
            case TileType.SprayStation:
                _animator.SetTrigger("TransSpray");
                _botInfo.CurrentBotType = BotType.Spray;
                break;
            case TileType.SampleStation:
                _animator.SetTrigger("TransSample");
                _botInfo.CurrentBotType = BotType.Sample;
                break;
            case TileType.PlantStation:
                _animator.SetTrigger("TransPlant");
                _botInfo.CurrentBotType = BotType.Plant;
                break;
            case TileType.DiscardStation:
                DumpInventory();
                break;
        }

        CurrentCommand = null;
    }

    private IEnumerator Sample(PuppetCommand command)
    {
        CurrentCommand = command;
        float actionTime = ActionTickTimeCost.Sample * GameManager.Instance.secondPerTick;
        StartCoroutine(StartProgressTimer(actionTime));
        
        while (_actionProgressing)
        {
            yield return null;
        }
        
        if (_botInfo.CurrentBotType != BotType.Sample || !SupportedActions.Contains("sample"))
        {
            command.SendIllegalActionResponse("bot is not sample bot or sample not supported");
            CurrentCommand = null;
            yield break;
        }

        if (WaterInventory != NutrientSolution.Empty)
        {
            command.SendIllegalActionResponse("water inventory must be empty before sample");
            CurrentCommand = null;
            yield break;
        }

        GridCellBehavior grid = CurrentFloor.Cells[_botInfo.CellIdx.x, _botInfo.CellIdx.y];
        if (grid.tileType != TileType.Soil)
        {
            command.SendIllegalActionResponse("must sample on a soil grid");
            CurrentCommand = null;
            yield break;
        }

        CurrentCommand = command;
        GameActions.Instance.Sample(grid as SoilCellBehavior, this);
        CurrentCommand = null;
    }

    private void DumpInventory()
    {
        PlantInventory = new List<PlantBehavior>();
        WaterInventory = NutrientSolution.Empty;
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
            command.SendIllegalActionResponse("Attmepting to move bot out of bounds");
            return;
        }

        CurrentCommand = command;
        StartCoroutine(MoveCoroutine(direct));
    }

    IEnumerator MoveCoroutine(Vector2Int direction) {
        _walking = true;
        Vector3 target = CurrentFloor.Cells[_botInfo.CellIdx.x + direction.x, _botInfo.CellIdx.y + direction.y].transform.position;
        if (GameManager.Instance.secondPerTick > 0) {
            float moveDuration = Vector3.Distance(transform.position, target) / GameManager.Instance.secondPerTick;
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


    public override void ExecuteCommunicate(PuppetCommand command)
    {
        throw new System.NotImplementedException();
    }

    public override JObject GetState(PuppetCommand command) {
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

    public override JObject HMTStateRep(HMTStateLevelOfDetail level) {
        JObject resp = new JObject();
        switch (level) {
            case HMTStateLevelOfDetail.Full:
                resp["actions"] = new JArray(SupportedActions);
                goto case HMTStateLevelOfDetail.Visible;

            case HMTStateLevelOfDetail.Visible:
                resp["x"] = _botInfo.CellIdx.x;
                resp["y"] = _botInfo.CellIdx.x;
                resp["floor"] = _botInfo.FloorIdx;
                resp["mode"] = _botInfo.CurrentBotType.ToString();
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

    private bool ValidTargetPosition(Vector2 position)
    {

        return position.x < CurrentFloor.SizeX && position.y < CurrentFloor.SizeY && position.x >= 0 && position.y >= 0;
        //if (ret) _walking = true;
        //else CurrentCommand = null;
        //return ret;
    }
}
