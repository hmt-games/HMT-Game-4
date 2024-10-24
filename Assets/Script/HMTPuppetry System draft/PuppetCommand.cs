using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp.Server;

namespace HMT {
    public struct PuppetCommand {
        private const string RESPONSE_FORMAT = "{{\"command\":\"{0}\", \"api\":{1}, \"puppet_id\":{2}  \"code\":{3}, \"status\":\"{4}\", \"message\":\"{5}\", \"content\":{6} }}";
        public const string NO_ACTION = "NO_ACTION";
        public const string NO_COMMAND = "NO_COMMAND";
        public const string NO_AGENT_ID = "NO_AGENT_ID";

        public string agentId;
        public string targetAPI;
        public string targetPuppet;
        public string command;
        public string action;
        public JObject json;
        public WebSocketBehavior originService;
        public bool Responded { get; private set; }
        public bool HasAction => action != NO_ACTION;
        public bool HasPlan => json.TryGetValue("plan", out _);

        public PuppetCommand(string targetAPI, JObject json, WebSocketBehavior originService) {
            JToken swap;
            this.targetAPI = targetAPI;
            this.agentId = json.TryGetValue("agent_id", out swap) ? swap.ToString() : NO_AGENT_ID;
            this.targetPuppet = json.TryGetValue("target", out swap) ? swap.ToString() : "root";
            this.command = json.TryGetValue("command", out swap) ? swap.ToString() : NO_COMMAND;
            this.action = json.TryGetValue("action", out swap) ? swap.ToString() : NO_ACTION;
            this.json = json;
            this.originService = originService;
            this.Responded = false;
        }

        public List<PuppetCommand> GetPlan() {
            List<PuppetCommand> plan = new List<PuppetCommand>();
            JToken swap;
            if(json.TryGetValue("plan", out swap)) {
                foreach(JObject planStep in swap) {
                    plan.Add(new PuppetCommand(targetAPI, planStep, originService));
                }
            }
            return plan;
        }

        public void SendRetryResponse(int code, string message, string content = null) {
            if(code / 1000 != 1) {
                throw new System.ArgumentException("RETRY Codes must be between 1000 and 1999");
            }
            string mess = string.Format(RESPONSE_FORMAT, command, targetAPI, targetPuppet, code, "RETRY", message, content);
            originService.Context.WebSocket.Send(mess);
            Responded = true;
        }

        public void SendAcknowledgeResponse() {
            string mess = string.Format(RESPONSE_FORMAT, command, targetAPI, targetPuppet, 2000, "OK", "Command Acknowledged", null);
            originService.Context.WebSocket.Send(mess);
            Responded = true;
        }
        public void SendGameOverResponse() {
            string mess = string.Format(RESPONSE_FORMAT, command, targetAPI, targetPuppet, 2999, "OK", "Game Over", null);
            originService.Context.WebSocket.Send(mess);
            Responded = true;
        }

        public void SendOKResponse(int code, string message, string content = null) {
            if(code <= 2000 || code >= 2999) {
                throw new System.ArgumentException("OK Codes must be between 2001 and 2998");
            }
            string mess = string.Format(RESPONSE_FORMAT, command, targetAPI, targetPuppet, code, "OK", message, content);
            originService.Context.WebSocket.Send(mess);
            Responded = true;
        }

        public void SendNotFoundResponse(int code, string message, string content = null) {
            if (code / 1000 != 3) {
                throw new System.ArgumentException("NOTFOUND Codes must be between 3000 and 3999");
            }
            string mess = string.Format(RESPONSE_FORMAT, command, targetAPI, targetPuppet, code, "NOTFOUND", message, content);
            originService.Context.WebSocket.Send(mess);
            Responded = true;
        }

        public void SendIllegalResponse(int code, string message, string content = null) {
            if(code / 1000 != 4) {
                throw new System.ArgumentException("ILLEGAL Codes must be between 4000 and 4999");
            }
            string mess = string.Format(RESPONSE_FORMAT, command, targetAPI, targetPuppet, code, "ILLEGAL", message, content);
            originService.Context.WebSocket.Send(mess);
            Responded = true;
        }

        public void SendErrorResponse(int code, string message, string content = null) {
            if(code / 1000 != 5) {
                throw new System.ArgumentException("ERROR Codes must be between 5000 and 5999");
            }
            string mess = string.Format(RESPONSE_FORMAT, command, targetAPI, targetPuppet, code, "ERROR", message, content);
            originService.Context.WebSocket.Send(mess);
            Responded = true;
        }
    }

    public struct AgentRegistrationRecord {
        public string agentId;
        public string sessionId;
        public byte priority;
        public Dictionary<string, HashSet<string>> targetsAndPuppets;

        public AgentRegistrationRecord(string agentId, string sessionId, byte priority) {
            this.agentId = agentId;
            this.sessionId = sessionId;
            this.priority = priority;
            targetsAndPuppets = new Dictionary<string, HashSet<string>>();
        }

        public AgentRegistrationRecord(string agentId, string sessionId, string target, string puppet, byte priority) {
            this.agentId = agentId;
            this.sessionId = sessionId;
            this.priority = priority;
            this.targetsAndPuppets = new Dictionary<string, HashSet<string>>();
            AddPuppet(target, puppet);
        }

        public int TargetCount {
            get {
                return targetsAndPuppets.Count;
            }
        }

        public int PuppetCount {
            get {
                int count = 0;
                foreach (var item in targetsAndPuppets) {
                    count += item.Value.Count;
                }
                return count;
            }
        }

        public void AddPuppet(string target, string puppet) {
            if(!targetsAndPuppets.ContainsKey(target)) {
                targetsAndPuppets.Add(target, new HashSet<string>());
            }
            targetsAndPuppets[target].Add(puppet);
        }

        public void RemovePuppet(string target, string puppet) {
            if (targetsAndPuppets.ContainsKey(target)) {
                targetsAndPuppets[target].Remove(puppet);
            }
        }
    }
}