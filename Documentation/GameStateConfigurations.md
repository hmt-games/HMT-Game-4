# Game State Configurations

### <span style="color:green">Plants</span>

- ```speciesName```: name of this plant species. Each species has distinct properties outlined below. Species names are case sensitive, and will be used in level config as an index (discussed further in the level config section).
- ```waterCapacity```: the capacity of the internal water reservoir of the plant. This is different from the water reservoir of the <span style="color:brown">soil</span> tile that the plant resides in. In case of a drought, the plant will still have access to the water stored internally as a buffer.
- ```uptakeRate```: the amount of water this plant can extract from the <span style="color:brown">soil</span> tile per tick.
- ```metabolismRate```: 
- ```fruitYieldAvg```, ```fruitYieldStdDev```: The normal distribution of the amount of fruits this species may bear when picked by bots.

```json
"speciesName" :
{		
	"waterCapacity" : <float>,
	"uptakeRate" : <float>,
	"metabolismRate" : <float>,
	"metabolismFactor" : [<float>, <float>, <float>, <float>],
	"rootHeightTransition" : <float>,
	"growthToleranceThreshold" : <float>,
	
	"leachingEnergyThreshold" : <float>,
	"leachingFactor" : [<float>, <float>, <float>, <float>],
	
	"fruitYieldAvg" : <float>,
	"fruitYieldStdDev" : <float>,

	"maxHealthHistory" : <int>,
	"stageTransitionThreshold" : [<float>, <float>, <float>, <float>],
	
	"traits" : [<string>, <string>, ..., <string>]
}
```

### <span style="color:purple">Bots</span>

```json
"botModeName" :
{
	// inventory
	"reservoirCapacity" : <float>, // 0f - 100f
	"plantInventoryCapacity" : <int>, // 0 - 100
	
	// capabilities
	"movementSpeed" : <float>, // defined in tiles per game tick
	"sensingRange" : [<int>, <int>]
	"supportedActions" : 
	{
		// "actionName" : actionTime(defined as a multiple of tick)
		// e.g. "pick" : 1.0
		<string> : <float>,
		<string> : <float>,
		...
	}
}
```

### <span style="color:brown">Soil</span>

```JSON
"soilTypeName" : 
{
	"waterCapacity" : <float>,
	"drainRate" : <float>
}
```

### Level (Tower)

```json
{
	"levelName" : <string>

"width" : <int>,
	"depth" : <int>,
	"height" : <int>,
"configs":<string>,
"seed":<int>,
}
	
	"floors" : // this list should be of height length
[
		[     // this should be a width * depth list
		// cell (1,0)
		{
			"gridType" : <"soil" | "station">,
			"gridConfig" : <string>, // index into soilTypeName or stationName
			"nutrientLevels" : [<float>, <float>, <float>, <float>, <float>],
			"botOnGrid" : "botMode" : <string>, // index into botModeName, "none" if no bot on this grid
			"plants" : 
			[
				"plant0" :  <plantData>,
				"plant1" :  <plantData>,
				// ...
			]
		},
		
		{ // (cell (2,0) },
		{ // (cell (3,0)},
		// ...
	],
	
	[],
	[],
	// ...
]
}
```

