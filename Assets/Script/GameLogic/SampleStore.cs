using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// For now this is a very dumb implementation. Ideally we'd have some kind of DB or data stream for outside users
/// </summary>
public class SampleStore : MonoBehaviour
{
public static SampleStore Instance { get; private set; }

    //TODO currentlty this is holding on to all samples, we don't need to maintain this duplicate list
    private List<SoilSample> allSamples = new List<SoilSample>();

    private Dictionary<Vector3Int, List<SoilSample>> positionedSamples = new Dictionary<Vector3Int, List<SoilSample>>();

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            allSamples = new List<SoilSample>();
        }
        else {
            Destroy(gameObject);
        }
    }

    public void AddSample(SoilSample sample) {
        allSamples.Add(sample);
        if (!positionedSamples.ContainsKey(sample.tileAddress)) {
            positionedSamples[sample.tileAddress] = new List<SoilSample>();
        }
        positionedSamples[sample.tileAddress].Add(sample);

        if (positionedSamples[sample.tileAddress].Count > 2) {
            // TODO Run Observation Agent logic here
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
