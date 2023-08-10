using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoombaBotAnimationManager : MonoBehaviour
{

    public static RoombaBotAnimationManager animationManagerInstance;

    public RoombaBotAnimationManager()
    {
        animationManagerInstance = this;
    }



    [SerializeField]
    string moveAnimationTrigger = "Move", idleAnimationTrigger = "Stop";


    [SerializeField]
    Animator roombaBotAnimator;



    private void OnEnable()
    {        
        RoombaBotInteractionManager.botInteractionManagerInstance.OnRoombaBotSelected_A += () =>
        {
            RoombaBotInteractionManager.botInteractionManagerInstance.currentSelectedRoombaBot.botMovement.OnStartMovement += StartMoveAnimation;
            RoombaBotInteractionManager.botInteractionManagerInstance.currentSelectedRoombaBot.OnActionFinished += StartIdleAnimation;
        };
        
    }


    private void OnDisable()
    {
        RoombaBotInteractionManager.botInteractionManagerInstance.OnRoombaBotSelected_A -= () =>
        {
            RoombaBotInteractionManager.botInteractionManagerInstance.currentSelectedRoombaBot.botMovement.OnStartMovement -= StartMoveAnimation;
            RoombaBotInteractionManager.botInteractionManagerInstance.currentSelectedRoombaBot.OnActionFinished -= StartIdleAnimation;
        };

    }


    // Start is called before the first frame update
    void Start()
    {
        if (roombaBotAnimator == null)
            roombaBotAnimator =  GetComponent<Animator>();
    }

    public void StartMoveAnimation()
    {
        roombaBotAnimator.SetTrigger(moveAnimationTrigger);
    }


    public void StartIdleAnimation()
    {
        roombaBotAnimator.SetTrigger(idleAnimationTrigger);
    }
}
