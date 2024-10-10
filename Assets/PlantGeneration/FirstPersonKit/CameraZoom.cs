using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [Tooltip("The player camera. If not set defaults to the main")]
    public Camera playerCamera;

    [Tooltip("The action that will trigger the zooming.")]
    public string zoomInput = "Fire2";

    [Tooltip("The target field of view when zooming. Narrower angle > zoom farther")]
    public float targetZoom = 30;

    [Tooltip("The time it takes to zoom in.")]
    public float zoomInTime = 0.2f;

    public AnimationCurve zoomInCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    [Tooltip("The time it takes to zoom out.")]
    public float zoomOutTime = 0.2f;

    public AnimationCurve zoomOutCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    // A timer used for zooming.
    private float zoomTimer;
    // The smoothing intensity when force looking.
    private float lookAtStrength;
    // The rotation of the body.
    private float bodyAngle;
    // The original field of view of the camera.
    private float originalFieldOfView;
    // The cached field of view when starting to zoom.
    private float originalZoomFieldOfView;
    // The field of view of the zoom.
    private float zoomFieldOfView;
    // Used to check if the player is zooming.
    private bool zooming;

    // Start is called before the first frame update
    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        originalFieldOfView = playerCamera.fieldOfView;
        zoomFieldOfView = originalFieldOfView;
        zoomTimer = zoomOutTime;
    }
    

    void Update()
    {
       
        // If the zoom button is held down, do the zoom.
        if (Input.GetButton(zoomInput))
        {

            // If the zoom in time is less than or equals to 0, just set the value directly.
            if (zoomInTime <= 0)
            {
                playerCamera.fieldOfView = targetZoom;
            }
            else
            {
                // If the player isn't zooming, they are now. Reset the timer and get the FOV.
                if (!zooming)
                {
                    zoomTimer = 0;
                    zooming = true;
                    originalZoomFieldOfView = zoomFieldOfView; 
                    
                    
                }

                // As long as the zoom timer is less than the zoom in time or the field of view is more than the target zoom, 
                // set the zoom value.
                if (zoomTimer < zoomInTime || playerCamera.fieldOfView > targetZoom)
                {
                    zoomFieldOfView = Mathf.Lerp(originalZoomFieldOfView, targetZoom, zoomInCurve.Evaluate(zoomTimer / zoomInTime));
                    zoomTimer += Time.deltaTime;
                    playerCamera.fieldOfView = zoomFieldOfView;
                }
            }
        }
        else
        {
            // If the zoom out time is less than or equals to 0, just set the value directly.
            if (zoomOutTime <= 0)
            {
                playerCamera.fieldOfView = originalFieldOfView;
            }
            else
            {
                // If the player is zooming, they aren't now. Reset the timer and get the FOV.
                if (zooming)
                {
                    zoomTimer = 0;
                    zooming = false;
                    originalZoomFieldOfView = zoomFieldOfView;
                }

                float target = originalFieldOfView;// + fieldOfViewKick.FieldOfViewDifference;

                // As long as the zoom timer is less than the zoom in time or the field of view is less than the original FOV, 
                // set the zoom value.
                if (zoomTimer < zoomOutTime || playerCamera.fieldOfView < target)
                {
                    zoomFieldOfView = Mathf.Lerp(originalZoomFieldOfView, target, zoomOutCurve.Evaluate(zoomTimer / zoomOutTime));
                    playerCamera.fieldOfView = zoomFieldOfView;
                    zoomTimer += Time.deltaTime;
                }
            }
        }
    }
}
