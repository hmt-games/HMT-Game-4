using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot : MonoBehaviour
{
    public enum BotType
    {
        Normal,
        Harvest,
        Pluck,
        Till,
        Spray,
        Sample,
        Plant
    }
    
    struct BotInfo
    {
        public int FloorIdx;
        public int X;
        public int Y;
        public GridCellBehavior[,] CellsOnFloor;
        public int MaxX;
        public int MaxY;
        public BotType CurrentBotType;
    }

    private BotInfo _botInfo;

    [SerializeField] private float speed = 10.0f;
    private bool _walking = false;
    private Vector3 _targetPos = Vector3.zero;

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        
        // test script to spawn bot on the first floor, at 0,0;
        TestSpawn();
    }

    private void Update()
    {
        if (_walking)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, speed * Time.deltaTime);
            if (transform.position == _targetPos) _walking = false;
            return;
        }
        
        Walk();
        
        if (Input.GetKeyDown(KeyCode.Space)) _animator.SetTrigger("PerformAction");
    }

    private void TestSpawn()
    {
        GridCellBehavior[,] cellsOnFloor = GameManager.Instance.parentTower.floors[0].Cells;
        _botInfo = new BotInfo
        {
            X = 0, Y = 0, 
            CellsOnFloor = cellsOnFloor,
            MaxX = cellsOnFloor.GetLength(0),
            MaxY = cellsOnFloor.GetLength(1),
            CurrentBotType = BotType.Normal
        };
    }

    private void Walk()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (ValidTargetPosition(_botInfo.X, _botInfo.Y + 1))
            {
                _targetPos = _botInfo.CellsOnFloor[_botInfo.X, _botInfo.Y + 1].transform.position;
                _botInfo.Y += 1;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (ValidTargetPosition(_botInfo.X, _botInfo.Y - 1))
            {
                _targetPos = _botInfo.CellsOnFloor[_botInfo.X, _botInfo.Y - 1].transform.position;
                _botInfo.Y -= 1;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (ValidTargetPosition(_botInfo.X - 1, _botInfo.Y))
            {
                _targetPos = _botInfo.CellsOnFloor[_botInfo.X - 1, _botInfo.Y].transform.position;
                _botInfo.X -= 1;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (ValidTargetPosition(_botInfo.X + 1, _botInfo.Y))
            {
                _targetPos = _botInfo.CellsOnFloor[_botInfo.X + 1, _botInfo.Y].transform.position;
                _botInfo.X += 1;
            }
        }
    }

    private void Action()
    {
        GameActionGoldenFinger.Instance.ShowActionWheel(transform.position);
    }

    private void Transform()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _botInfo.CurrentBotType = BotType.Harvest;
            _animator.SetTrigger("TransHarvest");
        }
        
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _botInfo.CurrentBotType = BotType.Pluck;
            _animator.SetTrigger("TransPluck");
        }
        
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _botInfo.CurrentBotType = BotType.Plant;
            _animator.SetTrigger("TransPlant");
        }
        
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            _botInfo.CurrentBotType = BotType.Sample;
            _animator.SetTrigger("TransSample");
        }
        
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            _botInfo.CurrentBotType = BotType.Spray;
            _animator.SetTrigger("TransSpray");
        }
        
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            _botInfo.CurrentBotType = BotType.Till;
            _animator.SetTrigger("TransTill");
        }
        
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            _botInfo.CurrentBotType = BotType.Normal;
            _animator.SetTrigger("TransNormal");
        }
    }

    private bool ValidTargetPosition(int x, int y)
    {
        bool ret = x < _botInfo.MaxX && y < _botInfo.MaxY && x >= 0 && y >= 0;
        if (ret) _walking = true;
        return ret;
    }
}
