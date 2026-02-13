using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

/*
 * This script triggers a UI message and/or a sound when the player enters an area.
 * This script has to be attached to an object with a collider marked as trigger (the area)
 * */

public class Interactable : MonoBehaviour
{
    
    [Tooltip("The message to display as text upon entering the trigger. Leave black if none.")]
    public string message = "";

    [Tooltip("Should the message disappear when the player exits the collider?")]
    public bool hideMessageAtExit = true;
    
    [Tooltip("The sound to play upon entering the trigger. Leave black if none.")]
    public AudioClip sound;

    [Tooltip("Should the sound play upon entering the trigger or upon interact action?")]
    public bool soundOnInteract = false;

    [Tooltip("Should the message disappear when the sound stops? ie subtitle/caption")]
    public bool hideMessageAtSoundComplete = false;

    [Tooltip("True if currently triggered")]
    public bool triggered = false;

    [Tooltip("Disable Interactable after exiting the trigger")]
    public bool disableAfterTrigger = false;

    [Tooltip("Disable Interactable after the interaction")]
    public bool disableAfterInteraction = false;

    [Tooltip("If no audiosource component is assigned, it will use the player 2D sound (eg voiceover).")]
    public AudioSource audioSource;
    [Tooltip("Automatically assigned")]
    public GameObject player;
    [Tooltip("Automatically assigned")]
    public PlayerInteraction playerInteraction;

    

    [Serializable]
    public class MyEvent : UnityEvent { }

    [Space(20)]
    public MyEvent EnterTrigger;
    public MyEvent ExitTrigger;
    public MyEvent OnInteract;


    // Start is called before the first frame update
    void Start()
    {
        

        //check if there is a collider
        Collider[] cols = transform.GetComponentsInChildren<Collider>();
        bool oneTrigger = false;

        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i].isTrigger)
                oneTrigger = true;
        }

        if (cols.Length == 0)
        {
            print("Warning: the object " + gameObject.name + " doesn't have any colliders attached");
        }
        else if (!oneTrigger)
        {
            print("Warning: the object " + gameObject.name + " doesn't have any colliders set on trigger attached");
        }
        

    }

    // Update is called once per frame
    void Update()
    {
        //if currently in the area and clicked
        if (Input.GetButtonDown(playerInteraction.InteractInput) && triggered && this.enabled)
        {
            Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject == player && !triggered && this.enabled)
        {
            
            
            triggered = true;
            playerInteraction.currentInteractable = this;
       
            //if there is a sound, play
            if (!soundOnInteract && sound != null)
            {
                audioSource.PlayOneShot(sound);
            }

            //if there is a message to display
            if (message != "")
            {
                //if the message is a caption pass the duration
                if (sound != null && hideMessageAtSoundComplete)
                    playerInteraction.DisplayMessage(message, sound.length);
                else if (!hideMessageAtExit) //pass a duration
                    playerInteraction.DisplayMessage(message, message.Length * playerInteraction.messageDurationPerCharacter);
                else //don't pass any duration
                    playerInteraction.DisplayMessage(message);
            }

            EnterTrigger.Invoke();

            if (disableAfterTrigger)
            {
                this.enabled = false;
            }
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player && this.enabled)
        {
            TriggerExit();
        }
    }

    public void TriggerExit()
    {
        
        if (hideMessageAtExit && playerInteraction.currentInteractable == this)
            playerInteraction.HideMessage();

        ExitTrigger.Invoke();

        triggered = false;
    }

    public void Interact()
    {
        if (this.enabled)
        {
            //print("Interacting with " + gameObject.name);

            //if there is a sound, play
            if (soundOnInteract && sound != null)
            {
                if (!audioSource.isPlaying || (audioSource.isPlaying && audioSource.clip != sound))
                {
                    audioSource.clip =sound;
                    audioSource.Play();
                }
            }

            OnInteract.Invoke();

            //auto disable
            if (disableAfterInteraction)
            {

                if (!audioSource.isPlaying)
                    this.enabled = false;
                else //wait for the sound to be done
                    Invoke("Disable", sound.length);
            }
        }
    }

    public void Disable()
    {
        this.enabled = false;
    }
}
