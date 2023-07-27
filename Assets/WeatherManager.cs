using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeatherManager : MonoBehaviour
{
    public enum Weather { Sunny = 0, Cloudy = 1, Rainy = 2 };

    [SerializeField]
    public Weather currentWeather;
    [SerializeField]
    private int CurrentDay = 0;

    [SerializeField]
    private float DayTime = 10.0f;

    [SerializeField]
    private List<int> WeatherBias = new List<int>() { 7, 7, 7 };

    [SerializeField]
    private List<Dictionary<int, int>> prediction = new List<Dictionary<int, int>>(new Dictionary<int, int>[7]);

    public TMP_Text _text;

    // Start is called before the first frame update
    void Start()
    {
        PredictWeek();
        int rdm = UnityEngine.Random.Range(0, 3);
        currentWeather = (Weather)rdm;

        StartCoroutine(DayTick());
    }

    // Update is called once per frame
    private void PredictDay(int dayIndex)
    {
        int range = 0;
        int possibilty = 0;
        int weather1 = -1;
        int weather2 = -1;

        // Pick the first possible weather for day x
        foreach (int bias in WeatherBias)
        {
            range += bias;
        }

        int rdm = UnityEngine.Random.Range(0, range);

        for (int i = 0; i < WeatherBias.Count; i++)
        {
            if (rdm < WeatherBias[i])
            {
                weather1 = i;
                break;
            }
            else
            {
                rdm -= WeatherBias[i];
            }
        }

        possibilty = UnityEngine.Random.Range(1, 20);
        Debug.Log("DayIndex: " + dayIndex);
        if (prediction[dayIndex] == null)
        {
            prediction[dayIndex] = new Dictionary<int, int>();
        }

        prediction[dayIndex][weather1] = possibilty;
        possibilty = 20 - possibilty;

        WeatherBias[weather1] -= 1;

        // Pick the second possible weather for day x
        range = 0;
        foreach (int bias in WeatherBias)
        {
            range += bias;
        }

        rdm = UnityEngine.Random.Range(0, range);

        for (int i = 0; i < WeatherBias.Count; i++)
        {
            if (rdm < WeatherBias[i])
            {
                weather2 = i;
                break;
            }
            else
            {
                rdm -= WeatherBias[i];
            }
        }

        if (weather2 == weather1)
        {
            prediction[dayIndex][weather1] = 20;
        }
        else
        {
            prediction[dayIndex][weather2] = possibilty;

            WeatherBias[weather2] -= 1;
        }
    }

    private void PredictWeek()
    {
        for(int i = 0; i < 7; i++)
        {
            PredictDay(i);
        }
    }

    public IEnumerator DayTick()
    {
        yield return new WaitForSeconds(DayTime);
        CurrentDay += 1;

        // Decide the weather of the new day based on the precentage
        List<int> weatherIndex = new List<int>();
        List<int> weatherProp = new List<int>();

        Debug.Log(prediction[0].Count);

        foreach (KeyValuePair<int, int> valuePair in prediction[0])
        {
            weatherIndex.Add(valuePair.Key);
            weatherProp.Add(valuePair.Value);
        }

        if (weatherIndex.Count == 1)
        {
            currentWeather = (Weather)weatherIndex[0];
        }
        else
        {
            int possibilty = UnityEngine.Random.Range(0, 20);
            if (possibilty < weatherProp[0])
            {
                currentWeather = (Weather)weatherIndex[0];
            }
            else
            {
                currentWeather = (Weather)weatherIndex[1];
            }
        }

        foreach(int key in weatherIndex)
        {
            WeatherBias[key] += 1;
        }

        for (int i = 0; i < prediction.Count - 1; i++)
        {
            prediction[i] = prediction[i+1];
        }

        PredictDay(6);

        Debug.Log("Current Weather: " + currentWeather);
        _text.text = "";

        foreach(Dictionary<int, int> dictionary in prediction)
        {
            _text.text += "Day " + "/n";
            foreach (KeyValuePair<int, int> valuePair in dictionary)
            {
                _text.text += ("Weather: " + valuePair.Key + " Percentage: " + valuePair.Value);
            }

        }

        StartCoroutine(DayTick());
    }
}