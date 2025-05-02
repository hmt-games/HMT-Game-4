using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace HMT.Puppetry {

    public abstract class PuppetBehavior : MonoBehaviour, IPuppet, IPuppetPerceivable {

        public virtual string ObjectID { get { return PuppetID; } }
        public virtual string PuppetID { get; protected set; }

        public string puppetIDPrefix = "puppet";

        public PuppetCommand CurrentCommand { get; protected set; }

        protected Queue<PuppetCommand> _currentPlan = new Queue<PuppetCommand>();
        public Queue<PuppetCommand> CurrentPlan { get { return _currentPlan; } }

        // Start is called before the first frame update
        protected virtual void Start() {
            PuppetID = IPuppet.GenerateUniquePuppetID(puppetIDPrefix);
            HMTPuppetManager.Instance.AddPuppet(this);
            CurrentCommand = null;
            _currentPlan = new Queue<PuppetCommand>();
        }

        // Update is called once per frame
        protected virtual void LateUpdate() {
            if (CurrentCommand == null && CurrentPlan.Count > 0) {
                CheckNextAction();
            }
        }

        public virtual void DispatchAction(PuppetCommand command) {
            if (CurrentCommand == null) {
                if (command.Command == PuppetCommandType.EXECUTE_PLAN) {
                    foreach (PuppetCommand cmd in command.GetPlan()) {
                        _currentPlan.Enqueue(cmd);
                    }
                }
                else {
                    _currentPlan.Enqueue(command);
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
                    _currentPlan.Clear();
                }

                if (command.Command == PuppetCommandType.EXECUTE_PLAN) {
                    foreach (PuppetCommand cmd in command.GetPlan()) {
                        _currentPlan.Enqueue(cmd);
                    }
                }
                else {
                    _currentPlan.Enqueue(command);
                }

            }
        }

        protected void CheckNextAction() {
            if (CurrentCommand == null &&  _currentPlan.Count > 0) {
                PuppetCommand nextCommand = _currentPlan.Dequeue();
                ExecuteAction(nextCommand);
            }
        }

        public virtual void ExecuteStop(PuppetCommand command) {
            if(CurrentCommand != null) {
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
            info["current_command"] = CurrentCommand?.HMTStateRep();
            info["current_plan"] = new JArray(CurrentPlan.Select(x => x.HMTStateRep()));
            info["current_action_set"] = new JArray(CurrentActionSet);
            info["full_action_set"] = new JArray(FullActionSet);
            return info;
        }

        #region Abstract Elements

        public abstract HashSet<string> CurrentActionSet { get; protected set; }

        public abstract HashSet<string> FullActionSet { get; protected set; }

        public abstract void ExecuteAction(PuppetCommand command);

        public abstract void ExecuteCommunicate(PuppetCommand command);

        public abstract JObject GetState(PuppetCommand command);

        public abstract JObject HMTStateRep(HMTStateLevelOfDetail level);

        #endregion
    }
}