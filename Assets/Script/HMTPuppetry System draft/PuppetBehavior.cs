using HMT;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public abstract class PuppetBehavior : MonoBehaviour, IPuppet {

    #region Static Elements
    
    private static Dictionary<string, int> ID_COUNTERS = new Dictionary<string, int>();
    protected static string GenerateUniquePuppetID(string prefix = null) {
        if (prefix == null) prefix = "puppet";
        if (!ID_COUNTERS.ContainsKey(prefix)) ID_COUNTERS[prefix] = 0;
        ID_COUNTERS[prefix]++;
        return prefix + "_" + ID_COUNTERS[prefix];
    }

    #endregion

    public virtual string PuppetID { get; protected set; }

    public string puppetIDPrefix = "puppet";

    public virtual byte CurrentCommandPriority { get; protected set; }

    public IEnumerable<PuppetCommand> CurrentPlan => currentPlan;
    private IEnumerable<PuppetCommand> currentPlan = null;

    public virtual bool ExecutingPlan { get { return currentPlan != null; } }

    public List<string> apiTarget = new List<string>();

    // Start is called before the first frame update
    protected virtual void Start() {
        PuppetID = GenerateUniquePuppetID(puppetIDPrefix);
        foreach(string target in apiTarget) {
            HMTPuppetManager.Instance.AddPuppet(target, this);
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

    public abstract string[] SupportedActions(string api); 

    public abstract bool ActionSupported(string api, string action);

    public abstract IEnumerator ExecuteAction(PuppetCommand command, byte priority);

    #endregion
}
