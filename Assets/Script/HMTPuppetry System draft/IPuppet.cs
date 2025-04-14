using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace HMT.Puppetry {
    /// <summary>
    /// This is an interface that will be used internally in the PupperManager system.
    /// 
    /// You should use it if you *NEED* to base your code on a different base class, otherwise use the abstract class.
    /// </summary>
    public interface IPuppet {
        public static Dictionary<string, int> ID_COUNTERS = new Dictionary<string, int>();
        public static string GenerateUniquePuppetID(string prefix = null) {
            if (prefix == null) prefix = "puppet";
            if (!ID_COUNTERS.ContainsKey(prefix)) ID_COUNTERS[prefix] = 0;
            ID_COUNTERS[prefix]++;
            return prefix + "_" + ID_COUNTERS[prefix];
        }

        /// <summary>
        /// The ID of the puppet, which is used as it's address in the system.
        /// </summary>
        public string PuppetID { get; }

        /// <summary>
        /// The current Action Command being executed by the puppet.
        /// </summary>
        public PuppetCommand CurrentCommand { get; }

        /// <summary>
        /// Returns whether the puppet is currently executing an Action.
        /// </summary>
        public bool ExecutingAction { get; }

        /// <summary>
        /// Returns whether the puppet is currently executing a plan.
        /// </summary>
        public bool ExecutingPlan { get; }

        /// <summary>
        /// Returns the list of actions supported by the puppet at the given API.
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public HashSet<string> SupportedActions { get; }

        /// <summary>
        /// Takes a Command from the HMTPupperManager and executes it.
        /// 
        /// The IEnumerator is used to allow for coroutines to be used in the implementation but it could also just set a variable or trigger an Action. 
        /// 
        /// All commands executions should recieve a response to the origin service even if it's just an acknowledgment of receipt.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public void ExecuteAction(PuppetCommand command);

        /// <summary>
        /// Takes a sequence of commands and executes them in order.
        /// 
        /// This plan would come bundled as part of a single Command from the HMTPupperManager.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public void ExecutePlan(PuppetCommand command);
        
        /// <summary>
        /// Takes a Commnuicate Command and executes it.
        /// </summary>
        /// <param name="command"></param>
        public void ExecuteCommunicate(PuppetCommand command);

        /// <summary>
        /// Returns the Json representation of the state as percieved by the puppet
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public JObject GetState(PuppetCommand command);
    }

}
