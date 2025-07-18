using GameConstant;
using HMT.Puppetry;
using System.Collections;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;

public class StationCellBehavior : GridCellBehavior {
    private const float ICON_CYCLE_TIME = .3f;

    public StationConfigSO config;


    [Header("Subobject Pointers")]
    [SerializeField]
    internal SpriteRenderer BaseSprite;

    [SerializeField]
    [Tooltip("This should be a child sprite used to indicate different station options. It may be in different places based on the Interaction the station supports")]
    internal SpriteRenderer IconSprite;

    [SerializeField]
    internal Transform ALevelBar;
    [SerializeField]
    internal Transform BLevelBar;
    [SerializeField]
    internal Transform CLevelBar;
    [SerializeField]
    internal Transform DLevelBar;


    public override bool AllowsDrops => false;

    void Start() {
        if (config != null && config.botModes.Count == 0 && config.interaction == StationInteraction.SwitchBotMode) {
            Debug.LogError("SwitchBotMode station has no defined bot modes");
        }

        StartCoroutine(IconAnimation());
    }

    public void SetStationConfig(StationConfigSO config) {
        StopAllCoroutines();
        this.config = config;
        StartCoroutine(IconAnimation());
    }

    public void Interact(FarmBot bot) {
        if (config == null) {
            Debug.LogError($"Station config is not set for station at Floor {parentFloor.floorNumber} [{gridX},{gridZ}]");
            return;
        }

        switch (config.interaction) {
            case StationInteraction.Score:
                GameActions.Instance.Score(bot);
                break;
            case StationInteraction.Trash:
                GameActions.Instance.DumpInventory(bot);
                break;
            case StationInteraction.SwitchBotMode:
                if (config.inventoryRule == StationInventoryRule.DumpFirst) {
                    GameActions.Instance.DumpInventory(bot);
                }

                //find next bot mode that's not this one and change to it.
                int currIdx = config.botModes.IndexOf(bot.BotModeConfig);
                currIdx = (currIdx + 1) % config.botModes.Count;
                GameActions.Instance.SwitchBotMode(bot, config.botModes[currIdx]);
                break;
            case StationInteraction.Reservoir:
                if (config.inventoryRule == StationInventoryRule.DumpFirst) {
                    GameActions.Instance.DumpInventory(bot);
                }
                GameActions.Instance.FillBotReservoir(bot, config.reservoirAddition);
                break;
            case StationInteraction.SeedBank:
                if (config.inventoryRule == StationInventoryRule.DumpFirst) {
                    GameActions.Instance.DumpInventory(bot);
                }
                GameActions.Instance.FillBotInventory(bot, config.seedConfig);
                break;
            default:
                Debug.LogWarning($"Unhandled station interaction: {config.interaction} for bot {bot.name} at {transform.position}.");
                break;
        }
    }

    //Checks if the bot can legally interact with the station
    internal bool IsCompatible(FarmBot farmBot) {
        switch (config.interaction, config.inventoryRule) {
            case (StationInteraction.Score, _):
                return farmBot.Inventory.HasPlantInventory && !farmBot.Inventory.IsEmpty;
            case (StationInteraction.Trash, _):
                return !farmBot.Inventory.IsEmpty;
            case (StationInteraction.SwitchBotMode, StationInventoryRule.RequireEmpty):
                return farmBot.Inventory.IsEmpty;
            case (StationInteraction.SwitchBotMode, _):
                return true;
            case (StationInteraction.Reservoir, StationInventoryRule.RequireEmpty):
                return farmBot.Inventory.IsEmpty && farmBot.Inventory.HasReservoir;
            case (StationInteraction.Reservoir, _):
                return farmBot.Inventory.HasReservoir;
            case (StationInteraction.SeedBank, StationInventoryRule.RequireEmpty):
                return farmBot.Inventory.IsEmpty && farmBot.Inventory.HasPlantInventory;
            case (StationInteraction.SeedBank, _):
                return farmBot.Inventory.HasPlantInventory;
            default:
                return false;
        }
    }

    IEnumerator IconAnimation() {
        switch (config.interaction) {
            case StationInteraction.Score:
                BaseSprite.sprite = SpriteResources.Instance.scoreStation;
                IconSprite.gameObject.SetActive(false);
                ALevelBar.gameObject.SetActive(false);
                BLevelBar.gameObject.SetActive(false);
                CLevelBar.gameObject.SetActive(false);
                DLevelBar.gameObject.SetActive(false);
                break;
            case StationInteraction.Trash:
                BaseSprite.sprite = SpriteResources.Instance.discardStation;
                IconSprite.gameObject.SetActive(false);
                ALevelBar.gameObject.SetActive(false);
                BLevelBar.gameObject.SetActive(false);
                CLevelBar.gameObject.SetActive(false);
                DLevelBar.gameObject.SetActive(false);
                yield break;
            case StationInteraction.Reservoir: // TODO - we need a way to visually indicate the concentrations
                BaseSprite.sprite = SpriteResources.Instance.reservoirStation;
                IconSprite.gameObject.SetActive(false);
                ALevelBar.gameObject.SetActive(true);
                BLevelBar.gameObject.SetActive(true);
                CLevelBar.gameObject.SetActive(true);
                DLevelBar.gameObject.SetActive(true);
                if (ALevelBar != null) {
                    ALevelBar.localScale = new Vector3(config.reservoirAddition.GetNutrientConcentration(NutrientType.A), 1, 1);
                }
                if (BLevelBar != null) {
                    BLevelBar.localScale = new Vector3(config.reservoirAddition.GetNutrientConcentration(NutrientType.B), 1, 1);
                }
                if (CLevelBar != null) {
                    CLevelBar.localScale = new Vector3(config.reservoirAddition.GetNutrientConcentration(NutrientType.C), 1, 1);
                }
                if (DLevelBar != null) {
                    DLevelBar.localScale = new Vector3(config.reservoirAddition.GetNutrientConcentration(NutrientType.D), 1, 1);
                }
                yield break;
            case StationInteraction.SeedBank:
                BaseSprite.sprite = SpriteResources.Instance.seedBankStation;
                IconSprite.gameObject.SetActive(true);
                ALevelBar.gameObject.SetActive(false);
                BLevelBar.gameObject.SetActive(false);
                CLevelBar.gameObject.SetActive(false);
                DLevelBar.gameObject.SetActive(false);
                IconSprite.sprite = config.seedConfig.plantSprites[0];
                break;
            case StationInteraction.SwitchBotMode:
                BaseSprite.sprite = SpriteResources.Instance.modeChangeStation;
                IconSprite.gameObject.SetActive(true);
                ALevelBar.gameObject.SetActive(false);
                BLevelBar.gameObject.SetActive(false);
                CLevelBar.gameObject.SetActive(false);
                DLevelBar.gameObject.SetActive(false);
                if (config.botModes.Count == 1) {
                    IconSprite.sprite = config.botModes[0].hatSprite;
                    yield break;
                }
                float lastChange = Time.time;
                int currentModeIndex = 0;
                while (true) {
                    if (Time.time - lastChange > ICON_CYCLE_TIME) {
                        lastChange = Time.time;
                        IconSprite.sprite = config.botModes[currentModeIndex].hatSprite;
                        currentModeIndex = (currentModeIndex + 1) % config.botModes.Count;
                    }
                    yield return null;
                }
        }
    }

    public override void OnTick() {
        // just do nothing for now
    }

    // when drained on station tile, we just discard the drained amount?
    // or just pass down to next floor
    public override NutrientSolution OnWater(NutrientSolution volumes) {
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
                rep["interaction"] = config.interaction.ToString();
                switch (config.interaction) {
                    case StationInteraction.Score:
                    case StationInteraction.Trash:
                        break;
                    case StationInteraction.SeedBank:
                        rep["seed_bank_plant"] = config.seedConfig.speciesName;
                        break;
                    case StationInteraction.Reservoir:
                        rep["reservoir_solution"] = config.reservoirAddition.ToFlatJSON();
                        break;
                    case StationInteraction.SwitchBotMode:
                        rep["bot_modes"] = new JArray(config.botModes.ConvertAll(mode => mode.name));
                        break;
                }
                break;
        }

        return rep;
    }
}
