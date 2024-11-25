using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp.Server;

namespace HMT {
    public struct PuppetCommand {
        private const string RESPONSE_FORMAT = "{{\"command\":\"{0}\", \"puppet_id\":{1}  \"code\":{2}, \"status\":\"{3}\", \"message\":\"{4}\", \"content\":{5} }}";
        private const string NO_ACTION = "no_action";
        private const string NO_COMMAND = "no_command";

        public static HashSet<string> VALID_COMMANDS = new HashSet<string> { EXECUTE_ACTION, EXECUTE_PLAN, GET_STATE, CONFIGURE };

        public const string EXECUTE_ACTION = "execute_action";
        public const string EXECUTE_PLAN = "execute_plan";
        public const string GET_STATE = "get_state";
        public const string CONFIGURE = "configure";

        public string agentId;
        public string targetPuppet;
        public string command;
        public string action;
        public JObject json;
        private HMTService originService;
        public byte priority;

        public bool Responded { get; private set; }
        public bool HasAction => action != NO_ACTION;
        public bool HasPlan => json.TryGetValue("plan", out _);

        public PuppetCommand(JObject json, HMTService originService) {
            agentId = originService.AgentId;
            targetPuppet = originService.PuppetId;
            priority = originService.CommandPriority;
            this.originService = originService;
            
            command = json.TryGetDefault("command", NO_COMMAND).ToLower();
            action = json.TryGetDefault("action", NO_ACTION).ToLower();
            this.json = json;            
            Responded = false;
        }

        public List<PuppetCommand> GetPlan() {
            if (this.command == "exectute_plan") {
                List<PuppetCommand> plan = new List<PuppetCommand>();
                JToken swap;
                if (json.TryGetValue("plan", out swap)) {
                    foreach (JObject planStep in swap) {
                        plan.Add(new PuppetCommand(planStep, originService));
                    }
                }
                return plan;
            }
            else {
                return new List<PuppetCommand>();
            }
        }

        public void SendRetryResponse(int code, string message, string content = null) {
            if(code / 1000 != 1) {
                throw new System.ArgumentException("RETRY Codes must be between 1000 and 1999");
            }
            string mess = string.Format(RESPONSE_FORMAT, command, targetPuppet, code, "RETRY", message, content);
            originService.Context.WebSocket.Send(mess);
            Responded = true;
        }

        public void SendAcknowledgeResponse() {
            string mess = string.Format(RESPONSE_FORMAT, command, targetPuppet, 2000, "OK", "Command Acknowledged", null);
            originService.Context.WebSocket.Send(mess);
            Responded = true;
        }
        public void SendGameOverResponse() {
            string mess = string.Format(RESPONSE_FORMAT, command, targetPuppet, 2999, "OK", "Game Over", null);
            originService.Context.WebSocket.Send(mess);
            Responded = true;
        }

        public void SendOKResponse(int code, string message, string content = null) {
            if(code <= 2001 || code >= 2999) {
                throw new System.ArgumentException("OK Codes must be between 2001 and 2998");
            }
            string mess = string.Format(RESPONSE_FORMAT, command, targetPuppet, code, "OK", message, content);
            originService.Context.WebSocket.Send(mess);
            Responded = true;
        }

        public void SendNotFoundResponse(int code, string message, string content = null) {
            if (code / 1000 != 3) {
                throw new System.ArgumentException("NOTFOUND Codes must be between 3000 and 3999");
            }
            string mess = string.Format(RESPONSE_FORMAT, command, targetPuppet, code, "NOTFOUND", message, content);
            originService.Context.WebSocket.Send(mess);
            Responded = true;
        }

        public void SendIllegalResponse(int code, string message, string content = null) {
            if(code / 1000 != 4) {
                throw new System.ArgumentException("ILLEGAL Codes must be between 4000 and 4999");
            }
            string mess = string.Format(RESPONSE_FORMAT, command, targetPuppet, code, "ILLEGAL", message, content);
            originService.Context.WebSocket.Send(mess);
            Responded = true;
        }

        public void SendErrorResponse(int code, string message, string content = null) {
            if(code / 1000 != 5) {
                throw new System.ArgumentException("ERROR Codes must be between 5000 and 5999");
            }
            string mess = string.Format(RESPONSE_FORMAT, command, targetPuppet, code, "ERROR", message, content);
            originService.Context.WebSocket.Send(mess);
            Responded = true;
        }
    }
}