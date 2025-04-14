/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// modified from Code Monkey's basic line plotter
// Credit (original code): https://unitycodemonkey.com/video.php?v=4nwAtbhsLEg
public class Window_Graph : MonoBehaviour
{
    [SerializeField] private int IDX = 0;
    [SerializeField] private List<Sprite> circleSprite;
    [SerializeField] private List<Color> spriteColor;
    [SerializeField] private RectTransform graphContainer;
    [SerializeField] private RectTransform labelTemplateX;
    [SerializeField] private RectTransform labelTemplateY;
    [SerializeField] private RectTransform dashTemplateX;
    [SerializeField] private RectTransform dashTemplateY;
    private List<GameObject> gameObjectList;

    private int _spriteIdx = 0;

    public static Window_Graph Instance0;
    public static Window_Graph Instance1;
    
    private void Awake()
    {
        if (IDX == 0) Instance0 = this;
        else Instance1 = this;

        gameObjectList = new List<GameObject>();

        //List<int> valueList = new List<int>() { 5, 98, 56, 45, 30, 22, 17, 15, 13, 17, 25, 37, 40, 36, 33, 50, 30, 60, 50, 40, 20, 5, 20, 10, 50, 30, 20, 11 };
        //List<float> valueList = new List<float>() { 5};
        //ShowGraph(valueList, -1, (int _i) => "Day " + (_i + 1), (float _f) => "$" + Mathf.RoundToInt(_f));
    }

    public void ShowGraph(List<float> valueList, int maxVisibleValueAmount = -1, Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null, float yMin = float.MaxValue, float yMax = float.MinValue) {
        if (getAxisLabelX == null) {
            getAxisLabelX = delegate (int _i) { return _i.ToString(); };
        }
        if (getAxisLabelY == null) {
            getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };
        }

        if (maxVisibleValueAmount <= 0) {
            maxVisibleValueAmount = valueList.Count;
        }

        foreach (GameObject gameObject in gameObjectList) {
            Destroy(gameObject);
        }
        gameObjectList.Clear();
        
        float graphWidth = graphContainer.sizeDelta.x;
        float graphHeight = graphContainer.sizeDelta.y;

        float yMaximum = yMax;
        float yMinimum = yMin;
        
        for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++) {
            float value = valueList[i];
            if (value > yMaximum) {
                yMaximum = value;
            }
            if (value < yMinimum) {
                yMinimum = value;
            }
        }

        float yDifference = yMaximum - yMinimum;
        if (yDifference <= 0) {
            yDifference = 5f;
        }
        yMaximum += (yDifference * 0.2f);
        yMinimum -= (yDifference * 0.2f);

        yMinimum = 0f; // Start the graph at zero

        float xSize = graphWidth / (maxVisibleValueAmount + 1);

        int xIndex = 0;

        GameObject lastCircleGameObject = null;
        for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++) {
            float xPosition = xSize + xIndex * xSize;
            float yPosition = ((valueList[i] - yMinimum) / (yMaximum - yMinimum)) * graphHeight;
            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition));
            gameObjectList.Add(circleGameObject);
            if (lastCircleGameObject != null) {
                GameObject dotConnectionGameObject = CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition);
                gameObjectList.Add(dotConnectionGameObject);
            }
            lastCircleGameObject = circleGameObject;

            if (IDX == 0)
            {
                RectTransform labelX = Instantiate(labelTemplateX);
                labelX.SetParent(graphContainer, false);
                labelX.gameObject.SetActive(true);
                labelX.anchoredPosition = new Vector2(xPosition, -15f);
                labelX.GetComponent<Text>().text = getAxisLabelX(i);
                gameObjectList.Add(labelX.gameObject);

                RectTransform dashX = Instantiate(dashTemplateX);
                dashX.SetParent(graphContainer, false);
                dashX.gameObject.SetActive(true);
                dashX.anchoredPosition = new Vector2(xPosition, -3f);
                gameObjectList.Add(dashX.gameObject);
            }

            xIndex++;
        }

        int separatorCount = 10;
        for (int i = 0; i <= separatorCount; i++) {
            RectTransform labelY = Instantiate(labelTemplateY);
            labelY.SetParent(graphContainer, false);
            labelY.gameObject.SetActive(true);
            float normalizedValue = i * 1f / separatorCount;
            labelY.anchoredPosition = new Vector2(IDX == 0 ? -10 : 2424, normalizedValue * graphHeight);
            labelY.GetComponent<Text>().text = getAxisLabelY(yMinimum + (normalizedValue * (yMaximum - yMinimum)));
            gameObjectList.Add(labelY.gameObject);

            if (IDX == 0)
            {
                RectTransform dashY = Instantiate(dashTemplateY);
                dashY.SetParent(graphContainer, false);
                dashY.gameObject.SetActive(true);
                dashY.anchoredPosition = new Vector2(-4f, normalizedValue * graphHeight);
                gameObjectList.Add(dashY.gameObject);
            }
        }

        _spriteIdx = 0;
    }

    public void ShowGraphOnTop(List<float> valueList, int maxVisibleValueAmount = -1, float yMin = float.MaxValue, float yMax = float.MinValue)
    {
        if (maxVisibleValueAmount <= 0) {
            maxVisibleValueAmount = valueList.Count;
        }

        float graphWidth = graphContainer.sizeDelta.x;
        float graphHeight = graphContainer.sizeDelta.y;

        float yMaximum = yMax;
        float yMinimum = yMin;
        
        for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++) {
            float value = valueList[i];
            if (value > yMaximum) {
                yMaximum = value;
            }
            if (value < yMinimum) {
                yMinimum = value;
            }
        }

        float yDifference = yMaximum - yMinimum;
        if (yDifference <= 0) {
            yDifference = 5f;
        }
        yMaximum += (yDifference * 0.2f);
        yMinimum -= (yDifference * 0.2f);

        yMinimum = 0f; // Start the graph at zero

        float xSize = graphWidth / (maxVisibleValueAmount + 1);

        int xIndex = 0;

        GameObject lastCircleGameObject = null;
        for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++) {
            float xPosition = xSize + xIndex * xSize;
            float yPosition = ((valueList[i] - yMinimum) / (yMaximum - yMinimum)) * graphHeight;
            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition), _spriteIdx, false);
            gameObjectList.Add(circleGameObject);
            if (lastCircleGameObject != null) {
                GameObject dotConnectionGameObject = CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition, false);
                gameObjectList.Add(dotConnectionGameObject);
            }
            lastCircleGameObject = circleGameObject;

            xIndex++;
        }

        _spriteIdx++;
    }

    private GameObject CreateCircle(Vector2 anchoredPosition, int spriteIdx = 0, bool mainPlot = true) {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite[spriteIdx];
        Color mainColor = IDX == 0 ? Color.white : Color.black;
        gameObject.GetComponent<Image>().color = mainPlot ? mainColor : spriteColor[spriteIdx];
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(30, 30);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        return gameObject;
    }

    private GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB, bool mainPlot = true) {
        GameObject gameObject = new GameObject("dotConnection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        Color mainColor = IDX == 0 ? Color.white : Color.black;
        gameObject.GetComponent<Image>().color = mainPlot ? mainColor : spriteColor[_spriteIdx];
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 3f);
        rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, GetAngleFromVectorFloat(dir));
        return gameObject;
    }
    
    public static float GetAngleFromVectorFloat(Vector3 dir) {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;

        return n;
    }

}
