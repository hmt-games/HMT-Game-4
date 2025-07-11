using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PrefabPooler : MonoBehaviour {

    public static PrefabPooler Instance { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    public Vector3 poolPosition = new Vector3(-9999, -9999, -9999); // Default position for pooled objects

    [SerializeField]
    private List<PrefabPoolSpec> prefabPools;

    [Serializable]
    public class PrefabPoolSpec {
        public string prefabName;
        public GameObject prefab;
        [Min(0)]
        public int initialPoolSize;
    }

    private Dictionary<string, Queue<GameObject>> prefabPoolDictionary;

    // Start is called before the first frame update
    void Start() {
        prefabPoolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (PrefabPoolSpec spec in prefabPools) {
            Queue<GameObject> objectQueue = new Queue<GameObject>();
            for (int i = 0; i < spec.initialPoolSize; i++) {
                GameObject instance = Instantiate(spec.prefab, poolPosition, Quaternion.identity);
                instance.SetActive(false);
                objectQueue.Enqueue(instance);
            }
            prefabPoolDictionary[spec.prefabName] = objectQueue;
        }
    }

    public GameObject InstantiatePrefab(string prefabName) {
        return InstantiatePrefab(prefabName, poolPosition, Quaternion.identity);
    }

    public GameObject InstantiatePrefab(string prefabName, Vector3 position, Quaternion rotation) {
        if (!prefabPoolDictionary.ContainsKey(prefabName)) {
            throw new KeyNotFoundException($"Prefab '{prefabName}' not found in pool dictionary. Make sure to initialize the pool first.");
        }

        if (prefabPoolDictionary[prefabName].Count > 0) {
            GameObject instance = prefabPoolDictionary[prefabName].Dequeue();
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.SetActive(true);
            foreach(var poolListener in instance.GetComponents<IPoolCallbacks>()) {
                poolListener.OnInstantiateFromPool();
            }

            return instance;
        }
        else {
            PrefabPoolSpec spec = prefabPools.Find(p => p.prefabName == prefabName);
            if (spec != null) {
                GameObject newInstance = Instantiate(spec.prefab, position, rotation);
                return newInstance;
            }
            else {
                Debug.LogError($"Prefab '{prefabName}' not found in pool.");
                return null;
            }
        }
    }

    //There is probably a safer way to handle this to make sure an prefab gets put back into the correct pool
    public void ReleasePrefabInstance(string prefabName, GameObject instance) {
        if (!prefabPoolDictionary.ContainsKey(prefabName)) {
            Debug.LogError($"Prefab '{prefabName}' not found in pool dictionary. Cannot release instance.");
            return;
        }
        foreach(var poolListener in instance.GetComponents<IPoolCallbacks>()) {
            poolListener.OnReleaseToPool();
        }
        instance.SetActive(false);
        instance.transform.position = poolPosition; // Reset position
        prefabPoolDictionary[prefabName].Enqueue(instance);
    }


}

//Custom PropertyDrawer for PrefabPoolSpec
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(PrefabPooler.PrefabPoolSpec))]
public class PrefabPoolSpecDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(property.FindPropertyRelative("prefabName"),new GUIContent(string.Empty,"Prefab Name, used to find prefabs in the pool"), GUILayout.MinWidth(100));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("prefab"), new GUIContent(string.Empty, "Prefab Reference"), GUILayout.MinWidth(100));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("initialPoolSize"), new GUIContent(string.Empty, "Initial Pool Size"), GUILayout.Width(50));

        EditorGUILayout.EndHorizontal();
        //var nameRect = new Rect(position.x, position.y, 50, position.height);
        //var prefabRect = new Rect(position.x + 55, position.y, 40, position.height);
        //var sizeRect = new Rect(position.x + 100, position.y, 30, position.height);

        //// Draw the prefab name field
        //EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("prefabName"), GUIContent.none);
        //// Draw the prefab field
        //EditorGUI.PropertyField(prefabRect, property.FindPropertyRelative("prefab"), GUIContent.none);
        //// Draw the initial pool size field
        //EditorGUI.PropertyField(sizeRect, property.FindPropertyRelative("initialPoolSize"), GUIContent.none);

        

        EditorGUI.EndProperty();
    }
}


#endif

