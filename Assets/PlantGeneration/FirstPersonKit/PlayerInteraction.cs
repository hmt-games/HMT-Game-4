using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Tooltip("If not assigned it will use the default one in the player prefab")]
    public Canvas canvas;
    [Tooltip("If not assigned it will use the default one in the player prefab")]
    public TMP_Text messageField;

    public string InteractInput = "Fire1";

    [Tooltip("If no audiosource component is assigned, it will use the player 2D sound (eg voiceover).")]
    public AudioSource audioSource;

    public Interactable[] interactables;

    [Tooltip("Message duration is this multiplied by the number of characters.")]
    public float messageDurationPerCharacter = 0.2f;
    
    public float messageTimer = -1;
    public Interactable currentInteractable;
    private Interactable targetedInteractable;

    // Start is called before the first frame update
    void Start()
    {
        if(canvas == null)
        {
            GameObject canvasObj = transform.Find("Canvas").gameObject;
            
            if (canvasObj != null)
            {
                canvas = canvasObj.GetComponent<Canvas>();
                
                if (messageField == null)
                {
                    GameObject messageFieldObj = canvasObj.transform.Find("Message").gameObject;

                    if (messageFieldObj != null)
                        messageField = messageFieldObj.GetComponent<TMP_Text>();
                }
            }

            if (canvas == null || messageField == null)
                print("Warning: I couldn't find a canvas and/or a message field");
        }

        //create default audiosource
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0;
        }

        //looks for all the interactables in the scene so you don't have to assign them 
        interactables = Resources.FindObjectsOfTypeAll<Interactable>();

        //reference the centralized stuff
        for (int i = 0; i < interactables.Length; i++)
        {
            InitializeInteractable(interactables[i]);
        }

        if (messageField != null)
            messageField.text = "";
    }

    void InitializeInteractable(Interactable inter)
    {
        inter.player = gameObject;
        inter.playerInteraction = this;
        inter.audioSource = audioSource;
    }

    // Update is called once per frame
    void Update()
    {
        
        //if there is a timer in progress make it disappear at the end
        if (messageTimer > 0)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer < 0)
                messageField.text = "";
        }
        
    }

    public void DisplayMessage(string msg, float dur=-1)
    {
        messageField.text = msg;
        messageTimer = dur;
    }

    public void HideMessage()
    {
        messageField.text = "";
    }

}
