using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
public class MapGenerator : NetworkBehaviour
{
    /*
     * For the tower config, the goal is to design a bi-directional mapping
     * between game states and text representations, so that any state can be loaded
     * from or saved to a text config. Defining such a mapping would also help with
     * procedural generation. As we want the text representation to be as human-readable
     * as possible, csv format is chosen currently, for its ease of editing and viewing
     * in google sheet above all other reasons.
     */
    //TODO: Instead of assuming the tower config is in the right format, we should implement sanity checks
    [SerializeField] private TextAsset towerConfig;
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GridTheme grid2DTheme;

    [SerializeField] private List<GameObject> plantsPrefab;
    private Dictionary<Char, GameObject> _text2Plant;
    public static MapGenerator Instance;

    public NetworkId towerID;
    public NetworkId[] floorIDs;
    // 3D array: floorNum * gridX * gridY
    public NetworkId[,,] gridCellIDs;
    // 4D array: floorNum * gridX * gridY * plantNum
    public NetworkId[,,,] plantIDs;
    public bool mapUpdated;

    public bool towerIDReceived;
    public bool floorIDsReceived;
    public bool gridCellIDsReceived;
    public bool plantIDsReceived;

    /*
    //[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestNetworkIDs()
    {
        Debug.Log("Client requesting map IDs from the server.");

        if (BasicSpawner._runner.IsServer)
        {
            RPC_SendNetworkIDs();
        }
        else
        {
            mapUpdated = false;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SendNetworkIDs()
    {
        Debug.Log("Server sending map IDs to the client.");

        RPC_ReceiveNetworkIDs(towerID, floorIDs, gridCellIDs, plantIDs);
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ReceiveNetworkIDs(
        NetworkId towerID,
        NetworkId[] floorIDsArray,
        NetworkId[,,] gridCellIDsArray,
        NetworkId[,,,] plantIDsArray)
    {
        Debug.Log("Client received map IDs from the server.");

        this.towerID = towerID;
        this.floorIDs = floorIDsArray;
        this.gridCellIDs = gridCellIDsArray;
        this.plantIDs = plantIDsArray;

        mapUpdated = true;
    }
    */

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestNetworkIDs(PlayerRef requestingPlayer)
    {
        Debug.Log("Client requesting map IDs from the server.");

        if (BasicSpawner._runner.IsServer)
        {
            // Server sends data to the client using the data streaming method.
            SendNetworkData(requestingPlayer);
        }
       // else
       // {
       //     mapUpdated = false;
       // }
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

    /*
    public byte[] SerializeGridCellIDs(NetworkId[,,] ids)
    {
        // Convert the 3D array into bytes
        return System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Wrapper<NetworkId[,,]>(ids)));
    }

    public byte[] SerializePlantIDs(NetworkId[,,,] ids)
    {
        // Convert the 4D array into bytes
        return System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Wrapper<NetworkId[,,,]>(ids)));
    }
    */

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

    /*
    public NetworkId[,,] DeserializeGridCellIDs(ArraySegment<byte> data)
    {
        return JsonUtility.FromJson<Wrapper<NetworkId[,,]>>(System.Text.Encoding.UTF8.GetString(data)).Items;
    }

    public NetworkId[,,,] DeserializePlantIDs(ArraySegment<byte> data)
    {
        return JsonUtility.FromJson<Wrapper<NetworkId[,,,]>>(System.Text.Encoding.UTF8.GetString(data)).Items;
    }
    */
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




    private void Awake()
    {
        if (Instance) Destroy(this.gameObject);
        else Instance = this;
        if (plantsPrefab.Count != 4) Debug.LogError("test mode, please give 4 plant prefabs");
        _text2Plant = new Dictionary<Char, GameObject>
        {
            {'A', plantsPrefab[0]},
            {'B', plantsPrefab[1]},
            {'C', plantsPrefab[2]},
            {'D', plantsPrefab[3]},
        };
    }

    private void Start()
    {
        //CreateTower();
    }




    public void CreateTower()
    {
        string separator = towerConfig.text.Split("\n")[1];
        string[] towerInfo = towerConfig.text.Split(separator + "\nFloor");
        string towerHeight = towerInfo[0].Split(",")[1];
        
        Debug.Log($"Creating a tower with height {towerHeight}");
        //GameObject towerObj = new GameObject();
        NetworkObject towerObj;
        if (BasicSpawner._runner.IsServer)
        {
            // Clear ID lists if server is spawning the tower from scratch
            towerID = default;
            int floorCount = Int32.Parse(towerHeight);
            floorIDs = new NetworkId[floorCount];
            gridCellIDs = new NetworkId[4 ,6, 6];
            plantIDs = new NetworkId[4, 6, 6, 4];


            towerObj = BasicSpawner._runner.Spawn(towerPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
            towerID = towerObj.Id;
        }
        else
        {
            //find towerObj thorugh Network ID
            if (!BasicSpawner._runner.TryFindObject(towerID, out towerObj))
            {
                Debug.LogError("Failed to extract Tower's network ID");
            }
        }
        towerID = towerObj.Id;
        towerObj.name = "Tower";
        //Tower nTower = towerObj.AddComponent<Tower>();
        Tower nTower = towerObj.GetComponent<Tower>();
        //if(BasicSpawner._runner.IsServer)
        nTower.floors = new Floor[Int32.Parse(towerHeight)];
        GameManager.Instance.parentTower = nTower;
        for (int i = 1; i < towerInfo.Length; i++)
        {
            nTower.floors[i-1] = CreateFloor(towerInfo[i], i, nTower);
        }
    }

    private Floor CreateFloor(string floorInfo, int floorIdx, Tower parentTower)
    {
        Debug.Log(floorInfo);
        string[] floorData = floorInfo.Split("\n");
        string[] floorSize = floorData[0].Split(",")[1].Split("x");
        int floorSizeX = Int32.Parse(floorSize[0]);
        int floorSizeY = Int32.Parse(floorSize[1]);


        GameManager.Instance.parentTower.width = floorSizeX;
        GameManager.Instance.parentTower.depth = floorSizeY;


        
        
        Debug.Log($"Creating floor {floorIdx} with size {floorSizeX}x{floorSizeY}");
        //GameObject floorObj = new GameObject();
        NetworkObject floorObj;
        if (BasicSpawner._runner.IsServer)
        {
            floorObj = BasicSpawner._runner.Spawn(floorPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
            floorIDs[floorIdx - 1] = floorObj.Id;
        }
        else
        {
            NetworkId floorID = floorIDs[floorIdx - 1];
            if (!BasicSpawner._runner.TryFindObject(floorID, out floorObj))
            {
                Debug.LogError("Failed to extract floor's network ID");
            }
        }
        floorObj.name = $"Floor{floorIdx}";
        //Floor nFloor = floorObj.AddComponent<Floor>();
        Floor nFloor = floorObj.GetComponent<Floor>();
        floorObj.transform.parent = parentTower.gameObject.transform;
        nFloor.parentTower = parentTower;
        nFloor.floorNumber = floorIdx;
        nFloor.SizeX = floorSizeX;
        nFloor.SizeY = floorSizeY;
        nFloor.Cells = new GridCellBehavior[floorSizeX,floorSizeY];

        for (int y = 0; y < floorSizeY; y++)
        {
            string[] plantRowInfo = floorData[1 + y].Split(",");
            string[] waterRowInfo = floorData[1 + y + floorSizeY].Split(",");
            string[] nutritionRowInfo = floorData[1 + y + floorSizeY * 2].Split(",");
            for (int x = 0; x < floorSizeX; x++)
            {
                int xIdx = x + 1;
                string plant = plantRowInfo[xIdx];
                string water = waterRowInfo[xIdx];
                string nutrition = nutritionRowInfo[xIdx];
                nFloor.Cells[x,y] = CreateGrid(x, y, plant, water, nutrition, nFloor, floorSizeY + 1);
            }
        }

        return nFloor;
    }

    private GridCellBehavior CreateGrid(int x, int y, string plants, string water, string nutrition, Floor parentFloor, int floorOffsetY)
    {
        Debug.Log($"Creating grid {x}, {y} with plant: {plants}, water: {water}, and nutrition: {nutrition}");
        //GameObject gridObj = Instantiate(tilePrefab, new Vector3(x, y + floorOffsetY * parentFloor.floorNumber, 0.0f), Quaternion.identity);

        NetworkObject gridObj;
        if (BasicSpawner._runner.IsServer)
        {
            gridObj = BasicSpawner._runner.Spawn(tilePrefab, new Vector3(x, y + floorOffsetY * parentFloor.floorNumber, 0.0f), Quaternion.identity);
            gridCellIDs[parentFloor.floorNumber - 1, x, y] = gridObj.Id;
        }
        else
        {
            NetworkId gridCellID = gridCellIDs[parentFloor.floorNumber - 1, x, y];
            if (!BasicSpawner._runner.TryFindObject(gridCellID, out gridObj))
            {
                Debug.LogError($"Failed to extract grid cell's network ID for cell [{x}, {y}] on floor {parentFloor.floorNumber - 1}");
            }
        }
        gridObj.name = $"Grid {x},{y}";
        gridObj.GetComponent<SpriteRenderer>().color = (x + y + parentFloor.floorNumber) % 2 == 0 ? grid2DTheme.lightColor : grid2DTheme.darkColor;
        gridObj.transform.parent = parentFloor.gameObject.transform;

        //GridCellBehavior nGrid = gridObj.AddComponent<GridCellBehavior>();
        GridCellBehavior nGrid = gridObj.GetComponent<GridCellBehavior>();
        //nGrid.parentFloor = parentFloor;
        nGrid.gridX = x;
        nGrid.gridY = y;
        
        //TODO: maybe we want our infrastructure to support arbitrary plant in the same cell
        if (plants.Length > 4)
        {
            Debug.LogError("Current only support up to 4 plant in one cell");
            return nGrid;
        }
        Transform plantsSlot = gridObj.transform.GetChild(1);
        for (int i = 0; i < plants.Length; i++)
        {
            if (!Char.IsLetter(plants[i])) continue;
            Vector3 pos = plantsSlot.GetChild(i).position;
            //Instantiate(_text2Plant[plants[i]], pos, Quaternion.identity, plantsSlot.GetChild(i));
            NetworkObject spawnedObject;
            if (BasicSpawner._runner.IsServer)
            {
                spawnedObject = BasicSpawner._runner.Spawn(_text2Plant[plants[i]], pos, Quaternion.identity);
                plantIDs[parentFloor.floorNumber - 1, x, y, i] = spawnedObject.Id;
            }
            else
            {
                NetworkId plantID = plantIDs[parentFloor.floorNumber - 1, x, y, i];
                if (!BasicSpawner._runner.TryFindObject(plantID, out spawnedObject))
                {
                    Debug.LogError($"Failed to extract grid cell's network ID for cell [{x}, {y}] on floor {i + 1}");
                }
            }
            spawnedObject.transform.SetParent(plantsSlot.GetChild(i));
        }

        nGrid.NutrientLevels = new NutrientSolution(float.Parse(water));
        string[] nutrients = nutrition.Split("|");
        nGrid.NutrientLevels.nutrients = new Vector4(
            float.Parse(nutrients[0]),
            float.Parse(nutrients[1]),
            float.Parse(nutrients[2]),
            float.Parse(nutrients[3]));
        
        return nGrid;
    }



    //aborted -> should integrate client's logic inside creatTower function
    /*
    public IEnumerator ClientUpdateTower()
    {
        //Request NetworkIDS using RPC call
        if (!BasicSpawner._runner.IsServer)
        {
            MapGenerator.Instance.RPC_RequestNetworkIDs();
        }
        while (!BasicSpawner._runner.IsServer && !MapGenerator.Instance.mapUpdated)
        {
            yield return null;
        }

        //Assign tower refrence
        NetworkObject towerObj;
        if(!BasicSpawner._runner.TryFindObject(towerID, out towerObj))
        {
            Debug.LogError("Failed to extract Tower's network ID");
        }
        towerObj.name = "Tower";
        Tower nTower = towerObj.GetComponent<Tower>();
        GameManager.Instance.parentTower = nTower;

        //Assign floor references
        for(int i=0; i< floorIDs.Count; i++)
        {
            NetworkId floorId = floorIDs[i];
            NetworkObject floorObject;
            if (!BasicSpawner._runner.TryFindObject(floorId, out floorObject))
            {
                Debug.LogError("Failed to extract floor's network ID");
            }
            floorObject.name = $"Floor{i+1}";
            Floor nFloor = floorObject.GetComponent<Floor>();
            nTower.floors[i] = nFloor;
            nFloor.parentTower = nTower;
            nFloor.Cells = new GridCellBehavior[nFloor.SizeX, nFloor.SizeY];

            // Assign gridcell references
            for (int x = 0; x < gridCellIDs[i].Count; x++)  // Loop through grid rows
            {
                for (int y = 0; y < gridCellIDs[i][x].Count; y++)  
                {
                    NetworkId gridCellId = gridCellIDs[i][x][y];  
                    NetworkObject gridCellObject;
                    if (!BasicSpawner._runner.TryFindObject(gridCellId, out gridCellObject))
                    {
                        Debug.LogError($"Failed to extract grid cell's network ID for cell [{x}, {y}] on floor {i + 1}");
                        continue;
                    }
                    // Assign the grid cell to the nFloor.Cells array
                    gridCellObject.name = $"Grid {x},{y}";
                    gridCellObject.GetComponent<SpriteRenderer>().color = (x + y + nFloor.floorNumber) % 2 == 0 ? grid2DTheme.lightColor : grid2DTheme.darkColor;
                    gridCellObject.transform.parent = nFloor.gameObject.transform;
                    GridCellBehavior nGridCell = gridCellObject.GetComponent<GridCellBehavior>();
                    nFloor.Cells[x, y] = nGridCell;  // Assign the cell to its position in the array

                    //Assign plant references

                }
            }

        }


    }
     */



}
