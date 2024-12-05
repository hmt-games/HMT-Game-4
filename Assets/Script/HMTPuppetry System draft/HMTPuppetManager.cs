using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Concurrent;
using Cinemachine;
using System.Net;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEditor;
using System.Security.Policy;

namespace HMT {
    public class HMTPuppetManager : MonoBehaviour {
        public static HMTPuppetManager Instance { get; private set; }
        private static int SERVICE_TARGET_COUNTER = 0;

        [Header("AI Socket Settings")]
        [Tooltip("Whether the server should automatically start itself on Start or wait to be started by an external caller.")]
        public bool StartServerOnStart = true;
        //[Tooltip("The URL of the socket server, will default to localhost if empty.")]
        //public string socketUrl = "ws://localhost";
        [Tooltip("The port that the puppetry server should use.")]
        public int socketPort = 4649;
        [Tooltip("The name of the root service apprended to the url for the puppet manager.")]
        public string rootService = "hmt";
        [Tooltip("Whether service targets should be assigned sequentially or randomly. If true, targets will be incremental integers, if false they will be randomly generated GUIDs")]
        public bool useSequentialServiceTargets = false;
        [Tooltip("Whether the puppet manager should use API keys to authenticate calls to service targets.")]
        public bool useAPIKeys = true;

        [Tooltip("The threshold for automatic responses to commands. If a command is not responded to by the target puppet in this time, a generic acknoweldgement will be sent. Note that this is in terms of unscaledTime not regular time so it does not respect speed up or pausing.")]
        public float autoResponseThreshold = 3f;
        [Tooltip("The default priority level for agent commands that do not specify on registration.")]
        [Range(0, 255)]
        public int defaultCommandPriority = 128;

        internal HttpServer server = null;

        private ArgParser Args;
        private ConcurrentQueue<PuppetCommand> commandQueue;
        private List<(float time, PuppetCommand puppet)> commandsInFlight;
        private Dictionary<string, IPuppet> PuppetIndex = new Dictionary<string, IPuppet>();
        private Dictionary<string, HMTService> ServiceIndex = new Dictionary<string, HMTService>();

        void Awake() {
            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(this);
            }
            Args = new ArgParser();
            Args.AddArg("hmtsocketport", ArgParser.ArgType.One);
            Args.AddArg("hmtapikeys", ArgParser.ArgType.Flag);
            Args.AddArg("hmtsequentialservices", ArgParser.ArgType.Flag);
            Args.ParseArgs();

            useSequentialServiceTargets = Args.GetArgValue("hmtsequentialservices", useSequentialServiceTargets);
            useAPIKeys = Args.GetArgValue("hmtapikeys", useAPIKeys);

            Dictionary<string, IPuppet> PuppetIndex = new Dictionary<string, IPuppet>();
            Dictionary<string, HMTService> ServiceIndex = new Dictionary<string, HMTService>();

            commandQueue = new ConcurrentQueue<PuppetCommand>();
            commandsInFlight = new List<(float time, PuppetCommand puppet)>();
            
        }

        // Start is called before the first frame update
        void Start() {
            if(StartServerOnStart) {
                StartSocketServer();
            }
        }

        public void StartSocketServer() {
            socketPort = Args.GetArgValue("hmtsocketport", socketPort);

            if (socketPort == 80) {
                Debug.LogWarning("HMTPuppetManager Socket set to Port 80, this might cause permissions issues.");
            }

            server = new HttpServer(socketPort);
            server.OnGet += Server_OnGet;
            server.OnPost += Server_OnPost;
            server.Start();
        }

        private void Server_OnPost(object sender, HttpRequestEventArgs e) {
            var path = e.Request.RawUrl;

            switch (path) {
                case "/register_agent":
                    JObject postData = e.GetJsonPostData();
                    string agentId = postData["agent_id"].ToString();
                    string puppetId = postData["puppet_id"].ToString();
                    string sessionID = postData.TryGetDefault<string>("session_id", System.Guid.NewGuid().ToString());
                    byte priority = postData.TryGetDefault<byte>("priority", (byte)defaultCommandPriority);
                    
                    if(!PuppetIndex.ContainsKey(puppetId)) {
                        Debug.LogWarning("Puppet ID not found in Puppet Index");
                        e.SendBasicResponse((int)HttpStatusCode.BadRequest, "Puppet ID not found in Puppet Index");
                        return;
                    }
                    
                    (string newServiceTarget, string apiKey) = LaunchNewServiceTarget(agentId, puppetId, priority);
                    JObject response = new JObject {
                        { "service_target", string.Format("ws://localhost:{0}/{1}/{2}", socketPort, rootService, newServiceTarget) },
                        { "session_id", sessionID },
                        { "agent_id", agentId },
                        { "puppet_id", puppetId },
                        { "priority", priority},
                        { "action_set", new JArray(PuppetIndex[puppetId].SupportedActions) }
                    };
                    if (useAPIKeys) {
                        response["api_key"] = apiKey;
                    }
                    e.SendJsonResponse(response);
                    break;
                case "/list_puppets":
                    e.SendJsonResponse(ListPuppets());
                    break;
                default:
                    e.SendBasicResponse((int)HttpStatusCode.NotFound, "path does not exist");
                    break;
            }
        }

        private void Server_OnGet(object sender, HttpRequestEventArgs e) {
            var path = e.Request.RawUrl;
            switch (path) {
                case "/list_puppets":
                    e.SendJsonResponse(ListPuppets());
                    break;
                default:
                    e.SendBasicResponse((int)HttpStatusCode.NotFound, "path does not exist");
                    break;
            }
        }

        private JObject ListPuppets() {
            JObject job = new JObject();
            JArray puppetList = new JArray();
            foreach(string key in PuppetIndex.Keys) {
                puppetList.Add(new JObject {
                    {"puppet_id", key },
                    {"action_set", new JArray(PuppetIndex[key].SupportedActions) }
                });
            }
            job["puppets"] = puppetList;
            return job;
        }

        public (string, string) LaunchNewServiceTarget(string agent_id, string puppet_id, byte priority) {
            string newServiceTarget = string.Empty;
            string apiKey = useAPIKeys ? System.Guid.NewGuid().ToString() : string.Empty;
            if (useSequentialServiceTargets) {
                newServiceTarget = ServiceIndex.Count.ToString();
            }
            else {
                newServiceTarget = System.Guid.NewGuid().ToString();
            }
            server.AddWebSocketService<HMTService>("/" + newServiceTarget, s => {
                s.ServiceTarget = newServiceTarget;
                s.AgentId = agent_id;
                s.PuppetId = puppet_id;
                s.CommandPriority = priority;
                s.APIKey = apiKey;
                s.ActionSet = PuppetIndex[puppet_id].SupportedActions;
            });
            return (newServiceTarget, apiKey);
        }

        public void EnqueueCommand(PuppetCommand command) {
            commandQueue.Enqueue(command);
        }

        // Update is called once per frame
        void Update() {
            while(commandQueue.Count > 0) {
                if (commandQueue.TryDequeue(out PuppetCommand command)) {
                    IPuppet puppet = PuppetIndex[command.targetPuppet];
                    switch (command.command) {
                        case PuppetCommand.EXECUTE_ACTION:
                            StartCoroutine(puppet.ExecuteAction(command, command.priority));
                            commandsInFlight.Add((Time.unscaledTime, command));
                            break;
                        case PuppetCommand.EXECUTE_PLAN:
                            StartCoroutine(puppet.ExecutePlan(command, command.GetPlan(), command.priority));
                            commandsInFlight.Add((Time.unscaledTime, command));
                            break;
                        case PuppetCommand.GET_STATE:
                            JObject state = puppet.GetState(command);
                            command.SendStateResponse(state);
                            break;
                    }
                }
            }
            if (autoResponseThreshold > 0) {
                foreach ((float time, PuppetCommand command) in commandsInFlight.ToList()) {
                    if (command.Responded) {
                        commandsInFlight.Remove((time, command));
                        continue;
                    }
                    if (Time.unscaledTime - time > autoResponseThreshold) {
                        command.SendAcknowledgeResponse();
                        commandsInFlight.Remove((time, command));
                    }
                }
            }
        }

        /// <summary>
        /// Adds a Puppet to the puppet list.
        /// </summary>
        /// <param name="puppet"></param>
        /// <returns></returns>
        public void AddPuppet(IPuppet puppet) {
            PuppetIndex[puppet.PuppetID] = puppet;
        }

        /// <summary>
        /// Removes a Puppet from the puppet list.
        /// </summary>
        /// <param name="puppetId"></param>
        /// <returns></returns>
        public void RemovePuppet(IPuppet puppet) {
            if (PuppetIndex.ContainsKey(puppet.PuppetID)) {
                PuppetIndex.Remove(puppet.PuppetID);
            }

            //TODO remove any socket connections that are attached to this puppet.
        }
    }

    /// <summary>
    /// This class is just for facilitating the socket interface. 
    /// 
    /// My goal would be for no logic to actually live here and instead by 
    /// handled by the ExecuteAction virtual method in the main HMTInterface class.
    /// </summary>
    public class HMTService : WebSocketBehavior {

        public string ServiceTarget { get; set; }
        public string AgentId { get; set; }
        public string PuppetId { get; set; }
        public byte CommandPriority { get; set; }
        public string APIKey { get; set; } = string.Empty;
        public HashSet<string> ActionSet { get; set; }

        protected override void OnMessage(MessageEventArgs e) {
            Debug.LogFormat("[HMTPuppetManager] recieved command: {0}", e.Data);

            JObject json;
            try {
                json = JObject.Parse(e.Data);
            }
            catch (JsonReaderException ex) {
                Debug.LogErrorFormat("[HMTPuppetManager] Error parsing JSON: {0}", ex.Message);
                Context.WebSocket.Send(string.Format("{\"status\": \"ERROR\",\"message\": \"Invalid JSON\",\"content\": {0}}", e.Data));
                return;
            }
            
            if (APIKey != string.Empty && json.TryGetDefault("api_key", string.Empty) != APIKey){
                Debug.LogErrorFormat("API Key Mismatch: {0} != {1}", json["api_key"], APIKey);
                Context.WebSocket.Send("{\"status\": \"ERROR\",\"message\": \"Invalid API Key\"}");
                return;
            }
            PuppetCommand newCommand = new PuppetCommand(json, this);
            
            if (!PuppetCommand.VALID_COMMANDS.Contains(newCommand.command)) {
                newCommand.SendErrorResponse(5001, "Command Not Recognized");
                return;
            }
            if(newCommand.HasAction && !ActionSet.Contains(newCommand.action)) {
                newCommand.SendErrorResponse(5002, "Action Not Supported by Puppet");
                return;
            }

            // Enqueue the command to the manager to handle cross thread concurrency issues.
            HMTPuppetManager.Instance.EnqueueCommand(newCommand);
        }

        protected override void OnOpen() {
            Debug.Log("[HMTPuppetManager] Client Connected.");
        }

        protected override void OnClose(CloseEventArgs e) {
            Debug.Log("[HMTPuppetManager] Cliend Disconnected.");
        }

        protected override void OnError(ErrorEventArgs e) {
            Debug.LogErrorFormat("[HMTPuppetManager] Error: {0}", e.Message);
            Debug.LogException(e.Exception);
        }
    }


  public static class WebsocketSharpExtensionMethods {
        public static JObject GetJsonPostData(this HttpRequestEventArgs e) {
            var req = e.Request;
            string json;
            using (var reader = new System.IO.StreamReader(req.InputStream, req.ContentEncoding)) {
                json = reader.ReadToEnd();
            }
            return JObject.Parse(json);
        }

        public static void SendJsonResponse(this HttpRequestEventArgs e, JObject json) {
            var response = e.Response;
            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(json));

            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "application/json";
            response.ContentEncoding = System.Text.Encoding.UTF8;
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

        public static void SendBasicResponse(this HttpRequestEventArgs e, int statusCode, string statusMessage, string content=null) {
            var response = e.Response;
            if (content == null) {
                content = statusMessage;
            }
            var buffer = System.Text.Encoding.UTF8.GetBytes(content);
            response.StatusCode = statusCode;
            response.StatusDescription = statusMessage;
            response.ContentType = "text/plain";
            response.ContentEncoding = System.Text.Encoding.UTF8;
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

        public static T TryGetDefault<T>(this JObject job, string key, T defaultValue) {
            return job.TryGetValue(key, out JToken token) ? token.Value<T>() : defaultValue;
        }   
    }


}