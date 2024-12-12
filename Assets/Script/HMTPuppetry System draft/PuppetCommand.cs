using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace HMT.Puppetry {
    public enum PuppetCommandType {
        INVALID_COMMAND,
        EXECUTE_ACTION,
        EXECUTE_PLAN,
        GET_STATE,
        COMMUNICATE,
        CONFIGURE,
    }

    public enum PuppetResponseCode {
        GameIntializing = 1000,
        PuppetIntializing = 1001,
        GamePaused = 1002,

        CommandAcknowleged = 2000,
        ReturnedState = 2001,
        GameOver = 2999,

        IllegalAction = 4000,
        InsufficientPriority = 4001,

        CommandParseError = 5000,
        APIKeyMismatch = 5001,
        CommandNotRecognized = 5002,
        ActionNotSupportedByPuppet = 5003,
    }


    public class PuppetCommand {
        private const string RESPONSE_FORMAT = "{{\"command\":\"{0}\", \"puppet_id\":{1}  \"code\":{2}, \"status\":\"{3}\", \"message\":\"{4}\", \"content\":{5} }}";
        private const string NO_ACTION = "no_action";

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

        public static HashSet<string> VALID_COMMANDS = new HashSet<string> { "execute_action", "execute_plan", "get_state", "configure", "communicate" };

        public static string CommandTypeToString(PuppetCommandType command) {
            return command switch {
                PuppetCommandType.EXECUTE_ACTION => "execute_action",
                PuppetCommandType.EXECUTE_PLAN => "execute_plan",
                PuppetCommandType.GET_STATE => "get_state",
                PuppetCommandType.COMMUNICATE => "communicate",
                PuppetCommandType.CONFIGURE => "configure",
                _ => "invalid_command"
            };
        }

        public static string FormatResponse(PuppetCommandType command, string puppetId, int code, string message, string content = "{}") {
            return string.Format(RESPONSE_FORMAT, CommandTypeToString(command), puppetId, code, ResponseCodeToStatus(code), message, content);
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
                case "communicate":
                    return PuppetCommandType.COMMUNICATE;
                default:
                    return PuppetCommandType.INVALID_COMMAND;
            }
        }

        public const byte IDLE_PRIORITY = 255;

        public string Action { get; private set; }
        public PuppetCommandType Command { get; private set; }
        public AgentServiceConfig AgentConfig { get; private set; }
        public string TargetPuppet { get { return AgentConfig.PuppetId; } }
        public byte Priority { get { return AgentConfig.CommandPriority; } }
        public JObject json { get; private set; }
        public bool Responded { get; private set; }
        private HMTPuppetService originService;

        public PuppetCommand(JObject json, HMTPuppetService originService) {
            AgentConfig = originService.ServiceConfig;
            this.originService = originService;
            
            Command = ParseCommandType(json.TryGetDefault("Command", string.Empty));
            Action = json.TryGetDefault("Action", NO_ACTION).ToLower();
            this.json = json;            
            Responded = false;
        }

        public PuppetCommand(string puppet_id, string action, byte priority = 128) {
            AgentConfig = new AgentServiceConfig(puppet_id, priority);
            Command = PuppetCommandType.EXECUTE_ACTION;
            this.Action = action;
            json = new JObject();
            originService = null;
            Responded = false;
        }

        /// <summary>
        /// A copy constructor that is only used for creating Action commands from plan commands.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="original"></param>
        private PuppetCommand(string action, PuppetCommand original) {
            AgentConfig = original.AgentConfig;
            Command = PuppetCommandType.EXECUTE_ACTION;
            this.Action = action;
            json = new JObject();
            originService = original.originService;
            Responded = false;
        }

        public List<PuppetCommand> GetPlan() {
            if (this.Command == PuppetCommandType.EXECUTE_PLAN) {
                List<PuppetCommand> plan = new List<PuppetCommand>();
                JToken swap;
                if (json.TryGetValue("plan", out swap)) {
                    foreach (JValue planStep in swap) {
                        plan.Add(new PuppetCommand(planStep.ToString(), this));
                    }
                }
                return plan;
            }
            else {
                return new List<PuppetCommand>();
            }
        }

        private void FormatAndSendResponse(int code, string message, string content = "{}") {
            if(originService == null || Responded) {
                return;
            }

            string mess = string.Format(RESPONSE_FORMAT, Command, TargetPuppet, code, message, content);
            originService.Context.WebSocket.Send(mess);
            Responded = true;
        }

        public void SendGameInitializingResponse() {
            FormatAndSendResponse((int)PuppetResponseCode.GameIntializing, "Game Initializing");
        }

        public void SendPuppetInitializingResponse() {
            FormatAndSendResponse((int)PuppetResponseCode.PuppetIntializing, "Puppet Initializing");
        }

        public void SendGamePausedResponse() {
            FormatAndSendResponse((int)PuppetResponseCode.GamePaused, "Game Paused");
        }

        public void SendAcknowledgeResponse() {
            FormatAndSendResponse((int)PuppetResponseCode.CommandAcknowleged, "Command Acknowledged");
        }

        public void SendGameOverResponse() {
            FormatAndSendResponse((int)PuppetResponseCode.GameOver, "Game Over");
        }

        public void SendStateResponse(JObject state) {
            FormatAndSendResponse((int)PuppetResponseCode.ReturnedState, "State Retrieved", state.ToString());
        }

        public void SendAPIKeyMismatchResponse() {
            FormatAndSendResponse((int)PuppetResponseCode.APIKeyMismatch, "API Key Mismatch");
        }

        public void SendCommandNotRecognizedResposne() {
            JObject content = new JObject();
            content["valid_commands"] = JArray.FromObject(VALID_COMMANDS);
            FormatAndSendResponse((int)PuppetResponseCode.CommandNotRecognized, "Command Not Recognized", content.ToString());
        }

        public void SendActionNotSupportedResponse(IEnumerable<string> supportedActions) {
            JObject content = new JObject {
                ["action_set"] = JArray.FromObject(supportedActions)
            };
            FormatAndSendResponse((int)PuppetResponseCode.ActionNotSupportedByPuppet, "Action Not Supported By Puppet", content.ToString());
        }

        public void SendIllegalActionResponse() {
            FormatAndSendResponse((int)PuppetResponseCode.IllegalAction, "Illegal Action");
        }

        public void SendInsufficientPriorityResponse() {
            FormatAndSendResponse((int)PuppetResponseCode.InsufficientPriority, "Insufficient Priority");
        }
    }
}