using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShotCamera : MonoBehaviour
{
    //Saves a screenshot when a button is pressed
    public KeyCode screenShotKey = KeyCode.P;
    string folderPath = "Assets/Screenshots/";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    
    void Update()
    {
        if (Input.GetKeyDown(screenShotKey))
        {
            if (!System.IO.Directory.Exists(folderPath)) // if this path does not exist yet
                System.IO.Directory.CreateDirectory(folderPath);  // it will get created

            var screenshotName =
                                    "Screenshot_" +
                                    System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + // puts the current time right into the screenshot name
                                    ".png"; // put youre favorite data format here
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folderPath, screenshotName), 2); // takes the sceenshot, the "2" is for the scaled resolution, you can put this to 600 but it will take really long to scale the image up
            Debug.Log("Screenshot saved as "+folderPath + screenshotName); // You get instant feedback in the console

        }
    }

}
