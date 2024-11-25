using HMT;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public abstract class PuppetBehavior : MonoBehaviour, IPuppet {

    public virtual string PuppetID { get; protected set; }

    public string puppetIDPrefix = "puppet";

    public virtual byte CurrentCommandPriority { get; protected set; }

    public IEnumerable<PuppetCommand> CurrentPlan => currentPlan;
    private IEnumerable<PuppetCommand> currentPlan = null;

    public virtual bool ExecutingPlan { get { return currentPlan != null; } }

    public List<string> apiTarget = new List<string>();

    // Start is called before the first frame update
    protected virtual void Start() {
        PuppetID = IPuppet.GenerateUniquePuppetID(puppetIDPrefix);
        foreach(string target in apiTarget) {
            HMTPuppetManager.Instance.AddPuppet(this);
        }
        CurrentCommandPriority = IPuppet.IDLE_PRIORITY;
        currentPlan = null;
    }

    // Update is called once per frame
    protected virtual void Update() {
        
    }

    public virtual IEnumerator ExecutePlan(PuppetCommand sourceCommand, IEnumerable<PuppetCommand> plan, byte priority) {
        if(priority <= CurrentCommandPriority) {
            CurrentCommandPriority = priority;
            currentPlan = plan;
            foreach(PuppetCommand command in currentPlan) {
                yield return ExecuteAction(command, CurrentCommandPriority);
            }
            CurrentCommandPriority = IPuppet.IDLE_PRIORITY;
        }
        else {
            sourceCommand.SendIllegalResponse(4001, "Insufficient priority to execute plan.");
            yield break;
        }
    }

    #region Abstract Elements

    public abstract void Countermand(byte priority);

    public abstract bool ExecutingAction { get; }

    public abstract HashSet<string> SupportedActions { get; }

    public abstract IEnumerator ExecuteAction(PuppetCommand command, byte priority);

    public abstract JObject GetState(PuppetCommand command);

    #endregion
}
