using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitScript : MonoBehaviour
{
    public KeyCode RestartKey = KeyCode.R;
    public KeyCode QuitKey = KeyCode.Escape;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(QuitKey))
        {
            Quit();
        }


        if (Input.GetKeyDown(RestartKey))
        {
            Restart();
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
