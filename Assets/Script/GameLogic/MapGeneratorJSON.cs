using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Text;
using Cinemachine;
using GameConstant;
using UnityEngine.Serialization;

public class MapGeneratorJSON : NetworkBehaviour
{
    [SerializeField] private TextAsset configJSON;
    [SerializeField] private TextAsset towerJSON;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GridTheme grid2DTheme;
    [SerializeField] private GameObject plantPrefab;
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject floorCamPrefab;


    [FormerlySerializedAs("_plantConfigs")] 
    public Dictionary<string, PlantConfig> plantConfigs;
    private Dictionary<string, SoilConfig> _soilConfigs;
    // private Dictionary<string, GameObject> _name2Plant;

    private JObject _configJObject;
    
    public int _width;
    public int _depth;
    public int _height;

    public static MapGeneratorJSON Instance;

    public NetworkId towerID;
    public NetworkId[] floorIDs;
    // 3D array: floorNum * gridX * gridY
    public NetworkId[,,] gridCellIDs;
    // 4D array: floorNum * gridX * gridY * plantNum
    public NetworkId[,,,] plantIDs;
    // 4D array plantstructs (containing actual data) here...

    // 3D array of soil? gridcell? struct (containing actual data)

    public bool mapUpdated;

    public bool towerIDReceived;
    public bool floorIDsReceived;
    public bool gridCellIDsReceived;
    public bool plantIDsReceived;
    public bool soilConfigReceived;
    public bool plantConfigReceived;
    
    private void Awake()
    {
        if (Instance) Destroy(this.gameObject);
        else Instance = this;
        plantConfigs = new Dictionary<string, PlantConfig>();
        _soilConfigs = new Dictionary<string, SoilConfig>();
        // _name2Plant = new Dictionary<string, GameObject>();
        _configJObject = JObject.Parse(configJSON.text);
        JObject towerJObject = JObject.Parse(towerJSON.text);
        _width = (int)towerJObject["Tower"]["width"];
        _depth = (int)towerJObject["Tower"]["depth"];
        _height = (int)towerJObject["Tower"]["height"];

        Debug.Log("height: " + _height + " width: " + _width + " depth: " + _depth);
        floorIDs = new NetworkId[_height];
        gridCellIDs = new NetworkId[_height, _width, _depth];
        plantIDs = new NetworkId[_height, _width, _depth, GLOBAL_CONSTANTS.MAX_PLANT_COUNT_PER_TILE];
    }

    private void Start()
    {
        //CreateTower();
    }



    //[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    [Rpc]
    public void RPC_PlantNextStage()
    {
        Debug.Log("Plant sprite goes to next stage");
        GameManager.Instance.PlantNextStage();
    }

    public void OnGrowButtonPressed()
    {
        if (BasicSpawner._runner.IsServer)
        {
            RPC_PlantNextStage();
        }
        else
        {
            Debug.LogWarning("Only the server can trigger plant growth.");
            //RPC_PlantNextStage();
        }
    }




    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestNetworkIDs(PlayerRef requestingPlayer)
    {
        Debug.Log("Client requesting object IDs from the server.");

        if (BasicSpawner._runner.IsServer)
        {
            // Server sends data to the client using the data streaming method.
            SendNetworkData(requestingPlayer);
            SendConfigData(requestingPlayer);
        }
    }

    private void SendNetworkData(PlayerRef player)
    {
        var runner = BasicSpawner._runner;

        // Prepare the data as byte arrays.
        byte[] towerIDData = SerializeNetworkID(towerID);
        byte[] floorIDsData = SerializeNetworkIDs(floorIDs);
        byte[] gridCellIDsData = SerializeGridCellIDs(gridCellIDs);
        byte[] plantIDsData = SerializePlantIDs(plantIDs);

        // Define a key to differentiate the data types.
        var towerKey = ReliableKey.FromInts(0, 0, 0, 0);
        var floorKey = ReliableKey.FromInts(1, 0, 0, 0);
        var gridCellKey = ReliableKey.FromInts(2, 0, 0, 0);
        var plantKey = ReliableKey.FromInts(3, 0, 0, 0);

        // Send the data to the requesting player using their PlayerRef
        runner.SendReliableDataToPlayer(player, towerKey, towerIDData);
        runner.SendReliableDataToPlayer(player, floorKey, floorIDsData);
        runner.SendReliableDataToPlayer(player, gridCellKey, gridCellIDsData);
        runner.SendReliableDataToPlayer(player, plantKey, plantIDsData);
    }

    public byte[] SerializeNetworkID(NetworkId id)
    {
        // Convert the NetworkId into bytes
        return BitConverter.GetBytes(id.Raw); // Convert the Raw uint representation into bytes
    }

    public byte[] SerializeNetworkIDs(NetworkId[] ids)
    {
        // Convert the array into bytes
        return System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Wrapper<NetworkId[]>(ids)));
    }


    public byte[] SerializeGridCellIDs(NetworkId[,,] ids)
    {
        List<NetworkId> flatList = new List<NetworkId>();

        foreach (var id in ids)
        {
            flatList.Add(id);
        }

        return System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Wrapper<NetworkId[]>(flatList.ToArray())));
    }

    public byte[] SerializePlantIDs(NetworkId[,,,] ids)
    {
        List<NetworkId> flatList = new List<NetworkId>();

        foreach (var id in ids)
        {
            flatList.Add(id);
        }

        return System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Wrapper<NetworkId[]>(flatList.ToArray())));
    }


    [System.Serializable]
    public class Wrapper<T>
    {
        public T Items;
        public Wrapper(T items) => Items = items;
    }

    public NetworkId DeserializeNetworkID(ArraySegment<byte> data)
    {
        // Convert the byte data back into NetworkId
        uint rawValue = BitConverter.ToUInt32(data.Array, data.Offset);
        return new NetworkId { Raw = rawValue };
    }

    public NetworkId[] DeserializeNetworkIDs(ArraySegment<byte> data)
    {
        return JsonUtility.FromJson<Wrapper<NetworkId[]>>(System.Text.Encoding.UTF8.GetString(data)).Items;
    }


    public NetworkId[,,] DeserializeGridCellIDs(ArraySegment<byte> data, int floorCount, int gridX, int gridY)
    {
        NetworkId[] flatArray = JsonUtility.FromJson<Wrapper<NetworkId[]>>(System.Text.Encoding.UTF8.GetString(data)).Items;
        NetworkId[,,] gridCellIDs = new NetworkId[floorCount, gridX, gridY];

        int index = 0;
        for (int f = 0; f < floorCount; f++)
        {
            for (int x = 0; x < gridX; x++)
            {
                for (int y = 0; y < gridY; y++)
                {
                    gridCellIDs[f, x, y] = flatArray[index++];
                }
            }
        }

        return gridCellIDs;
    }

    public NetworkId[,,,] DeserializePlantIDs(ArraySegment<byte> data, int floorCount, int gridX, int gridY, int plantCount)
    {
        NetworkId[] flatArray = JsonUtility.FromJson<Wrapper<NetworkId[]>>(System.Text.Encoding.UTF8.GetString(data)).Items;
        NetworkId[,,,] plantIDs = new NetworkId[floorCount, gridX, gridY, plantCount];

        int index = 0;
        for (int f = 0; f < floorCount; f++)
        {
            for (int x = 0; x < gridX; x++)
            {
                for (int y = 0; y < gridY; y++)
                {
                    for (int p = 0; p < plantCount; p++)
                    {
                        plantIDs[f, x, y, p] = flatArray[index++];
                    }
                }
            }
        }

        return plantIDs;
    }

    [System.Serializable]
    public class PlantConfigDTO
    {
        public float capacities;
        public float uptakeRate;
        public float[] metabolismNeeds;
        public float metabolismWaterNeeds;
        public float[] metabolismFactor;
        public float[] growthConsumptionRateLimit;
        public float[] growthFactor;
        public float rootHeightTransition;
        public float growthToleranceThreshold;
        public float leachingEnergyThreshold;
        public float[] leachingFactor;
        public bool onWaterCallbackBypass;
        public List<string> spriteBase64; // Serialized sprite data in base64 format
    }

    [System.Serializable]
    public class SoilConfigDTO
    {
        public float drainTime;
        public float capacities;
    }

    public void SendConfigData(PlayerRef player)
    {
        var runner = BasicSpawner._runner;

        // Serialize the configurations
        byte[] plantConfigData = SerializePlantConfigs();
        byte[] soilConfigData = SerializeSoilConfigs();

        // Define unique keys for each data type
        var plantConfigKey = ReliableKey.FromInts(4, 0, 0, 0);
        var soilConfigKey = ReliableKey.FromInts(5, 0, 0, 0);

        // Send the data to the requesting player
        runner.SendReliableDataToPlayer(player, plantConfigKey, plantConfigData);
        runner.SendReliableDataToPlayer(player, soilConfigKey, soilConfigData);
    }

    public byte[] SerializePlantConfigs()
    {
        var dtoDictionary = plantConfigs.ToDictionary(
            kvp => kvp.Key,
            kvp => new PlantConfigDTO
            {
                capacities = kvp.Value.capacities,
                uptakeRate = kvp.Value.uptakeRate,
                metabolismNeeds = ConvertToFloatArray(kvp.Value.metabolismNeeds),
                metabolismWaterNeeds = kvp.Value.metabolismWaterNeeds,
                metabolismFactor = ConvertToFloatArray(kvp.Value.metabolismFactor),
                growthConsumptionRateLimit = ConvertToFloatArray(kvp.Value.growthConsumptionRateLimit),
                growthFactor = ConvertToFloatArray(kvp.Value.growthFactor),
                rootHeightTransition = kvp.Value.rootHeightTransition,
                growthToleranceThreshold = kvp.Value.growthToleranceThreshold,
                leachingEnergyThreshold = kvp.Value.leachingEnergyThreshold,
                leachingFactor = ConvertToFloatArray(kvp.Value.leachingFactor),
                onWaterCallbackBypass = kvp.Value.onWaterCallbackBypass,
                spriteBase64 = kvp.Value.plantSprites.Select(SpriteToBase64).ToList()
            }
        );

        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dtoDictionary));
    }

    public byte[] SerializeSoilConfigs()
    {
        var dtoDictionary = _soilConfigs.ToDictionary(
            kvp => kvp.Key,
            kvp => new SoilConfigDTO
            {
                drainTime = kvp.Value.drainTime,
                capacities = kvp.Value.capacities
            }
        );

        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dtoDictionary));
    }

    public void DeserializePlantConfigs(byte[] data)
    {
        var json = Encoding.UTF8.GetString(data);
        var dtoDictionary = JsonConvert.DeserializeObject<Dictionary<string, PlantConfigDTO>>(json);

        plantConfigs = dtoDictionary.ToDictionary(
            kvp => kvp.Key,
            kvp =>
            {
                var config = ScriptableObject.CreateInstance<PlantConfig>();
                config.capacities = kvp.Value.capacities;
                config.uptakeRate = kvp.Value.uptakeRate;
                config.metabolismNeeds = ConvertToVector4(kvp.Value.metabolismNeeds);
                config.metabolismWaterNeeds = kvp.Value.metabolismWaterNeeds;
                config.metabolismFactor = ConvertToVector4(kvp.Value.metabolismFactor);
                config.growthConsumptionRateLimit = ConvertToVector4(kvp.Value.growthConsumptionRateLimit);
                config.growthFactor = ConvertToVector4(kvp.Value.growthFactor);
                config.rootHeightTransition = kvp.Value.rootHeightTransition;
                config.growthToleranceThreshold = kvp.Value.growthToleranceThreshold;
                config.leachingEnergyThreshold = kvp.Value.leachingEnergyThreshold;
                config.leachingFactor = ConvertToVector4(kvp.Value.leachingFactor);
                config.onWaterCallbackBypass = kvp.Value.onWaterCallbackBypass;
                config.plantSprites = kvp.Value.spriteBase64.Select(Base64ToSprite).ToList();


                //DisplayReceivedSprite(config.plantSprites[0]);
                return config;
            }
        );
    }

    private float[] ConvertToFloatArray(Vector4 vector)
    {
        return new float[] { vector.x, vector.y, vector.z, vector.w };
    }

    private Vector4 ConvertToVector4(float[] array)
    {
        if (array.Length != 4) throw new ArgumentException("Array must have exactly 4 elements.");
        return new Vector4(array[0], array[1], array[2], array[3]);
    }

    private string SpriteToBase64(Sprite sprite)
    {
        Texture2D texture = sprite.texture;
        byte[] textureData = texture.EncodeToPNG(); // Convert texture to PNG
        return Convert.ToBase64String(textureData); // Encode as Base64
    }

    private Sprite Base64ToSprite(string base64)
    {
        byte[] textureData = Convert.FromBase64String(base64); // Decode Base64
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(textureData); // Load texture from PNG data

        // Create a sprite from the texture
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    public void DeserializeSoilConfigs(byte[] data)
    {
        var json = Encoding.UTF8.GetString(data);
        var dtoDictionary = JsonConvert.DeserializeObject<Dictionary<string, SoilConfigDTO>>(json);

        _soilConfigs = dtoDictionary.ToDictionary(
            kvp => kvp.Key,
            kvp =>
            {
                var config = ScriptableObject.CreateInstance<SoilConfig>();
                config.drainTime = kvp.Value.drainTime;
                config.capacities = kvp.Value.capacities;
                return config;
            }
        );
    }


    private void CreateConfigs()
    {
        foreach (var config in _configJObject.Properties())
        {
            if (config.Name.ToLower().Contains("plant")) CreatePlantConfig(config.Name);
            else CreateSoilConfig(config.Name);
        }
    } 

    public void CreateTower()
    {
        JObject towerJObject = JObject.Parse(towerJSON.text);
        int width = (int)towerJObject["Tower"]["width"];
        int depth = (int)towerJObject["Tower"]["depth"];
        int height = (int)towerJObject["Tower"]["height"];

        NetworkObject towerObj;

        // if this is server instance, spawn and initialzie the tower object
        if (BasicSpawner._runner.IsServer)
        {
            towerObj = BasicSpawner._runner.Spawn(towerPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
            towerID = towerObj.Id;
            CreateConfigs();
        }
        //if this is not server instance, extract information from server
        else
        {
            if (!BasicSpawner._runner.TryFindObject(towerID, out towerObj))
            {
                Debug.LogError("Failed to extract Tower's network ID");
            }
        }

        Tower nTower = towerObj.GetComponent<Tower>();
        nTower.floors = new Floor[height];
        if (BasicSpawner._runner.IsServer)
        {
            nTower.width = width;
            nTower.depth = depth;
        }
        GameManager.Instance.parentTower = nTower;

        nTower.floors = new Floor[height];

        for (int level = 0; level < height; level++)
        {
            CreateFloor(towerJObject[$"Floor{level}"], level, nTower);
        }
        
        Debug.LogWarning("Map generation for host should be done?");
        //GameManager.Instance.SpawnBot();
        GameManager.Instance.SpawnPuppetBot();
    }

    private void CreateFloor(JToken floorJObject, int floorIdx, Tower parentTower)
    {

        NetworkObject floorObj;
        // if this is server instance, spawn and initialzie the object
        if (BasicSpawner._runner.IsServer)
        {
            floorObj = BasicSpawner._runner.Spawn(floorPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
            Debug.Log("Floor index: " + floorIdx);
            floorIDs[floorIdx] = floorObj.Id;
        }
        //if this is not server instance, extract information from server
        else
        {
            NetworkId floorID = floorIDs[floorIdx];
            if (!BasicSpawner._runner.TryFindObject(floorID, out floorObj))
            {
                Debug.LogError("Failed to extract floor's network ID");
            }
        }
        /*
        GameObject floorObj = new GameObject();
        floorObj.name = $"Floor{floorIdx}";
        Floor nFloor = floorObj.AddComponent<Floor>();
        */
        floorObj.name = $"Floor{floorIdx}";
        Floor nFloor = floorObj.GetComponent<Floor>();
        floorObj.transform.parent = parentTower.gameObject.transform;
        nFloor.parentTower = parentTower;
        nFloor.floorNumber = floorIdx;
        nFloor.Cells = new GridCellBehavior[_width, _depth];
        
        List<JToken> floorGrids = floorJObject.ToList();
        Debug.Log("floorGridx.Count: " + floorGrids.Count);
        for (int gridIdx = 0; gridIdx < floorGrids.Count; gridIdx++)
        {
            CreateGrid(floorGrids[gridIdx], gridIdx, nFloor);
        }

        parentTower.floors[floorIdx] = nFloor;
        
        // create virtual camera for this floor
        GameObject nCam = Instantiate(floorCamPrefab,
            new Vector3((_width - 1.0f) / 2, (_depth + 1) * floorIdx + (_depth - 1.0f) / 2, -14.0f),
            quaternion.identity);
        CinemachineVirtualCamera virtualCamera = nCam.GetComponent<CinemachineVirtualCamera>();
        if (floorIdx == 0) virtualCamera.m_Priority = 100;
        else virtualCamera.m_Priority = 0;
        CameraManager.Instance.floorCams.Add(virtualCamera);
    }

    private void CreateGrid(JToken gridJObject, int gridIdx, Floor parentFloor)
    {
        int x = gridIdx % _width;
        //is this the y in the old version? yes - ziyu
        int z = gridIdx / _width;
        
        int floorOffsetY = parentFloor.floorNumber * (_depth + 1);
        
        //GameObject gridObj = Instantiate(tilePrefab, new Vector3(x, z + floorOffsetY, 0.0f), Quaternion.identity);
        NetworkObject gridObj;
        if (BasicSpawner._runner.IsServer)
        {
            gridObj = BasicSpawner._runner.Spawn(tilePrefab, new Vector3(x, z + floorOffsetY * parentFloor.floorNumber, 0.0f), Quaternion.identity);
            gridCellIDs[parentFloor.floorNumber, x, z] = gridObj.Id;
        }
        else
        {

            NetworkId gridCellID = gridCellIDs[parentFloor.floorNumber, x, z];
            if (!BasicSpawner._runner.TryFindObject(gridCellID, out gridObj))
            {
                Debug.LogError($"Failed to extract grid cell's network ID for cell [{x}, {z}] on floor {parentFloor.floorNumber}");
            }
        }


        gridObj.name = $"Grid {x},{z}";
        gridObj.GetComponent<SpriteRenderer>().color = (x + z + parentFloor.floorNumber) % 2 == 0 ? grid2DTheme.lightColor : grid2DTheme.darkColor;
        gridObj.transform.parent = parentFloor.gameObject.transform;

        //GridCellBehavior nGrid = gridObj.AddComponent<GridCellBehavior>();
        GridCellBehavior nGrid = gridObj.GetComponent<GridCellBehavior>();
        nGrid.parentFloor = parentFloor;
        nGrid.gridX = x;
        nGrid.gridZ = z;
        
        // add soil config
        string soilConfig = (string)gridJObject["soilConfig"];
        if (!_soilConfigs.ContainsKey(soilConfig)) CreateSoilConfig(soilConfig);
        nGrid.soilConfig = _soilConfigs[soilConfig];
        
        // set properties
        Vector4 nutrients = Vector4FromJTokenList(gridJObject["NutrientLevels"]["nutrients"].ToList());
        NutrientSolution nutrientSolution =
            new NutrientSolution((float)gridJObject["NutrientLevels"]["water"], nutrients);

        //not sure if this works in networked version (INetworkStruct)
        nGrid.NutrientLevels = nutrientSolution;
        
        // add all plants
        int plantIdx = 0;
        nGrid.plants = new List<PlantBehavior>();
        Transform plantsSlot = gridObj.transform.GetChild(1);
        JToken plants = gridJObject["Plants"];
        if (plants is JObject plantsObj)
        {
            foreach (var property in plantsObj.Properties())
            {
                JToken plantJToken = property.Value;
                CreatePlant(plantJToken, nGrid, plantsSlot.GetChild(plantIdx), plantIdx);
                plantIdx++;
                nGrid.plantCount++;
            }
        }

        parentFloor.Cells[x, z] = nGrid;
    }

    private void CreatePlant(JToken plantJToken, GridCellBehavior parentGrid, Transform plantSlot, int plantIdx)
    {
        string plantConfigName = (string)plantJToken["config"];
        if (!plantConfigs.ContainsKey(plantConfigName)) {
            if (!BasicSpawner._runner.IsServer)
            {
                Debug.LogError("Error: Client didn't get the required sprite");
            }
            else
            {
                CreatePlantConfig(plantConfigName);
            }
        }

        //GameObject plantObj = Instantiate(plantPrefab, plantSlot.position, quaternion.identity, plantSlot);
        NetworkObject plantObj;
        if (BasicSpawner._runner.IsServer)
        {
            plantObj = BasicSpawner._runner.Spawn(plantPrefab, plantSlot.position, Quaternion.identity);
            plantIDs[parentGrid.parentFloor.floorNumber, parentGrid.gridX, parentGrid.gridZ, plantIdx] = plantObj.Id;
        }
        else
        {
            NetworkId plantID = plantIDs[parentGrid.parentFloor.floorNumber, parentGrid.gridX, parentGrid.gridZ, plantIdx];
            if (!BasicSpawner._runner.TryFindObject(plantID, out plantObj))
            {
                Debug.LogError($"Failed to extract plant cell's network ID for cell [{parentGrid.gridX}, {parentGrid.gridZ}] on floor {parentGrid.parentFloor.floorNumber}");
                Debug.Log("plant ID: " + plantID);
            }

        }
        plantObj.transform.SetParent(plantSlot);
        PlantBehavior nPlant = plantObj.GetComponent<PlantBehavior>();
        nPlant.config = plantConfigs[plantConfigName];
        nPlant.SetInitialProperties(new PlantInitInfo(
            (float)plantJToken["rootMass"],
            (float)plantJToken["height"],
            (float)plantJToken["energyLevel"],
            (float)plantJToken["health"],
            (float)plantJToken["age"],
            (float)plantJToken["water"],
            Vector4FromJTokenList(plantJToken["nutrient"].ToList())));
        //DisplayReceivedSprite(nPlant.config.plantSprites[0]);

        nPlant.GetComponent<SpriteRenderer>().sprite = nPlant.config.plantSprites[0];
        nPlant.parentCell = parentGrid;

        parentGrid.plants.Add(nPlant);
    }

    private void CreatePlantConfig(string configName)
    {
        JToken planConfigJToken = _configJObject[configName];
        if (planConfigJToken == null)
        {
            Debug.LogError($"MapGenerator tries to retrieve config:{configName} but it does not exist");
            return;
        }
        
        PlantConfig plantConfig = ScriptableObject.CreateInstance<PlantConfig>();

        plantConfig.capacities = (float)planConfigJToken["capacities"];
        plantConfig.uptakeRate = (float)planConfigJToken["uptakeRate"];
        plantConfig.metabolismNeeds = Vector4FromJTokenList(planConfigJToken["metabolismNeeds"].ToList());
        plantConfig.metabolismWaterNeeds = (float)planConfigJToken["metabolismWaterNeeds"];
        plantConfig.metabolismFactor = Vector4FromJTokenList(planConfigJToken["metabolismFactor"].ToList());
        plantConfig.growthConsumptionRateLimit = Vector4FromJTokenList(planConfigJToken["growthConsumptionRateLimit"].ToList());
        plantConfig.growthFactor = Vector4FromJTokenList(planConfigJToken["growthFactor"].ToList());
        plantConfig.rootHeightTransition = (float)planConfigJToken["rootHeightTransition"];
        plantConfig.growthToleranceThreshold = (float)planConfigJToken["growthToleranceThreshold"];
        plantConfig.leachingEnergyThreshold = (float)planConfigJToken["leachingEnergyThreshold"];
        plantConfig.leachingFactor = Vector4FromJTokenList(planConfigJToken["leachingFactor"].ToList());
        plantConfig.onWaterCallbackBypass = Convert.ToBoolean((int)planConfigJToken["onWaterCallbackBypass"]);
        plantConfig.plantSprites = PlantToSpriteCapturer.Instance.CaptureAllStagesAtOnce();
        plantConfigs[configName] = plantConfig;
    }

    private void CreateSoilConfig(string configName)
    {
        JToken soilConfigJToken = _configJObject[configName];
        float water = (float)soilConfigJToken["capacities"];

        SoilConfig nSoilConfig = ScriptableObject.CreateInstance<SoilConfig>();
        nSoilConfig.drainTime = (float)soilConfigJToken["drainTime"];
        nSoilConfig.capacities = water;

        _soilConfigs[configName] = nSoilConfig;
    }

    private Vector4 Vector4FromJTokenList(List<JToken> list)
    {
        return new Vector4(
            (float)list[0], (float)list[1], 
            (float)list[2], (float)list[3]);
    }
}
