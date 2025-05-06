using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HMT.Puppetry {

    public abstract class PuppetBehavior : MonoBehaviour, IPuppet, IPuppetPerceivable {

        public virtual string ObjectID { get { return PuppetID; } }
        public virtual string PuppetID { get; protected set; }

        public string puppetIDPrefix = "puppet";

        public PuppetCommand CurrentCommand { get; protected set; }

       
        public HashSet<string> CurrentActionSet {
            get {
                return new HashSet<string>(coroutineActionSet.Keys.Union(directActionSet.Keys));
            }
        }

        

        #region Unity Functions

        // Start is called before the first frame update
        protected virtual void Start() {
            PuppetID = IPuppet.GenerateUniquePuppetID(puppetIDPrefix);
            HMTPuppetManager.Instance.AddPuppet(this);
            CurrentCommand = PuppetCommand.IDLE;
            _currentQueue = new Queue<PuppetCommand>();
        }

        // Update is called once per frame
        protected virtual void LateUpdate() {
            if (CurrentCommand.Command == PuppetCommandType.IDLE && CommandQueue.Count > 0) {
                CheckNextAction();
            }
        }

        #endregion

        #region Action Delegate Management

        /// <summary>
        /// The intent behind the Action Delegate system is to allow for implementers to easily specify 
        /// action funtions in the typical Unity style of using coroutines or direct function calls.
        /// </summary>

        protected Dictionary<string, Func<PuppetCommand, IEnumerator>> coroutineActionSet = new Dictionary<string, Func<PuppetCommand, IEnumerator>>();

        protected Dictionary<string, System.Action<PuppetCommand>> directActionSet = new Dictionary<string, System.Action<PuppetCommand>>();

        /// <summary>
        /// Registers an action function that executes as a coroutine.
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="action"></param>
        protected void RegisterAction(string actionName, Func<PuppetCommand, IEnumerator> action) {
            coroutineActionSet[actionName] = action;
            if (directActionSet.ContainsKey(actionName)) {
                Debug.LogWarningFormat("Action {0} registed as both a direction action and a coroutine. Note that the direct action will take precedence.", actionName);
            }
        }

        /// <summary>
        /// Registers an action function that executes directly.
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="action"></param>
        protected void RegisterAction(string actionName, System.Action<PuppetCommand> action) {
            directActionSet[actionName] = action;
            if (coroutineActionSet.ContainsKey(actionName)) {
                Debug.LogWarningFormat("Action {0} registed as both a direction action and a coroutine. Note that the direct action will take precedence.", actionName);
            }
        }

        /// <summary>
        /// Removes an action from the current action set.
        /// </summary>
        /// <param name="actionName"></param>
        protected void UnRegisterAction(string actionName) {
            if (coroutineActionSet.ContainsKey(actionName)) {
                coroutineActionSet.Remove(actionName);
            }
            else if (directActionSet.ContainsKey(actionName)) {
                directActionSet.Remove(actionName);
            }
        }

        /// <summary>
        /// Clears the current action set.
        /// </summary>
        protected void ClearActionSet() {
            coroutineActionSet.Clear();
            directActionSet.Clear();
        }

        #region BuiltInActions

        public void ActionNotImplemented(PuppetCommand command) {
            command.SendActionNotImplementedResponse();
        }

        #endregion

        #endregion

        #region Command Queue Management

        protected Queue<PuppetCommand> _currentQueue = new Queue<PuppetCommand>();
        public Queue<PuppetCommand> CommandQueue { get { return _currentQueue; } }

        protected void CheckNextAction() {
            if (CurrentCommand.Command == PuppetCommandType.IDLE && _currentQueue.Count > 0) {
                PuppetCommand nextCommand = _currentQueue.Dequeue();
                ExecuteAction(nextCommand);
            }
        }

        #endregion

        #region Implemented Interface Functions

        public virtual void ExecuteAction(PuppetCommand command) {
            if (directActionSet.ContainsKey(command.Action)) {
                directActionSet[command.Action](command);
            }
            else if (coroutineActionSet.ContainsKey(command.Action)) {
                StartCoroutine(coroutineActionSet[command.Action](command));
            }
            else {
                command.SendActionNotCurrentlySupported();
            }
        }

        public virtual void DispatchAction(PuppetCommand command) {
            if (CurrentCommand.Command == PuppetCommandType.IDLE) {
                if (command.Command == PuppetCommandType.EXECUTE_PLAN) {
                    foreach (PuppetCommand cmd in command.GetPlan()) {
                        _currentQueue.Enqueue(cmd);
                    }
                }
                else {
                    _currentQueue.Enqueue(command);
                }
                CheckNextAction();
            }
            else {

                if (command.Priority > CurrentCommand.Priority) {
                    command.SendInsufficientPriorityResponse();
                    return;
                }
                else if (command.Priority < CurrentCommand.Priority) {
                    CurrentCommand = command.GenerateStop();
                    _currentQueue.Clear();
                }

                if (command.Command == PuppetCommandType.EXECUTE_PLAN) {
                    foreach (PuppetCommand cmd in command.GetPlan()) {
                        _currentQueue.Enqueue(cmd);
                    }
                }
                else {
                    _currentQueue.Enqueue(command);
                }

            }
        }

        public virtual void ExecuteStop(PuppetCommand command) {
            if(CurrentCommand.Command != PuppetCommandType.IDLE) {
                if (CurrentCommand.Priority > command.Priority) {
                    command.SendInsufficientPriorityResponse();
                    return;
                }
                else if (CurrentCommand.Priority <= command.Priority) {
                    CurrentCommand = command;
                }
            }
        }

        public virtual JObject GetInfo(PuppetCommand command) {
            JObject info = new JObject();
            info["puppet_id"] = PuppetID;
            if (CurrentCommand.Command != PuppetCommandType.IDLE) {
                info["current_command"] = CurrentCommand.HMTStateRep();
            }
            else {
                info["current_command"] = null;
            }
            info["command_queue"] = new JArray(CommandQueue.Select(x => x.HMTStateRep()));
            info["current_action_set"] = new JArray(CurrentActionSet);
            info["full_action_set"] = new JArray(FullActionSet);
            return info;
        }

        #endregion



        #region Abstract Properties and Functions

        public abstract HashSet<string> FullActionSet { get; }

        public abstract void ExecuteCommunicate(PuppetCommand command);

        public abstract JObject GetState(PuppetCommand command);

        public abstract JObject HMTStateRep(HMTStateLevelOfDetail level);

        #endregion
    }
}