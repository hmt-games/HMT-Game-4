using System.Collections;
using System.Collections.Generic;
using HMT.Puppetry;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WebSocketSharp;

public class DefaultPuppetBot : PuppetBehavior
{
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
        _botInfo.X = x;
        _botInfo.Y = y;
        _botInfo.CellsOnFloor = GameManager.Instance.parentTower.floors[floor].Cells;
        _botInfo.MaxX = _botInfo.CellsOnFloor.GetLength(0);
        _botInfo.MaxY = _botInfo.CellsOnFloor.GetLength(1);
    }

    public override HashSet<string> SupportedActions =>
        new()
        {
            "pick", "harvest", "spray", "plant",
            "sample", "move", "moveto"
        };

    public override void ExecuteAction(PuppetCommand command)
    {
        CurrentCommand = command;
        Debug.LogFormat("Default Puppet Bot Execute Action:{0}", command.json.ToString());
        switch (command.Action)
        {
            case "move":
                Move();
                break;
        }
    }

    protected virtual void Update()
    {
        if (_walking)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, GameManager.Instance.secondPerTick * Time.deltaTime);
            if (transform.position == _targetPos)
            {
                _walking = false;
                CurrentCommand = null;
            }
            return;
        }
    }

    private void Move()
    {
        JObject Params = CurrentCommand.Params;
        string direction = (string)Params["direction"];
        if (direction.IsNullOrEmpty())
        {
            CurrentCommand.SendMissingParametersResponse(new JObject
            {
                {"direction", new JArray{"up", "down", "left", "right"}}
            });

            CurrentCommand = null;
            return;
        }

        switch (direction)
        {
            case "up":
                if (ValidTargetPosition(_botInfo.X, _botInfo.Y + 1))
                {
                    _targetPos = _botInfo.CellsOnFloor[_botInfo.X, _botInfo.Y + 1].transform.position;
                    _botInfo.Y += 1;
                }
                break;
            case "down":
                if (ValidTargetPosition(_botInfo.X, _botInfo.Y - 1))
                {
                    _targetPos = _botInfo.CellsOnFloor[_botInfo.X, _botInfo.Y - 1].transform.position;
                    _botInfo.Y -= 1;
                }
                break;
            case "left":
                if (ValidTargetPosition(_botInfo.X - 1, _botInfo.Y))
                {
                    _targetPos = _botInfo.CellsOnFloor[_botInfo.X - 1, _botInfo.Y].transform.position;
                    _botInfo.X -= 1;
                }
                break;
            case "right":
                if (ValidTargetPosition(_botInfo.X + 1, _botInfo.Y))
                {
                    _targetPos = _botInfo.CellsOnFloor[_botInfo.X + 1, _botInfo.Y].transform.position;
                    _botInfo.X += 1;
                }
                break;
            default:
                CurrentCommand.SendBadParametersResponse(new JObject
                {
                    {"direction", direction}
                }, new JObject
                {
                    "direction", new JArray{"up", "down", "left", "right"}
                });
                CurrentCommand = null;
                break;
        }
    }

    public override void ExecuteCommunicate(PuppetCommand command)
    {
        throw new System.NotImplementedException();
    }

    public override JObject GetState(PuppetCommand command)
    {
        return new JObject();
    }
    
    private bool ValidTargetPosition(int x, int y)
    {
        bool ret = x < _botInfo.MaxX && y < _botInfo.MaxY && x >= 0 && y >= 0;
        if (ret) _walking = true;
        else CurrentCommand = null;
        return ret;
    }
}
