using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeatherSystem : MonoBehaviour
{
    public Weather Easy;
    public Weather Medium;
    public Weather Hard;

    public enum WeatherCondition { Sunny = 0, Cloudy = 1, Rainy = 2 };

    public WeatherCondition currentWeather;
    public Weather currentDifficulty;

    [SerializeField]
    // Count for #day in the game
    private int CurrentDay = 0;
    [SerializeField]
    // Count down for day in a week. It can be some number other than 7, "week" here is only for the cycle of the weather pattern
    private static int Weekday = 7;
    private int Week_Countdown = 7;
    [SerializeField]
    // Count for #week in the game. Medium and Hard will appear in later of the game
    private int CurrentWeek = 0;

    [SerializeField]
    private float DayTime = 5.0f;

    private Queue<Dictionary<int, int>> forecast = new Queue<Dictionary<int, int>>();
    private Queue<int> result = new Queue<int>();

    public TMP_Text _text;

    // Start is called before the first frame update
    void Start()
    {
        currentDifficulty = Easy;

        PredictWeek();

        StartCoroutine(Ticking());
    }

    private void PredictWeek()
    {
        // Based on the difficulty level, predict the first day of the week based on the start probability
        int previousDayResult = PredictDay(currentDifficulty.start_Probabilities);
        result.Enqueue(previousDayResult);

        for (int i = 0; i < 6; i ++)
        {
            switch (previousDayResult)
            {
                case 0:
                    previousDayResult = PredictDay(currentDifficulty.sunny_TransitMatrix);
                    break;
                case 1:
                    previousDayResult = PredictDay(currentDifficulty.cloudy_TransitMatrix);
                    break;
                case 2:
                    previousDayResult = PredictDay(currentDifficulty.rainy_TransitMatrix);
                    break;
            }
            result.Enqueue(previousDayResult);
        }
    }

    private int PredictDay(List<float> probability)
    {
        int weather1 = -1;
        int weather2 = -1;
        int weatherResult = -1;

        // Pick the first possible weather for day x
        weather1 = GetRandomWeather(probability);

        // Pick the second possible weather for day x
        weather2 = GetRandomWeather(probability);

        if (weather2 == weather1)
        {
            Dictionary<int, int> newForecast = new Dictionary<int, int>() { { weather1, 20 } };
            forecast.Enqueue(newForecast);

            weatherResult = weather1;
        }
        else
        {
            int possibility1 = GetRandomPossibilty();
            int possibility2 = 20 - possibility1;
            Dictionary<int, int> newForecast = new Dictionary<int, int>() { { weather1, possibility1 }, { weather2, possibility2 } };
            forecast.Enqueue(newForecast);

            int possibility = GetRandomPossibilty();
            if (possibility < possibility1)
            {
                weatherResult = weather1;
            }
            else
            {
                weatherResult = weather2;
            }
        }

        return weatherResult;
    }

    private int GetRandomWeather(List<float> probability)
    {
        int weather = -1;

        float rdm = UnityEngine.Random.Range(0.0f, 1.0f);

        for (int i = 0; i < probability.Count; i++)
        {
            if (rdm < probability[i])
            {
                weather = i;
                break;
            }
            else
            {
                rdm -= probability[i];
            }
        }

        return weather;
    }

    private int GetRandomPossibilty()
    {
        int possibility = 0;
        possibility = UnityEngine.Random.Range(1, 20);

        return possibility;
    }

    public void DayTick()
    {
        if (Week_Countdown >= Weekday)
        {
            // Week + 1
            CurrentWeek += 1;

            // Switch difficulty level if needed
            if (CurrentWeek%5 == 0)
            {
                currentDifficulty = Hard;
            }else if (CurrentWeek%2 == 0)
            {
                currentDifficulty = Medium;
            }
            else
            {
                currentDifficulty = Easy;
            }

            // Predict a new week
            PredictWeek();

            // Clear count down
            Week_Countdown = 0;
        }

        CurrentDay += 1;
        Week_Countdown += 1;

        currentWeather = (WeatherCondition)result.Dequeue();
        forecast.Dequeue();
    }

    public IEnumerator Ticking()
    {
        DayTick();
        Broadcast();
        yield return new WaitForSeconds(DayTime);

        StartCoroutine(Ticking());
    }

    private void Broadcast()
    {
        Debug.Log("Current Weather: " + currentWeather);
        _text.text = "";

        Dictionary<int, int>[] arr = forecast.ToArray();
        int j = CurrentDay + 1;
        for(int i = 0; i < 7; i++)
        {
            _text.text += "Day " + j + "\n";

            Dictionary<int, int> dictionary = arr[i];
            foreach (KeyValuePair<int, int> valuePair in dictionary)
            {
                _text.text += ("Weather: " + valuePair.Key + " Percentage: " + valuePair.Value + "\n");
            }

            j += 1;
        }
    }
}
