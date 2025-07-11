using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// For now this is a very dumb implementation. Ideally we'd have some kind of DB or data stream for outside users
/// </summary>
public class SampleStore : MonoBehaviour
{
public static SampleStore Instance { get; private set; }

    private List<SoilSample> samples = new List<SoilSample>();

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            samples = new List<SoilSample>();
        }
        else {
            Destroy(gameObject);
        }
    }

    public void AddSample(SoilSample sample) {
        samples.Add(sample);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
