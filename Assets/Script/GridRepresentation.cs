using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridRepresentation
{
    public static Vector3 PositionFromGridCoord(int row, int col)
    {
        return new Vector3(row, 0.5f, col);
    }
    
    public static GridType[,] LevelConfigTo2DArray(LevelConfig config)
    {
        GridType[,] res = new GridType[config.width, config.height];
        for (int i = 0; i < config.width; i++)
        {
            for (int j = 0; j < config.height; j++)
            {
                res[i, j] = GridType.Empty;
            }
        }

        Vector2 pos = config.playerStartPos;
        res[(int)pos.x, (int)pos.y] = GridType.Player;

        pos = config.goalPos;
        res[(int)pos.x, (int)pos.y] = GridType.Goal;

        foreach (Vector2 pos_ in config.keysPos)
        {
            res[(int)pos_.x, (int)pos_.y] = GridType.Key;
        }
        
        foreach (Vector2 pos_ in config.reversesPos)
        {
            res[(int)pos_.x, (int)pos_.y] = GridType.Reverse;
        }
        
        foreach (Vector2 pos_ in config.blocksPos)
        {
            res[(int)pos_.x, (int)pos_.y] = GridType.Block;
        }

        return res;
    } 

    public enum GridType
    {
        Empty,
        Player,
        Goal,
        Key,
        Reverse,
        Block,
    }
}
