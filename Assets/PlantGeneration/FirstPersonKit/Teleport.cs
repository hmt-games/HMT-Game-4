using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Teleport : MonoBehaviour
{

    [Tooltip("Should the object rotate as the destination object?")]
    public bool inheritRotation = true;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void TeleportTo(GameObject destination)
    {
        CharacterController cc = gameObject.GetComponent<CharacterController>();


        if (destination == null)
        {
            print("Warning: no destination (object) has been choosen");
        }
        else
        {
            //disable character controller
            if (cc != null)
                cc.enabled = false;

            transform.position = destination.transform.position;

            if (inheritRotation)
            {
                transform.rotation = destination.transform.rotation;
            }


            //enable character controller
            if (cc != null)
            {
                //ANNOYING workaround for a Unity BUG: triggerExit doesn't fire if teleporting
                PlayerInteraction pi = gameObject.GetComponent<PlayerInteraction>();
                if(pi != null)
                {
                    if(pi.currentInteractable != null)
                    {
                        Interactable i = pi.currentInteractable.GetComponent<Interactable>();
                        i.TriggerExit();
                        pi.currentInteractable = null;
                    }
                }

                cc.enabled = true;
            }
        }
    }


}
