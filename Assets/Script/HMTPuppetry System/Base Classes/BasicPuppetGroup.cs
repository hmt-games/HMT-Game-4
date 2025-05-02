using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HMT.Puppetry {
    public class BasicPuppetGroup : IPuppetGroup {

        public string GroupName { get; private set; }

        public PuppetGroupMembershipType MembershipType { get; protected set; }

        public PuppetGroupStateRepresentation StateRepresentation { get; protected set; }

        public HashSet<string> SubPuppets {
            get {
                return new HashSet<string>(_subPuppets.Keys);
            }
        }

        public string PuppetID { get { return "pg-" + GroupName; } }

        public PuppetCommand CurrentCommand { get; private set; }

        public Queue<PuppetCommand> CurrentPlan { get; private set; } = new Queue<PuppetCommand>();

        public HashSet<string> CurrentActionSet {
            get {
                HashSet<string> currentActionSet = new HashSet<string>();
                foreach (var puppet in _subPuppets.Values) {
                    currentActionSet.UnionWith(puppet.CurrentActionSet);
                }
                return currentActionSet;
            }
        }

        public HashSet<string> FullActionSet {
            get {
                HashSet<string> fullActionSet = new HashSet<string>();
                foreach (var puppet in _subPuppets.Values) {
                    fullActionSet.UnionWith(puppet.FullActionSet);
                }
                return fullActionSet;
            }
        }

        private Dictionary<string, IPuppet> _subPuppets = new Dictionary<string, IPuppet>();
        private HashSet<string> allTimeSubPuppets = new HashSet<string>();

        public BasicPuppetGroup(string groupName) {
            GroupName = groupName;
            MembershipType = PuppetGroupMembershipType.Category;
            StateRepresentation = PuppetGroupStateRepresentation.Union;
            _subPuppets = new Dictionary<string, IPuppet>();
            allTimeSubPuppets = new HashSet<string>();
        }

        public BasicPuppetGroup(string groupName, PuppetGroupMembershipType membershipType, PuppetGroupStateRepresentation stateRep) {
            GroupName = groupName;
            MembershipType = membershipType;
            StateRepresentation = stateRep;
            _subPuppets = new Dictionary<string, IPuppet>();
            allTimeSubPuppets = new HashSet<string>();
        }

        public void AddSubPuppet(IPuppet puppet) {
            _subPuppets.Add(puppet.PuppetID, puppet);
            allTimeSubPuppets.Add(puppet.PuppetID);
        }

        public void RemoveSubPuppet(IPuppet puppet) {
            _subPuppets.Remove(puppet.PuppetID);
        }

        public bool ContainsSubPuppet(string puppet_id) {
            return _subPuppets.ContainsKey(puppet_id);
        }

        public void DispatchAction(PuppetCommand command) {
            if (!string.IsNullOrEmpty(command.TargetSubPuppet)) {
                if (!ContainsSubPuppet(command.TargetSubPuppet)) {
                    if (allTimeSubPuppets.Contains(command.TargetSubPuppet)) {
                        command.SendSubPuppetLeftGroupResponse(command.TargetSubPuppet);
                    }
                    else {
                        command.SendSubPuppetNotFoundResponse(command.TargetSubPuppet);
                    }
                    return;
                }
                _subPuppets[command.TargetSubPuppet].DispatchAction(command);
            }
            else {
                foreach (var puppet in _subPuppets.Values) {
                    puppet.DispatchAction(command);
                }
            }
        }

        public void ExecuteAction(PuppetCommand command) {
            return;
        }

        public void ExecuteCommunicate(PuppetCommand command) {
            throw new System.NotImplementedException();
        }

        public void ExecuteStop(PuppetCommand command) {
            if (!string.IsNullOrEmpty(command.TargetSubPuppet)) {
                if (!ContainsSubPuppet(command.TargetSubPuppet)) {
                    if (allTimeSubPuppets.Contains(command.TargetSubPuppet)) {
                        command.SendSubPuppetLeftGroupResponse(command.TargetSubPuppet);
                    }
                    else {
                        command.SendSubPuppetNotFoundResponse(command.TargetSubPuppet);
                    }
                    return;
                }
                _subPuppets[command.TargetSubPuppet].ExecuteStop(command);
            }
            else {
                foreach (var puppet in _subPuppets.Values) {
                    puppet.ExecuteStop(command);
                }
            }
        }

        public JObject GetInfo(PuppetCommand command) {
            return new JObject {
                {"group_name", GroupName},
                {"membership_type", MembershipType.ToString()},
                {"state_representation", StateRepresentation.ToString()},
                {"sub_puppets", new JArray(SubPuppets)}
            };
        }

        public JObject GetState(PuppetCommand command) {
            switch (StateRepresentation) {
                case PuppetGroupStateRepresentation.Target:
                    if (!string.IsNullOrEmpty(command.TargetSubPuppet)) {
                        if (!ContainsSubPuppet(command.TargetSubPuppet)) {
                            if (allTimeSubPuppets.Contains(command.TargetSubPuppet)) {
                                command.SendSubPuppetLeftGroupResponse(command.TargetSubPuppet);
                            }
                            else {
                                command.SendSubPuppetNotFoundResponse(command.TargetSubPuppet);
                            }
                            return new JObject();
                        }
                        return _subPuppets[command.TargetSubPuppet].GetState(command);
                    }
                    else { 
                        return new JObject();
                    }
                case PuppetGroupStateRepresentation.Custom:
                case PuppetGroupStateRepresentation.Union:
                    JObject unionState = new JObject();
                    unionState["info"] = GetInfo(command);
                    JObject percept = new JObject();

                    foreach (var puppet in _subPuppets.Values) {
                        JObject puppetState = puppet.GetState(command);
                        //TODO this is probably not going to do what we want it to do,
                        // It might clober data or duplicate overlapping data
                        percept.Merge(puppetState["percept"]);
                    }
                    unionState["percept"] = percept;
                    return unionState;
                default:
                    throw new System.NotImplementedException();
            }
        }


    }

}