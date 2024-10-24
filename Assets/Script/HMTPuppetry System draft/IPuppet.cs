using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace HMT {

    /// <summary>
    /// This is an interface that will be used internally in the PupperManager system.
    /// 
    /// You should use it if you *NEED* to base your code on a different base class, otherwise use the abstract class.
    /// </summary>
    public interface IPuppet {

        public static byte IDLE_PRIORITY = 255;

        /// <summary>
        /// The ID of the puppet, which is used as it's address in the system.
        /// </summary>
        public string PuppetID { get; }

        /// <summary>
        /// The priority level of the current command being executed.
        /// 
        /// Commands with less or equal priority will take precendence over commands with higher priority. When the puppet is idle it's priority will be 255.
        /// </summary>
        public byte CurrentCommandPriority { get; }

        /// <summary>
        /// Return whether the puppet is currently executing a plan.
        /// 
        /// If not currently executing a plan it can still be executing a single command.
        /// </summary>
        public bool ExecutingPlan { get; }

        /// <summary>
        /// Return whether the puppet is currently executing a single action.
        /// </summary>
        public bool ExecutingAction { get; }


        /// <summary>
        /// Returns whether or not the action is supported by the puppet at the given API.
        /// </summary>
        /// <param name="api"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ActionSupported(string api, string action);

        /// <summary>
        /// Returns the list of actions supported by the puppet at the given API.
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public string[] SupportedActions(string api);

        /// <summary>
        /// Takes a command from the HMTPupperManager and executes it.
        /// 
        /// The IEnumerator is used to allow for coroutines to be used in the implementation but it could also just set a variable or trigger an action. 
        /// 
        /// All commands executions should recieve a response to the origin service even if it's just an acknowledgment of receipt.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public IEnumerator ExecuteAction(PuppetCommand command, byte priority);

        /// <summary>
        /// Takes a sequence of commands and executes them in order.
        /// 
        /// This plan would come bundled as part of a single command from the HMTPupperManager.
        /// </summary>
        /// <param name="sourceCommand"></param>
        /// <param name="plan"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public IEnumerator ExecutePlan(PuppetCommand sourceCommand, IEnumerable<PuppetCommand> plan, byte priority);

        /// <summary>
        /// Countermands a command with a given priority.
        /// 
        /// If the puppet is currently executing a plan it will also stop the plan.
        /// </summary>
        /// <param name="priority"></param>
        public void Countermand(byte priority);
    }

}
