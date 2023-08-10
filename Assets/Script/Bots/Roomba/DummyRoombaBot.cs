using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyRoombaBot : RoombaBot
{    
    //Define the primitive actions in detail here. Define only the required ones.
    public override IEnumerator Fertilize()
    {        

        yield return ChangeMode(IPrimitiveRoombaBotActions.RoombaBotMode.Fertilize);

        yield return GotoLoc(currentTargetNodeToPerformActionOn.coordinate);

        //Fertilize Action
        yield return base.Fertilize();

        OnActionFinished?.Invoke();

        Idle();

        yield break;

    }

    public override IEnumerator Water()
    {

        yield return ChangeMode(IPrimitiveRoombaBotActions.RoombaBotMode.Water);

        yield return GotoLoc(currentTargetNodeToPerformActionOn.coordinate);

        //Water Action
        yield return base.Water();

        OnActionFinished?.Invoke();

        Idle();

        yield break;

    }


    public override IEnumerator Harvest()
    {

        yield return ChangeMode(IPrimitiveRoombaBotActions.RoombaBotMode.Harvest);

        yield return GotoLoc(currentTargetNodeToPerformActionOn.coordinate);
        
        //Harvest Action
        yield return base.Harvest();

        OnActionFinished?.Invoke();

        Idle();

        yield break;

    }


    public override IEnumerator GotoLoc(Vector2 gridNodeCoordinates)
    {
        yield return base.GotoLoc(gridNodeCoordinates);
        yield break;
    }

    public override IEnumerator ChangeMode(IPrimitiveRoombaBotActions.RoombaBotMode roombaBotMode)
    {
        yield return base.ChangeMode(roombaBotMode);
        yield break;
    }


    public override void Idle()
    {
        base.Idle();
    }

    public override void PerformAction()
    {
        base.PerformAction();

        switch (currentActionToPerform)
        {
            case IPrimitiveRoombaBotActions.PrimitiveActions.None:
                Debug.LogError("None Action cannot be performed");
                break;

            case IPrimitiveRoombaBotActions.PrimitiveActions.Water:
                StartCoroutine(Water());
                break;

            case IPrimitiveRoombaBotActions.PrimitiveActions.Fertilize:
                StartCoroutine(Fertilize());
                break;

            case IPrimitiveRoombaBotActions.PrimitiveActions.Harvest:
                StartCoroutine(Harvest());
                break;
        }
    }

    public override void StopAction()
    {
        base.StopAction();
    }

}
