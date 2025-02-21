//using System.Collections;
//using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{

    //private NetworkRunner _runner;
    public static NetworkRunner _runner;
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    [SerializeField] private NetworkPrefabRef _plantPrefab;

    async void StartGame(GameMode mode)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;


        // Create the NetworkSceneInfo from the current scene
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        StartCoroutine(InitializeMap());

    }



    private IEnumerator InitializeMap()
    {
        yield return new WaitForSeconds(1f);

        if (!_runner.IsServer)
        {
            // Request network IDs and pass the client's PlayerRef to the server
            MapGeneratorJSON.Instance.RPC_RequestNetworkIDs(_runner.LocalPlayer);

            MapGeneratorJSON.Instance.mapUpdated = false;
            MapGeneratorJSON.Instance.towerIDReceived = false;
            MapGeneratorJSON.Instance.floorIDsReceived = false;
            MapGeneratorJSON.Instance.gridCellIDsReceived = false;
            MapGeneratorJSON.Instance.plantIDsReceived = false;
            MapGeneratorJSON.Instance.soilConfigReceived = false;
            MapGeneratorJSON.Instance.plantConfigReceived = false;

            // Wait for all data to be received via streaming
            while (!MapGeneratorJSON.Instance.mapUpdated)
            {
                yield return null;
            }
        }

        //TODO: ?? replace this with wait for some event ??
        yield return new WaitForSeconds(1f);

        // Now that all data is received, create the tower and initialize the map
        MapGeneratorJSON.Instance.CreateTower();
        DataVisualization.Instance.Init();
        GameActionGoldenFinger.Instance.Init();
        //StartCoroutine(RepeatedlyUpdateHeatmap());
    }

    private IEnumerator RepeatedlyUpdateHeatmap()
    {
        while (true)
        {
            HeatMapSwicher.S.SwitchOnHeatMap();
            yield return new WaitForSeconds(5f);
        }

    }



    private void OnGUI()
    {
        if (_runner == null)
        {
            if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
            {
                StartGame(GameMode.Host);
            }
            if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
            {
                StartGame(GameMode.Client);
            }
        }
    }


    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        if (Input.GetKey(KeyCode.W))
            data.direction += Vector3.forward;

        if (Input.GetKey(KeyCode.S))
            data.direction += Vector3.back;

        if (Input.GetKey(KeyCode.A))
            data.direction += Vector3.left;

        if (Input.GetKey(KeyCode.D))
            data.direction += Vector3.right;

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            // Create a unique position for the player
            /*
            Debug.Log("New player spawned-------------------");
            Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3, 1, 0);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            // Keep track of the player avatars for easy access
            _spawnedCharacters.Add(player, networkPlayerObject);
            */
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        /*
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
        */
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        Debug.Log("OnReliableDataReceived triggered");

        /*
        int floorCount = 20;  // Example
        int gridX = 8;
        int gridY = 8;
        int plantCount = 5;
        */
        int floorCount = MapGeneratorJSON.Instance._height;  
        int gridX = MapGeneratorJSON.Instance._width;
        int gridY = MapGeneratorJSON.Instance._depth;
        int plantCount = 5;



        // Extract the integers from the key for comparison
        int key0, key1, key2, key3;
        key.GetInts(out key0, out key1, out key2, out key3);

        // Check if the key matches the expected towerID key
        var towerKey = ReliableKey.FromInts(0, 0, 0, 0);
        int tKey0, tKey1, tKey2, tKey3;
        towerKey.GetInts(out tKey0, out tKey1, out tKey2, out tKey3);

        if (key0 == tKey0 && key1 == tKey1 && key2 == tKey2 && key3 == tKey3)
        {
            // Deserialize and assign tower ID
            Debug.Log("Deserialize tower: " + MapGeneratorJSON.Instance.DeserializeNetworkID(data));
            MapGeneratorJSON.Instance.towerID = MapGeneratorJSON.Instance.DeserializeNetworkID(data);
            Debug.Log("Received tower ID");
            MapGeneratorJSON.Instance.towerIDReceived = true;
        }

        // Check if the key matches the expected floorIDs key
        var floorKey = ReliableKey.FromInts(1, 0, 0, 0);
        int fKey0, fKey1, fKey2, fKey3;
        floorKey.GetInts(out fKey0, out fKey1, out fKey2, out fKey3);

        if (key0 == fKey0 && key1 == fKey1 && key2 == fKey2 && key3 == fKey3)
        {
            Debug.Log("Deserialize floor: " + MapGeneratorJSON.Instance.DeserializeNetworkIDs(data));
            MapGeneratorJSON.Instance.floorIDs = MapGeneratorJSON.Instance.DeserializeNetworkIDs(data);
            Debug.Log("Received floor IDs");
            MapGeneratorJSON.Instance.floorIDsReceived = true;
        }

        // Check for gridCellIDs key
        var gridCellKey = ReliableKey.FromInts(2, 0, 0, 0);
        gridCellKey.GetInts(out fKey0, out fKey1, out fKey2, out fKey3);

        if (key0 == fKey0 && key1 == fKey1 && key2 == fKey2 && key3 == fKey3)
        {
            Debug.Log("Deserialize Grid cell: " + MapGeneratorJSON.Instance.DeserializeGridCellIDs(data, floorCount, gridX, gridY));
            MapGeneratorJSON.Instance.gridCellIDs = MapGeneratorJSON.Instance.DeserializeGridCellIDs(data, floorCount, gridX, gridY);
            Debug.Log("Received grid cell IDs");
            MapGeneratorJSON.Instance.gridCellIDsReceived = true;
        }

        // Check for plantIDs key
        var plantKey = ReliableKey.FromInts(3, 0, 0, 0);
        plantKey.GetInts(out fKey0, out fKey1, out fKey2, out fKey3);

        if (key0 == fKey0 && key1 == fKey1 && key2 == fKey2 && key3 == fKey3)
        {
            Debug.Log("Deserialize plants: " + MapGeneratorJSON.Instance.DeserializePlantIDs(data, floorCount, gridX, gridY, plantCount));
            MapGeneratorJSON.Instance.plantIDs = MapGeneratorJSON.Instance.DeserializePlantIDs(data, floorCount, gridX, gridY, plantCount);
            Debug.Log("Received plant IDs");
            MapGeneratorJSON.Instance.plantIDsReceived = true;
        }


        // Plant Configs
        var plantConfigKey = ReliableKey.FromInts(4, 0, 0, 0);
        if (key.Equals(plantConfigKey))
        {
            MapGeneratorJSON.Instance.DeserializePlantConfigs(data.Array);
            Debug.Log("Received and deserialized Plant Configs.");
            MapGeneratorJSON.Instance.plantConfigReceived = true;
        }

        // Soil Configs
        var soilConfigKey = ReliableKey.FromInts(5, 0, 0, 0);
        if (key.Equals(soilConfigKey))
        {
            MapGeneratorJSON.Instance.DeserializeSoilConfigs(data.Array);
            Debug.Log("Received and deserialized Soil Configs.");
            MapGeneratorJSON.Instance.soilConfigReceived = true;
        }


        // Update the mapUpdated flag when all IDs are received
        if (MapGeneratorJSON.Instance.towerIDReceived && MapGeneratorJSON.Instance.floorIDsReceived
            && MapGeneratorJSON.Instance.gridCellIDsReceived && MapGeneratorJSON.Instance.plantIDsReceived
            && MapGeneratorJSON.Instance.plantConfigReceived && MapGeneratorJSON.Instance.soilConfigReceived)
        {
            MapGeneratorJSON.Instance.mapUpdated = true;
        }
        Debug.Log("MapGenerator.Instance.mapUpdated: " + MapGeneratorJSON.Instance.mapUpdated);
    }


    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    //[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    [Rpc]
    public void RPC_PlantNextStage()
    {
        Debug.Log("Plant sprite goes to next stage");
        GameManager.Instance.PlantNextStage();
    }

    public void CallPlantNextStage()
    {
        if (BasicSpawner._runner.IsServer)
        {
            RPC_PlantNextStage();
        }
        else
        {
            Debug.LogWarning("Only the server can trigger plant growth.");
            RPC_PlantNextStage();
        }
    }



}
