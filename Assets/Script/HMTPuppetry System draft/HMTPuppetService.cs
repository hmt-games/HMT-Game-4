using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;

namespace HMT.Puppetry {
    /// <summary>
    /// This class is just for facilitating the socket interface. 
    /// 
    /// My goal would be for no logic to actually live here and instead by 
    /// handled by the ExecuteAction virtual method in the main HMTInterface class.
    /// </summary>
    public class HMTPuppetService : WebSocketBehavior {
        public AgentServiceConfig ServiceConfig { get; set; }
        public HashSet<string> ActionSet { get; set; }

        protected override void OnMessage(MessageEventArgs e) {
            Debug.LogFormat("[HMTPuppetService] /{0} recieved Command: {1}", ServiceConfig.ServiceTarget,  e.Data);

            JObject json;
            try {
                json = JObject.Parse(e.Data);
            }
            catch (JsonReaderException ex) {
                Debug.LogErrorFormat("[HMTPuppetService] /{0} Error parsing JSON: {1}", ServiceConfig.ServiceTarget, ex.Message);
                Context.WebSocket.Send(PuppetCommand.FormatResponse(PuppetCommandType.INVALID_COMMAND, 
                    ServiceConfig.PuppetId, 
                    (int)PuppetResponseCode.CommandParseError, 
                    "Command Parse Error", 
                    new JObject {
                        { "original", JsonConvert.ToString(e.Data) }
                    }));
                return;
            }

            PuppetCommand newCommand = new PuppetCommand(json, this);
            
            if (newCommand.Command == PuppetCommandType.INVALID_COMMAND) {
                newCommand.SendCommandNotRecognizedResposne();
                return;
            }

            if (ServiceConfig.APIKey != string.Empty && newCommand.json.TryGetDefault("api_key", string.Empty) != ServiceConfig.APIKey) {
                Debug.LogErrorFormat("API Key Mismatch: {0} != {1}", json["api_key"], ServiceConfig.APIKey);
                newCommand.SendAPIKeyMismatchResponse();
                return;
            }

            if (newCommand.Command == PuppetCommandType.EXECUTE_ACTION && 
                !ActionSet.Contains(newCommand.Action)) {
                newCommand.SendActionNotSupportedResponse(ActionSet);
                return;
            }

            switch (HMTPuppetManager.Instance.Status) {
                case HMTPuppetManager.PuppetryStatus.Intializing:
                    newCommand.SendGameInitializingResponse();
                    break;
                case HMTPuppetManager.PuppetryStatus.Paused:
                    newCommand.SendGamePausedResponse();
                    break;
                case HMTPuppetManager.PuppetryStatus.Running:
                    // Enqueue the Command to the manager to handle cross thread concurrency issues.
                    HMTPuppetManager.Instance.EnqueueCommand(newCommand);
                    break;
            }

            
        }

        protected override void OnOpen() {
            Debug.LogFormat("[HMTPuppetService] {0} Client Connected.", ServiceConfig.ServiceTarget);
        }

        protected override void OnClose(CloseEventArgs e) {
            Debug.LogFormat("[HMTPuppetService] {0} Cliend Disconnected.", ServiceConfig.ServiceTarget);
        }

        protected override void OnError(ErrorEventArgs e) {
            Debug.LogErrorFormat("[HMTPuppetService] {0} Error: {1}", ServiceConfig.ServiceTarget, e.Message);
            Debug.LogException(e.Exception);
        }
    }
}