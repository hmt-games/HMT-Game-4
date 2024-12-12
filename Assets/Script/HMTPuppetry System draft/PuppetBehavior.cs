using HMT.Puppetry;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PuppetBehavior : MonoBehaviour, IPuppet {

    public virtual string PuppetID { get; protected set; }

    public string puppetIDPrefix = "puppet";

    public PuppetCommand CurrentCommand { get; protected set; }

    public IList<PuppetCommand> CurrentPlan => currentPlan;
    private IList<PuppetCommand> currentPlan = null;

    public virtual bool ExecutingAction { get { return CurrentCommand != null; } }

    public virtual bool ExecutingPlan { get { return currentPlan != null; } }

    public List<string> apiTarget = new List<string>();

    // Start is called before the first frame update
    protected virtual void Start() {
        PuppetID = IPuppet.GenerateUniquePuppetID(puppetIDPrefix);
        foreach(string target in apiTarget) {
            HMTPuppetManager.Instance.AddPuppet(this);
        }
        CurrentCommand = null;
        currentPlan = null;
    }

    // Update is called once per frame
    protected virtual void Update() {
        
    }

    public virtual void ExecutePlan(PuppetCommand command) { 
        if (command.Command != PuppetCommandType.EXECUTE_PLAN) {
            throw new System.ArgumentException("Command does not have a plan.");
        }
        if((CurrentCommand == null && command.Priority <= PuppetCommand.IDLE_PRIORITY) || command.Priority <= CurrentCommand.Priority) {
            CurrentCommand = command;
            StartCoroutine(ExecutePlanCoroutine(command));
        }
        else {
            command.SendInsufficientPriorityResponse();
        }
    }

    IEnumerator ExecutePlanCoroutine(PuppetCommand puppetCommand) {
        currentPlan = CurrentCommand.GetPlan();
        for(int i = 0; i < currentPlan.Count; i++) {
            ExecuteAction(currentPlan[i]);
            while(ExecutingAction) {
                yield return null;
            }
            if(!ExecutingPlan) {
                break;
            }
        }
        CurrentCommand = null;
        yield break;
    }

    #region Abstract Elements

    public abstract HashSet<string> SupportedActions { get; }

    public abstract void ExecuteAction(PuppetCommand command);

    public abstract void ExecuteCommunicate(PuppetCommand command);

    public abstract JObject GetState(PuppetCommand command);

    #endregion
}
