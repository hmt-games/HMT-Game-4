using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeatherManager : MonoBehaviour
{
    public enum Weather { Sunny = 0, Cloudy = 1, Rainy = 2 };
    public enum Difficulty { Easy = 0, Medium = 1, Hard = 2 };

    [SerializeField]
    public Weather currentWeather;

    [SerializeField]
    public Difficulty currentDifficulty = Difficulty.Easy;

    [SerializeField]
    // Count for #day in the game
    private int CurrentDay = 0;
    [SerializeField]
    // Count down for day in a week. It can be some number other than 7, "week" here is only for the cycle of the weather pattern
    private static int Weekday = 7;
    private int Week_Countdown = 0;

    [SerializeField]
    private float DayTime = 5.0f;

    
    private List<int> WeatherBias_Balanced = new List<int>() { 7, 7, 7 };
    private Queue<int> WeatherBias_Hard = new Queue<int>(new List<int>() { 4, 4, 8 } );

    [SerializeField]
    private List<int> WeatherBias = new List<int>();

    [SerializeField]
    private List<Dictionary<int, int>> prediction = new List<Dictionary<int, int>>(new Dictionary<int, int>[7]);

    public TMP_Text _text;

    // Start is called before the first frame update
    void Start()
    {
        if (currentDifficulty == Difficulty.Easy)
        {
            WeatherBias = WeatherBias_Balanced;
        }
        else
        {
            Week_Countdown = Weekday;
            LoadWeatherBias();
        }

        PredictWeek();
        int rdm = UnityEngine.Random.Range(0, 3);
        currentWeather = (Weather)rdm;

        StartCoroutine(DayTick());
    }

    private void LoadWeatherBias()
    {
        if (currentDifficulty == Difficulty.Hard)
        {
            int first = WeatherBias_Hard.Dequeue();
            WeatherBias_Hard.Enqueue(first);

            WeatherBias.Clear();
            foreach (int value in WeatherBias_Hard)
            {
                WeatherBias.Add(value);
            }
        }
    }

    // Update is called once per frame
    private void PredictDay(int dayIndex)
    {
        int weather1 = -1;
        int weather2 = -1;

        // Pick the first possible weather for day x
        weather1 = GetRandomWeather();

        // Pick the second possible weather for day x
        weather2 = GetRandomWeather();

        
        Debug.Log("DayIndex: " + dayIndex);
        if (prediction[dayIndex] == null)
        {
            prediction[dayIndex] = new Dictionary<int, int>();
        }

        if (weather2 == weather1)
        {
            prediction[dayIndex][weather1] = 20;
        }
        else
        {
            int possibilty = GetRandomPossibilty();
            prediction[dayIndex][weather1] = possibilty;
            possibilty = 20 - possibilty;
            prediction[dayIndex][weather2] = possibilty;
        }

        if (currentDifficulty != Difficulty.Easy)
        {
            Week_Countdown -= 1;
        }
    }

    private int GetRandomWeather()
    {
        int range = 0;
        int weather = -1;

        foreach (int bias in WeatherBias)
        {
            range += bias;
        }

        int rdm = UnityEngine.Random.Range(0, range);

        for (int i = 0; i < WeatherBias.Count; i++)
        {
            if (rdm < WeatherBias[i])
            {
                weather = i;
                break;
            }
            else
            {
                rdm -= WeatherBias[i];
            }
        }

        WeatherBias[weather] -= 1;

        return weather;
    }

    private int GetRandomPossibilty()
    {
        int possibilty = 0;
        possibilty = UnityEngine.Random.Range(1, 20);

        return possibilty;
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
        Broadcast();
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

        // Based on difficulty level, see if we need to change the weather bias
        if (currentDifficulty == Difficulty.Easy)
        {
            if (weatherIndex.Count == 1)
            {
                foreach (int key in weatherIndex)
                {
                    WeatherBias[key] += 2;
                }
            }
            else
            {
                foreach (int key in weatherIndex)
                {
                    WeatherBias[key] += 1;
                }
            }
        }
        else
        {
            Debug.Log("Bias Changed");
            if (Week_Countdown == 0)
            {
                Week_Countdown = Weekday;
                LoadWeatherBias();
            }
        }

        for (int i = 0; i < prediction.Count - 1; i++)
        {
            prediction[i].Clear();

            foreach (int key in prediction[i+1].Keys)
            {
                prediction[i][key] = prediction[i + 1][key];
            }
        }

        // Predict one more day
        prediction[6].Clear();
        PredictDay(6);

        StartCoroutine(DayTick());
    }

    private void Broadcast()
    {
        Debug.Log("Current Weather: " + currentWeather);
        _text.text = "";

        int j = CurrentDay + 1;
        foreach (Dictionary<int, int> dictionary in prediction)
        {
            _text.text += "Day " + j + "\n";
            foreach (KeyValuePair<int, int> valuePair in dictionary)
            {
                _text.text += ("Weather: " + valuePair.Key + " Percentage: " + valuePair.Value + "\n");
            }

            j += 1;
        }

        Debug.Log("WeatherBias: " + WeatherBias[0] + WeatherBias[1] + WeatherBias[2]);
    }
}