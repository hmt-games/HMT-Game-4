using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantToSpriteCamera : MonoBehaviour
{
    private Camera _captureCamera;

    public Sprite Capture()
    {
        _captureCamera = GetComponent<Camera>();
        
        Bounds bounds = CalculateBounds(transform.parent.gameObject);
        SetCameraToBounds(_captureCamera, bounds);
        
        // render camera to a temp buffer
        // RenderTexture renderTexture = new RenderTexture(
        //     (int)Mathf.Max(bounds.size.x, 1.0f) * 500, 
        //     (int)Mathf.Max(bounds.size.y, 1.0f) * 500, 
        //     32);
        RenderTexture renderTexture = new RenderTexture(
            1024, 
            1024, 
            16);
        _captureCamera.targetTexture = renderTexture;
        _captureCamera.Render();
        
        // transfer buffer to a texture
        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        
        RenderTexture.active = null;
        _captureCamera.targetTexture = null;
        Destroy(renderTexture);
        
        texture = CropTransparentEdges(texture);
        
        // Create the sprite from the Texture2D
        Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        return newSprite;
    }
    
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
    
    private void SetCameraToBounds(Camera cam, Bounds bounds)
    {
        cam.transform.position = new Vector3(bounds.center.x, bounds.center.y, cam.transform.position.z);
        float aspectRatio = (float)Screen.width / Screen.height;
        float sizeX = bounds.size.x / 2f / aspectRatio;
        float sizeY = bounds.size.y / 2f;
        cam.orthographicSize = Mathf.Max(sizeX, sizeY);
    }
    
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
}
