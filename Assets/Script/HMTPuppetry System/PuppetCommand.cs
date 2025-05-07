using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace HMT.Puppetry {
    public enum PuppetCommandType {
        INVALID_COMMAND,
        IDLE,
        EXECUTE_ACTION,
        EXECUTE_PLAN,
        STOP,
        GET_INFO,
        GET_STATE,
        COMMUNICATE,
        CONFIGURE,
    }

    public enum PuppetResponseCode {
        GameIntializing = 1000,
        PuppetIntializing = 1001,
        GamePaused = 1002,

        CommandAcknowleged = 2000,
        StateRetrieved = 2001,
        InfoRetrieved = 2002,
        GameOver = 2999,

        SubPuppetNotFound = 3000,
        SubPuppetLeftGroup = 3001,

        IllegalAction = 4000,
        InsufficientPriority = 4001,
        MissingParameters = 4002,
        BadParameters = 4003,
        NotSupportedInMode = 4004,


        CommandParseError = 5000,
        APIKeyMismatch = 5001,
        CommandNotRecognized = 5002,
        ActionNotSupportedByPuppet = 5003,
        ActionNotImplemented = 5004,
    }


    public struct PuppetCommand {

        public static bool VERBOSE_RESPONSES = true;
        
        private const string NO_ACTION = "no_action";

        public const byte IDLE_PRIORITY = 255;

        public static PuppetCommand IDLE {
            get {
                return new PuppetCommand(PuppetCommandType.IDLE, IDLE_PRIORITY);
            }
        }

        public static string ResponseCodeToStatus(int code) {
            return (code / 1000) switch {
                1 => "RESEND",
                2 => "OK",
                3 => "NOTFOUND",
                4 => "ILLEGAL",
                5 => "ERROR",
                _ => "UNKNOWN"
            };
        }

        public static HashSet<string> VALID_COMMANDS = new HashSet<string> { "execute_action", "execute_plan", "get_state", "configure", "communicate", "stop" };

        public static string CommandTypeToString(PuppetCommandType command) {
            return command switch {
                PuppetCommandType.EXECUTE_ACTION => "execute_action",
                PuppetCommandType.EXECUTE_PLAN => "execute_plan",
                PuppetCommandType.GET_STATE => "get_state",
                PuppetCommandType.STOP => "stop",
                PuppetCommandType.COMMUNICATE => "communicate",
                PuppetCommandType.CONFIGURE => "configure",
                _ => "invalid_command"
            };
        }

        private static PuppetCommandType ParseCommandType(string commandString) {
            switch (commandString.ToLower()) {
                case "execute_action":
                    return PuppetCommandType.EXECUTE_ACTION;
                case "execute_plan":
                    return PuppetCommandType.EXECUTE_PLAN;
                case "get_state":
                    return PuppetCommandType.GET_STATE;
                case "configure":
                    return PuppetCommandType.CONFIGURE;
                case "stop":
                    return PuppetCommandType.STOP;
                case "communicate":
                    return PuppetCommandType.COMMUNICATE;
                default:
                    return PuppetCommandType.INVALID_COMMAND;
            }
        }

        public static string FormatResponse(PuppetCommandType command, string puppetId, int code, string message, JObject content = null) {

            if(content == null)
            {
                content = new JObject();
            }
            JObject resp = new JObject
            {
                {"command", CommandTypeToString(command)},
                {"puppe_id", puppetId },
                { "code", code },
                {"status", ResponseCodeToStatus(code)  },
                {"message", message },
                {"content", content }
            };
            return resp.ToString();
        }

        public string Action { get; private set; }
        public PuppetCommandType Command { get; private set; }
        public AgentServiceRecord AgentConfig { get; private set; }
        public string TargetPuppet { get { return AgentConfig.PuppetId; } }
        public string TargetSubPuppet { get; private set; }
        public byte Priority { get { return AgentConfig.CommandPriority; } }
        public JObject ActionParams { get; private set; }

        private List<PuppetCommand> Plan;

        public JObject json { get; private set; }
        public bool Responded { get; private set; }

        public string TransactionID { get; private set; }
        
        private HMTPuppetService originService;

        #region Constructors

        /// <summary>
        /// External Constructor used by the Puppetry Interface for commands coming from outside agents.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="originService"></param>
        public PuppetCommand(JObject json, HMTPuppetService originService) {
            AgentConfig = originService.ServiceConfig;
            this.originService = originService;

            Command = ParseCommandType(json.TryGetDefault("command", string.Empty));
            Action = json.TryGetDefault("action", NO_ACTION).ToLower();
            //Debug.LogFormat("<color=red>PARSE</color> json[\"params\"]: {0}  json.ToString(): {1}", json["params"], json.ToString());
            TransactionID = json.TryGetDefault("transaction_id", string.Empty);
            TargetSubPuppet = json.TryGetDefault("subpuppet", string.Empty);
            ActionParams = json.TryGetDefault("params", new JObject());
            this.json = json;
            Plan = null;
            Responded = false;
        }

        /// <summary>
        /// Internal Action Constructor used for creating commands generated by in-game logic or player input.
        /// </summary>
        /// <param name="puppet_id"></param>
        /// <param name="action"></param>
        /// <param name="Params"></param>
        /// <param name="priority"></param>
        public PuppetCommand(string puppet_id, string action, JObject Params = null, byte priority = 128) {
            AgentConfig = new AgentServiceRecord(puppet_id, priority);
            Command = PuppetCommandType.EXECUTE_ACTION;
            Action = action;
            TargetSubPuppet = string.Empty;
            TransactionID = string.Empty;
            json = new JObject();
            this.ActionParams = Params;
            Plan = null;
            originService = null;
            Responded = false;
        }

        /// <summary>
        /// Internal Plan Constructor used for creating commands generated by in-game logic or player input.
        /// </summary>
        /// <param name="puppet_id"></param>
        /// <param name="plan"></param>
        /// <param name="priority"></param>
        public PuppetCommand(string puppet_id, List<(string action, JObject Params)> plan, byte priority = 128) {
            AgentConfig = new AgentServiceRecord(puppet_id, priority);
            Command = PuppetCommandType.EXECUTE_PLAN;
            Action = "execute_plan";
            TargetSubPuppet = string.Empty;
            TransactionID = string.Empty;
            json = new JObject();
            this.ActionParams = null;
            Plan = new List<PuppetCommand>();
            originService = null;
            Responded = false;
            foreach (var step in plan) {
                Plan.Add(new PuppetCommand(step.action, step.Params, this));
            }
        }

        /// <summary>
        /// A copy constructor that is only used for creating Action commands from plan commands.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="original"></param>
        private PuppetCommand(string action, JObject Params, PuppetCommand original) {
            AgentConfig = original.AgentConfig;
            Command = PuppetCommandType.EXECUTE_ACTION;
            Action = action;
            TargetSubPuppet = original.TargetSubPuppet;
            TransactionID = original.TransactionID;
            this.ActionParams = Params;
            json = new JObject();
            this.Plan = null;
            originService = original.originService;
            Responded = false;
        }

        /// <summary>
        /// A copy constructor that is only used to change the command type of a command.
        /// </summary>
        /// <param name="original"></param>
        private PuppetCommand(PuppetCommandType command, PuppetCommand original) {
            AgentConfig = original.AgentConfig;
            Command = command;
            Action = NO_ACTION;
            TargetSubPuppet = string.Empty;
            TransactionID = original.TransactionID;
            this.ActionParams = null;
            json = new JObject();
            this.Plan = null;
            originService = original.originService;
            Responded = true;
        }

        private PuppetCommand(PuppetCommandType command, byte priority) {
            AgentConfig = AgentServiceRecord.NONE;
            Command = command;
            Action = NO_ACTION;
            TargetSubPuppet = string.Empty;
            TransactionID = string.Empty;
            this.ActionParams = null;
            json = new JObject();
            this.Plan = null;
            originService = null;
            Responded = true;
        }

        #endregion

        /// <summary>
        /// Creates a STOP command that uses the same priority and properties as the original command.
        /// </summary>
        /// <returns></returns>
        public PuppetCommand GenerateStop() {
            return new PuppetCommand(PuppetCommandType.STOP, this);
        }

        public JObject HMTStateRep() {
            JObject state = new JObject();
            state["puppet_id"] = TargetPuppet;
            state["command"] = CommandTypeToString(Command);
            state["action"] = Action;
            state["params"] = ActionParams;
            state["priority"] = Priority;
            return state;
        }

        public List<PuppetCommand> GetPlan() {
            if(this.Command != PuppetCommandType.EXECUTE_PLAN) {
                return null;
            }
            if (Plan != null) {
                return Plan;
            }
            Plan = new List<PuppetCommand>();
            JToken swap;
            if (json.TryGetValue("plan", out swap)) {
                foreach (JValue planStep in swap) {
                    Plan.Add(new PuppetCommand(planStep["action"].ToString(), planStep["Params"] as JObject, this));
                }
            }
            return Plan;
        }

        //private const string RESPONSE_FORMAT = "{{\"command\":\"{0}\", \"puppet_id\":{1},  \"code\":{2}, \"status\":\"{3}\", \"message\":\"{4}\", \"content\":{5} }}";
        private void FormatAndSendResponse(int code, string message, JObject content = null) {
            if (originService == null || Responded) {
                return;
            }

            if (content == null)
            {
                content = new JObject();
            }

            JObject resp;

            if (VERBOSE_RESPONSES) {
                resp = new JObject {
                        {"command",  CommandTypeToString(Command)},
                        {"puppet_id", TargetPuppet },
                        { "code", code },
                        {"status", ResponseCodeToStatus(code)  },
                        {"message", message },
                        {"content", content }
                    };
            }
            else {
                resp = new JObject {
                        {"command",  CommandTypeToString(Command)},
                        {"puppet_id", TargetPuppet },
                        {"code", code },
                        {"content", content }
                    };
            }
            if(TransactionID != string.Empty) {
                resp["transaction_id"] = TransactionID;
            }
            string mess = resp.ToString();
            if (TransactionID != string.Empty) {
                Debug.LogFormat("<color=magenta>{5}</color> puppet: <color=cyan>{0}</color> responds: <color=yellow>{1}</color> to: <color=red>{2}</color> {3}, {4}\nfull_response:{6}", TargetPuppet, code, AgentConfig.AgentId, CommandTypeToString(Command), Action, TransactionID, mess);
            }
            else {
                Debug.LogFormat("puppet: <color=cyan>{0}</color> responds: <color=yellow>{1}</color> to: <color=red>{2}</color> {3}, {4}\nfull_response:{5}", TargetPuppet, code, AgentConfig.AgentId, CommandTypeToString(Command), Action, mess);
            }
            Responded = true;
            originService.Context.WebSocket.Send(mess);
        }

        #region RESEND (1000s) Responses

        public void SendGameInitializingResponse() {
            FormatAndSendResponse((int)PuppetResponseCode.GameIntializing, "Game Initializing");
        }

        public void SendPuppetInitializingResponse() {
            FormatAndSendResponse((int)PuppetResponseCode.PuppetIntializing, "Puppet Initializing");
        }

        public void SendGamePausedResponse() {
            FormatAndSendResponse((int)PuppetResponseCode.GamePaused, "Game Paused");
        }

        #endregion

        #region OK (2000s) Responses

        public void SendAcknowledgeResponse() {
            FormatAndSendResponse((int)PuppetResponseCode.CommandAcknowleged, "Command Acknowledged");
        }

        public void SendGameOverResponse() {
            FormatAndSendResponse((int)PuppetResponseCode.GameOver, "Game Over");
        }

        public void SendStateResponse(JObject state) {
            FormatAndSendResponse((int)PuppetResponseCode.StateRetrieved, "State Retrieved", state);
        }

        public void SendInfoResponse(JObject info) {
            FormatAndSendResponse((int)PuppetResponseCode.InfoRetrieved, "Info Retrieved", info);
        }

        #endregion

        #region NOTFOUND (3000s) Responses

        public void SendSubPuppetNotFoundResponse(string subPuppetId) {
            JObject content = new JObject {
                {"subpuppet_id", subPuppetId }
            };
            FormatAndSendResponse((int)PuppetResponseCode.SubPuppetNotFound, "SubPuppet Not Found", content);
        }

        public void SendSubPuppetLeftGroupResponse(string subPuppetId) {
            JObject content = new JObject {
                {"subpuppet_id", subPuppetId }
            };
            FormatAndSendResponse((int)PuppetResponseCode.SubPuppetLeftGroup, "SubPuppet Left Group", content);
        }

        #endregion

        #region ILLEGAL (4000s) Responses

        public void SendIllegalActionResponse() {
            FormatAndSendResponse((int)PuppetResponseCode.IllegalAction, "Illegal Action");
        }

        public void SendIllegalActionResponse(string longMessage) {
            JObject content = new JObject {
                {"long_message", longMessage }
            };
            FormatAndSendResponse((int)PuppetResponseCode.IllegalAction, "Illegal Action", content);
        }

        public void SendInsufficientPriorityResponse() {
            FormatAndSendResponse((int)PuppetResponseCode.InsufficientPriority, "Insufficient Priority");
        }

        public void SendMissingParametersResponse(JObject requiredParams) {
            JObject content = new JObject {
                {"required_params", requiredParams }
            };
            FormatAndSendResponse((int)PuppetResponseCode.MissingParameters, "Missing Parameters", content);
        }

        public void SendBadParametersResponse(JObject badParameters, JObject requiredParams) {
            JObject content = new JObject {
                {"bad_params", badParameters },
                {"required_params", requiredParams }
            };
            FormatAndSendResponse((int)PuppetResponseCode.BadParameters, "Bad Parameters", content);
        }

        public void SendActionNotCurrentlySupported() {
            FormatAndSendResponse((int)PuppetResponseCode.NotSupportedInMode, "Not Supported In Current Mode");
        }

        #endregion

        #region ERROR (5000s) Responses

        public void SendAPIKeyMismatchResponse() {
            FormatAndSendResponse((int)PuppetResponseCode.APIKeyMismatch, "API Key Mismatch");
        }

        public void SendCommandNotRecognizedResposne() {
            JObject content = new JObject();
            content["valid_commands"] = JArray.FromObject(VALID_COMMANDS);
            FormatAndSendResponse((int)PuppetResponseCode.CommandNotRecognized, "Command Not Recognized", content);
        }

        public void SendActionNotSupportedResponse(IEnumerable<string> supportedActions) {
            JObject content = new JObject();
            content["action_set"] = JArray.FromObject(supportedActions);
            FormatAndSendResponse((int)PuppetResponseCode.ActionNotSupportedByPuppet, "Action Not Supported By Puppet", content);
        }

        public void SendActionNotImplementedResponse() {
            FormatAndSendResponse((int)PuppetResponseCode.ActionNotImplemented, "Action Not Implemented");
        }

        #endregion

    }
}