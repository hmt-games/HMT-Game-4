using GameConstant;
using HMT.Puppetry;
using System.Collections;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class StationCellBehavior : GridCellBehavior
{
    private const float ICON_CYCLE_TIME = .3f;

    public enum StationInteraction {
        Score,
        Trash,
        SwitchBotMode,
        Reservoir,
        SeedBank
    }

    public StationInteraction interaction;

    public float interactionTime = 1f;

    public List<BotModeSO> botModes;

    public NutrientSolution reservoirSolution = NutrientSolution.Empty;
    public PlantConfigSO seedBankPlantConfig;
    public bool requireEmptyInventory = false;
    
    [SerializeField]
    [Tooltip("This should be a child sprite used to indicate different station options. It will be in different places based on the Interaction the station supports")]
    internal SpriteRenderer IconSprite;

    public override bool AllowsDrops => false;

    void Start() {
        if(botModes.Count == 0 && interaction == StationInteraction.SwitchBotMode) {
            Debug.LogError("SwitchBotMode station has no defined bot modes");
        }

        StartCoroutine(IconAnimation());
    }

    public BotModeSO Interact(OldFarmPuppetBot bot)
    {
        if (tileType == TileType.SprayAStation | tileType == TileType.SprayBStation |
            tileType == TileType.SprayCStation | tileType == TileType.SprayDStation)
        {
            StartCoroutine(bot.SprayUp());
        } 
        else if (tileType == TileType.DiscardStation)
        {
            bot.DumpInventory();
        } 
        else if (tileType == TileType.ScoreStation)
        {
            GameActions.Instance.Score(bot);
        }

        GameConfigSO gameConfig = GameManager.Instance.gameConfig;
        
        return tileType switch
        {
            TileType.HarvestStation => gameConfig.harvest,
               TileType.SprayAStation 
            or TileType.SprayBStation
            or TileType.SprayCStation
            or TileType.SprayDStation => gameConfig.spray,
            TileType.PluckStation => gameConfig.pick,
            TileType.PlantStation => gameConfig.plant,
            TileType.TillStation => gameConfig.till,
            TileType.SampleStation => gameConfig.sample,
            _ => null
        };
    }

    public void Interact(FarmBot bot) {
        switch(interaction) {
            case StationInteraction.Score:
                GameActions.Instance.Score(bot);
                break;
            case StationInteraction.Trash:
                GameActions.Instance.DumpInventory(bot);
                break;
            case StationInteraction.SwitchBotMode:
                //find next bot mode that's not this one and change to it.
                int currIdx = botModes.IndexOf(bot.BotModeConfig);
                currIdx = (currIdx + 1) % botModes.Count;
                GameActions.Instance.SwitchBotMode(bot, botModes[currIdx]);
                break;
            case StationInteraction.Reservoir:
                GameActions.Instance.FillBotReservoir(bot, reservoirSolution);
                break;
            case StationInteraction.SeedBank:
                GameActions.Instance.FillBotInventory(bot, seedBankPlantConfig);
                break;
            default:
                Debug.LogWarning($"Unhandled station interaction: {interaction} for bot {bot.name} at {transform.position}.");
                break;
        }
    }

    //Checks if the bot can legally interact with the station
    internal bool IsCompatible(FarmBot farmBot) {
        switch(interaction) {
            case StationInteraction.Score:
                return farmBot.Inventory.HasPlantInventory && !farmBot.Inventory.IsEmpty;
            case StationInteraction.Trash:
                return !farmBot.Inventory.IsEmpty;
            case StationInteraction.SwitchBotMode:
                if (requireEmptyInventory) {
                    return farmBot.Inventory.IsEmpty;
                }
                else {
                    return true;
                }
            case StationInteraction.Reservoir:
                if(requireEmptyInventory) {
                    return farmBot.Inventory.IsEmpty;
                }
                else {
                    return farmBot.Inventory.IsEmpty || farmBot.Inventory.HasReservoir;
                }
            case StationInteraction.SeedBank:
                if (requireEmptyInventory) {
                    return farmBot.Inventory.IsEmpty;
                }
                else {
                    return farmBot.Inventory.IsEmpty || farmBot.Inventory.HasPlantInventory;
                }
            default:
                return false;
        }
    }

    IEnumerator IconAnimation() {
        switch(interaction) {
            case StationInteraction.Score:
            case StationInteraction.Trash:
            case StationInteraction.Reservoir: // TODO - we need a way to visually indicate the concentrations
                yield break;
            case StationInteraction.SeedBank:
                IconSprite.sprite = seedBankPlantConfig.plantSprites[0];
                break;
            case StationInteraction.SwitchBotMode:
                if (botModes.Count == 1) {
                    IconSprite.sprite = botModes[0].hatSprite;
                    yield break;
                }
                float lastChange = Time.time;
                int currentModeIndex = 0;
                while (true) {
                    if (Time.time - lastChange > ICON_CYCLE_TIME) {
                        lastChange = Time.time;
                        IconSprite.sprite = botModes[currentModeIndex].hatSprite;
                        currentModeIndex = (currentModeIndex + 1) % botModes.Count;
                    }
                    yield return null;
                }
        }
    }

    public override void OnTick()
    {
        // just do nothing for now
    }
    
    // when drained on station tile, we just discard the drained amount?
    // or just pass down to next floor
    public override NutrientSolution OnWater(NutrientSolution volumes)
    {
        //TODO we may want to have this return empty (ie no water drains out of a station to lower floors).
        return NutrientSolution.Empty;
    }

    public override JObject HMTStateRep(HMTStateLevelOfDetail lod) {
        JObject rep = base.HMTStateRep(lod);
        switch (lod) {
            case HMTStateLevelOfDetail.Full:
            case HMTStateLevelOfDetail.Visible:
            case HMTStateLevelOfDetail.Seen:
            case HMTStateLevelOfDetail.Unseen:
                rep["cell_type"] = "station";
                rep["station_type"] = tileType.ToString();
                break;
        }

        return rep;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(StationCellBehavior))]
public class StationCellBehaviorEditor : Editor {
    public override void OnInspectorGUI() {
        StationCellBehavior stationCell = (StationCellBehavior)target;

        EditorGUILayout.BeginVertical();

        stationCell.interaction = (StationCellBehavior.StationInteraction)EditorGUILayout.EnumPopup("Interaction", stationCell.interaction);
        stationCell.interactionTime = EditorGUILayout.FloatField("Interaction Time", stationCell.interactionTime);

        switch(stationCell.interaction) {
            case StationCellBehavior.StationInteraction.Trash:
            case StationCellBehavior.StationInteraction.Score:
            case StationCellBehavior.StationInteraction.SeedBank:
                stationCell.requireEmptyInventory = EditorGUILayout.Toggle("Require Empty Inventory", stationCell.requireEmptyInventory);
                stationCell.seedBankPlantConfig = (PlantConfigSO)EditorGUILayout.ObjectField("Seed Bank Plant Config", stationCell.seedBankPlantConfig, typeof(PlantConfigSO), false);
                stationCell.IconSprite = (SpriteRenderer)EditorGUILayout.ObjectField("Icon Sprite", stationCell.IconSprite, typeof(SpriteRenderer), true);
                break;
            case StationCellBehavior.StationInteraction.Reservoir:
                stationCell.requireEmptyInventory = EditorGUILayout.Toggle("Require Empty Inventory", stationCell.requireEmptyInventory);
                //TODO - Need a custom inspector property for NutrientSolutions
                Vector4 nutrients = stationCell.reservoirSolution.nutrients;
                //stationCell.reservoirSolution.water = EditorGUILayout.FloatField("Water Ratio", stationCell.reservoirSolution.water);
                nutrients.x = EditorGUILayout.Slider("Nutrient A", nutrients.x, 0, 1.0f);
                nutrients.y = EditorGUILayout.Slider("Nutrient B", nutrients.y, 0, 1.0f);
                nutrients.z = EditorGUILayout.Slider("Nutrient C", nutrients.z, 0, 1.0f);
                nutrients.w = EditorGUILayout.Slider("Nutrient D", nutrients.w, 0, 1.0f);
                stationCell.reservoirSolution.nutrients = nutrients;


                //stationCell.reservoirSolution = (NutrientSolution)EditorGUILayout.ObjectField("Reservoir Solution", stationCell.reservoirSolution, typeof(NutrientSolution), true);
                break;
            case StationCellBehavior.StationInteraction.SwitchBotMode:
                
                stationCell.requireEmptyInventory = EditorGUILayout.Toggle("Require Empty Inventory", stationCell.requireEmptyInventory);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("botModes"));
                stationCell.IconSprite = (SpriteRenderer)EditorGUILayout.ObjectField("Icon Sprite", stationCell.IconSprite, typeof(SpriteRenderer), true);
                break;
        }



        EditorGUILayout.EndVertical();
    }
}


#endif
