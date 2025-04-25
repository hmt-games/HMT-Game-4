using System.Collections;
using System.Collections.Generic;
using HMT.Puppetry;
using Newtonsoft.Json.Linq;
using GameConstant;
using UnityEngine;
using WebSocketSharp;

public class DefaultPuppetBot : PuppetBehavior
{
    protected struct BotInfo {
        public int FloorIdx;
        public Vector2Int CellIdx;
        public BotMode CurrentBotMode;
    }

    /// <summary>
    /// TODO: This needs to be put in some kind of config
    /// </summary>
    public int SensorRange = 1;

    private BotInfo _botInfo;
    private bool _walking = false;
    private Vector3 _targetPos = Vector3.zero;

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
        //_botInfo.X = x;
        //_botInfo.Y = y;
    }


    Floor CurrentFloor {
        get { return GameManager.Instance.parentTower.floors[_botInfo.FloorIdx]; }
    }

    public override HashSet<string> SupportedActions
    {
        get =>
            new()
            {
                "pick", "harvest", "spray", "plant",
                "sample", "move", "moveto"
            };
        protected set => throw new System.NotImplementedException();
    }

    public override void ExecuteAction(PuppetCommand command)
    {
        //CurrentCommand = command;
        Debug.LogFormat("Default Puppet Bot Execute Action:{0}", command.json.ToString());
        switch (command.Action)
        {
            case "move":
                Move(command);
                break;
            default:
                command.SendIllegalActionResponse();
                break;
        }
    }

    protected virtual void Update()
    {
        //if (_walking)
        //{
        //    transform.position = Vector3.MoveTowards(transform.position, _targetPos, GameManager.Instance.secondPerTick * Time.deltaTime);
        //    if (transform.position == _targetPos)
        //    {
        //        _walking = false;
        //        CurrentCommand = null;
        //    }
        //    return;
        //}
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



        //switch (direction)
        //{
        //    case "up":
        //        if (ValidTargetPosition(_botInfo.X, _botInfo.Y + 1))
        //        {
        //            _targetPos = CurrentFloor.Cells[_botInfo.X, _botInfo.Y + 1].transform.position;
        //            _botInfo.Y += 1;
        //        }
        //        break;
        //    case "down":
        //        if (ValidTargetPosition(_botInfo.X, _botInfo.Y - 1))
        //        {
        //            _targetPos = CurrentFloor.Cells[_botInfo.X, _botInfo.Y - 1].transform.position;
        //            _botInfo.Y -= 1;
        //        }
        //        break;
        //    case "left":
        //        if (ValidTargetPosition(_botInfo.X - 1, _botInfo.Y))
        //        {
        //            _targetPos = CurrentFloor.Cells[_botInfo.X - 1, _botInfo.Y].transform.position;
        //            _botInfo.X -= 1;
        //        }
        //        break;
        //    case "right":
        //        if (ValidTargetPosition(_botInfo.X + 1, _botInfo.Y))
        //        {
        //            _targetPos = CurrentFloor.Cells[_botInfo.X + 1, _botInfo.Y].transform.position;
        //            _botInfo.X += 1;
        //        }
        //        break;
        //    default:
        //        CurrentCommand.SendBadParametersResponse(new JObject
        //        {
        //            {"direction", direction}
        //        }, new JObject
        //        {
        //            "direction", new JArray{"up", "down", "left", "right"}
        //        });
        //        CurrentCommand = null;
        //        break;
        //}
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

    private bool ValidTargetPosition(Vector2 position)
    {

        return position.x < CurrentFloor.SizeX && position.y < CurrentFloor.SizeY && position.x >= 0 && position.y >= 0;
        //if (ret) _walking = true;
        //else CurrentCommand = null;
        //return ret;
    }
}
