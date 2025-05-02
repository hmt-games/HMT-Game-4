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
        /// A collection of Action Commands that are queued up to execute.
        /// 
        /// Implicitly this is a queue but it could be implemented in other ways
        /// </summary>
        public Queue<PuppetCommand> CurrentPlan { get; }

        /// <summary>
        /// Returns the list of actions supported by the puppet in it's current configuration or mode.
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public HashSet<string> CurrentActionSet { get; }

        /// <summary>
        /// Returns the list of actions that could ever be supported by the puppet in any configuration or mode.
        /// </summary>
        public HashSet<string> FullActionSet { get; }

        /// <summary>
        /// Used by the HMTPuppetManager to send commands to the puppet.
        /// 
        /// The Manager does not directly call ExecuteAction to allow puppets to implement their own conflict resolution system.
        /// </summary>
        /// <param name="command"></param>
        public void DispatchAction(PuppetCommand command);

        /// <summary>
        /// Takes a Command from the HMTPupperManager and executes it.
        /// 
        /// All commands executions should recieve a response to the origin service even if it's just an acknowledgment of receipt.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public void ExecuteAction(PuppetCommand command);

        /// <summary>
        /// Takes a Commnuicate Command and executes it.
        /// </summary>
        /// <param name="command"></param>
        public void ExecuteCommunicate(PuppetCommand command);

        /// <summary>
        /// Stops the current action and clears the current plan if there is sufficient priority.
        /// </summary>
        /// <param name="command"></param>
        public void ExecuteStop(PuppetCommand command);

        /// <summary>
        /// Returns the JSON representation of the state as percieved by the puppet
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public JObject GetState(PuppetCommand command);

        /// <summary>
        /// Returns a JSON representation of basic puppet information.
        /// 
        /// The provided command could provide additional context such as specific information requested.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public JObject GetInfo(PuppetCommand command);
    }

}
