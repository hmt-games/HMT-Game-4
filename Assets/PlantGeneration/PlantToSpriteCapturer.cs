using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlantToSpriteCapturer : MonoBehaviour
{
    [SerializeField] private Camera captureCamera;
    [SerializeField] private GameObject targetSpriteObject;
    [SerializeField] private int iterations = 4;
    [SerializeField] private Plants2d plants2d;
    [SerializeField] private int TestQuadAmount = 1000;

    public static PlantToSpriteCapturer Instance;
    
    private List<Sprite> _capturedSprite;

    private void Awake()
    {
        if (Instance)
        {
            Debug.LogWarning("There should never be more than 1 PlantToSpriteCapturer in scene");
            Destroy(gameObject);
        }
        else Instance = this;
    }

    public void Capture()
    {
        StartCoroutine(CaptureHelper());
    }

    public List<Sprite> CaptureAllStagesAtOnce()
    {
        captureCamera.gameObject.SetActive(true);
        
        float posStep = Random.Range(999f, 9999f);
        List<PlantToSpriteCamera> plantToSpriteCameras = new List<PlantToSpriteCamera>();
        plantToSpriteCameras.Add(targetSpriteObject.GetComponentInChildren<PlantToSpriteCamera>());

        List<GameObject> nPlants = new List<GameObject>();

        Vector3 originalPos = targetSpriteObject.transform.position;
        
        plants2d.OnButtonPress();
        Dictionary<char, string> ruleSet = plants2d.ruleSet;
        for (int i = 1; i < iterations; i++)
        {
            // Debug.Log($"Iteration {i} with max iteration {iterations}");
            Vector3 nPlantPos = originalPos + Vector3.up * (i * posStep);
            GameObject nPlant = Instantiate(targetSpriteObject, nPlantPos, quaternion.identity);
            nPlants.Add(nPlant);
            Plants2d nPlant2d = nPlant.GetComponent<Plants2d>();
            nPlant2d.ruleSet = ruleSet;
            for (int j = 0; j < i; j++)
            {
                nPlant2d.IncreaseIterations();
            }
            plantToSpriteCameras.Add(nPlant.GetComponentInChildren<PlantToSpriteCamera>());
        }

        _capturedSprite = new List<Sprite>();
        foreach (PlantToSpriteCamera plantToSpriteCamera in plantToSpriteCameras)
        {
            _capturedSprite.Add(plantToSpriteCamera.Capture());
        }

        foreach (GameObject plant in nPlants)
        {
            Destroy(plant);
        }
        
        captureCamera.gameObject.SetActive(false);
        return _capturedSprite;
    }

    public List<Sprite> CreatePlantSprites()
    {
        List<Sprite> newPlantSprites = new List<Sprite>(iterations);
        
        plants2d.OnButtonPress();
        for (int i = 0; i < iterations; i++)
        {
            CaptureStage(newPlantSprites);
            plants2d.IncreaseIterations();
        }
        
        foreach(Transform child in transform)
            Destroy(child.gameObject);
        
        return newPlantSprites;
    }

    private IEnumerator CaptureHelper()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        plants2d.OnButtonPress();
        yield return new WaitForEndOfFrame();
        
        _capturedSprite = new List<Sprite>(iterations);
        for (int i = 0; i < iterations; i++)
        {
            CaptureStage();
            plants2d.IncreaseIterations();
            yield return new WaitForEndOfFrame();
        }
    }

    private void CaptureStage(List<Sprite> sprites = null)
    {
        Bounds bounds = CalculateBounds(targetSpriteObject);
        SetCameraToBounds(captureCamera, bounds);
        
        // render camera to a temp buffer
        // RenderTexture renderTexture = new RenderTexture(
        //     (int)Mathf.Max(bounds.size.x, 1.0f) * 500, 
        //     (int)Mathf.Max(bounds.size.y, 1.0f) * 500, 
        //     32);
        RenderTexture renderTexture = new RenderTexture(
            1024, 
            1024, 
            16);
        captureCamera.targetTexture = renderTexture;
        captureCamera.Render();
        
        // transfer buffer to a texture
        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        
        RenderTexture.active = null;
        captureCamera.targetTexture = null;
        Destroy(renderTexture);
        
        texture = CropTransparentEdges(texture);
        
        // Create the sprite from the Texture2D
        Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        
        if (sprites == null) _capturedSprite.Add(newSprite);
        else sprites.Add(newSprite);
    }
    
    // Returns a camera bound that will exactly match the dimension of obj
    private Bounds CalculateBounds(GameObject obj)
    {
        SpriteRenderer[] renderers = obj.GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length == 0)
            return new Bounds(Vector3.zero, Vector3.zero);

        Bounds bounds = renderers[0].bounds;
        foreach (var renderer_ in renderers)
        {
            bounds.Encapsulate(renderer_.bounds);
        }
        return bounds;
    }

    // place the camera to the bound and adjust its projection size to that the target perfectly fit
    private void SetCameraToBounds(Camera cam, Bounds bounds)
    {
        cam.transform.position = new Vector3(bounds.center.x, bounds.center.y, cam.transform.position.z);
        float aspectRatio = (float)Screen.width / Screen.height;
        float sizeX = bounds.size.x / 2f / aspectRatio;
        float sizeY = bounds.size.y / 2f;
        cam.orthographicSize = Mathf.Max(sizeX, sizeY);
    }
    
    // remove all transparent data from original texture
    private Texture2D CropTransparentEdges(Texture2D original)
    {
        int xMin = original.width, xMax = 0, yMin = original.height, yMax = 0;
        Color32[] pixels = original.GetPixels32();

        for (int y = 0; y < original.height; y++)
        {
            for (int x = 0; x < original.width; x++)
            {
                if (pixels[y * original.width + x].a != 0)
                {
                    xMin = Mathf.Min(xMin, x);
                    xMax = Mathf.Max(xMax, x);
                    yMin = Mathf.Min(yMin, y);
                    yMax = Mathf.Max(yMax, y);
                }
            }
        }

        int width = xMax - xMin + 1;
        int height = yMax - yMin + 1;
        Texture2D croppedTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        Color[] croppedPixels = original.GetPixels(xMin, yMin, width, height);
        croppedTexture.SetPixels(croppedPixels);
        croppedTexture.Apply();

        return croppedTexture;
    }
    
    // helper to visualize what has been captured on screen
    private void DisplaySpriteOnQuad(Sprite sprite, GameObject quad)
    {
        // quad.transform.position = Vector3.zero;
        quad.SetActive(true);
        quad.transform.localScale = new Vector3(sprite.bounds.size.x, sprite.bounds.size.y, 1);
        Material material = new Material(Shader.Find("Sprites/Default"));
        material.mainTexture = sprite.texture;
        quad.GetComponent<Renderer>().material = material;
    }
    

    public void PerformanceTest()
    {
        Sprite sprite = _capturedSprite[2];
        
        Material sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        sharedMaterial.mainTexture = sprite.texture;
        sharedMaterial.enableInstancing = true; // Enable GPU instancing
        
        for (int i = 0; i < TestQuadAmount; i++)
        {
            // Vector3 screenPosition = new Vector3(Random.Range(0, Screen.width), Random.Range(0, Screen.height), 0);
            // Vector3 worldPosition = cam.ScreenToWorldPoint(screenPosition);
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.position = new Vector3(Random.Range(-1f, 17f), Random.Range(-2f, 10f), 0f);
            quad.transform.localScale = new Vector3(sprite.bounds.size.x, sprite.bounds.size.y, 1);
            quad.transform.parent = transform;
            quad.GetComponent<Renderer>().material = sharedMaterial;
            quad.isStatic = true; // Enable static batching for each quad
        }
    }
}
