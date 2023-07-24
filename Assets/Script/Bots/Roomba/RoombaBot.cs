using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Should hold characteristics of a bot.
/// Specific types of bot behaviors should inherit from this class.
/// </summary>
public abstract class RoombaBot : MonoBehaviour
{
    public RoombaBotMovement botMovement;

    public virtual void Start()
    {
        botMovement = GetComponent<RoombaBotMovement>();
    }

}
