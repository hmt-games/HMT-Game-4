using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SoilSample {
    

    public readonly NutrientSolution nutrientSolution;
    public readonly float gameTime;
    public readonly ulong gameTick;
    public readonly int floorIndex;
    public readonly int tileX;
    public readonly int tileY;
    public readonly string samplerName;


    public SoilSample(NutrientSolution nutrientSolution, float gameTime, ulong gameTick, int floorIndex, int tileX, int tileY, string samplerName) {
        this.nutrientSolution = nutrientSolution;
        this.gameTime = gameTime;
        this.gameTick = gameTick;
        this.floorIndex = floorIndex;
        this.tileX = tileX;
        this.tileY = tileY;
        this.samplerName = samplerName;
    }


    public SoilSample(NutrientSolution nutrientSolution, float gameTime, ulong gameTick, int floorIndex, Vector2Int tileAddress, string samplerName) {
        this.nutrientSolution = nutrientSolution;
        this.gameTime = gameTime;
        this.gameTick = gameTick;
        this.floorIndex = floorIndex;
        this.tileX = tileAddress.x;
        this.tileY = tileAddress.y;
        this.samplerName = samplerName;
    }
}
