using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantToSpriteCapturer : MonoBehaviour
{
    [SerializeField] private Camera captureCamera;
    [SerializeField] private GameObject targetSpriteObject;
    [SerializeField] private List<GameObject> quadDisplays;
    [SerializeField] private int iterations = 4;
    [SerializeField] private Plants2d plants2d;
    
    private List<Sprite> _capturedSprite;

    public void Capture()
    {
        StartCoroutine(CaptureHelper());
    }

    private IEnumerator CaptureHelper()
    {
        foreach (var quad in quadDisplays)
        {
            quad.SetActive(false);
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

        DisplayAllSpriteOnQuad();
    }

    private void CaptureStage()
    {
        Bounds bounds = CalculateBounds(targetSpriteObject);
        SetCameraToBounds(captureCamera, bounds);
        
        // render camera to a temp buffer
        // RenderTexture renderTexture = new RenderTexture(
        //     (int)Mathf.Max(bounds.size.x, 1.0f) * 500, 
        //     (int)Mathf.Max(bounds.size.y, 1.0f) * 500, 
        //     32);
        RenderTexture renderTexture = new RenderTexture(
            1000, 
            1000, 
            32);
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
        _capturedSprite.Add(newSprite);
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
        cam.orthographicSize = Mathf.Max(bounds.size.x, bounds.size.y) / 2f;
        float aspectRatio = (float)Screen.width / Screen.height;
        cam.orthographicSize = Mathf.Max(bounds.size.x / 2f / aspectRatio, bounds.size.y / 2f);
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

    private void DisplayAllSpriteOnQuad()
    {
        for (int i = 0; i < iterations; i++)
        {
            DisplaySpriteOnQuad(_capturedSprite[i], quadDisplays[i]);
        }
    }
}
