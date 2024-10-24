using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Concurrent;
using Cinemachine;

namespace HMT {
    public class HMTPuppetManager : MonoBehaviour {
        public static HMTPuppetManager Instance { get; private set; }

        [Header("AI Socket Settings")]
        [Tooltip("Whethe rthe server should automatically start itself on Start or wait to be started by an external caller.")]
        public bool StartServerOnStart = false;
        [Tooltip("The URL of the socket server, will default to localhost if empty.")]
        public string socketUrl = "ws://localhost";
        [Tooltip("The port of the socket server.")]
        public int socketPort = 4649;
        [Tooltip("The name of the root service apprended to the url for the puppet manager.")]
        public string rootService = "hmt";
        [Tooltip("The list of API targets that the puppet manager will listen for. These should coorespond to different API types (eg. bots-lowlevel, bots-highlevel, floors, towers, etc.)")]
        public string[] apiTargets = new string[0];
        [Tooltip("The threshold for automatic responses to commands. If a command is not responded to by the target puppet in this time, a generic acknoweldgement will be sent. Note that this is in terms of unscaledTime not regular time so it does not respect speed up or pausing.")]
        public float autoResponseThreshold = 3f;

        [Tooltip("The default priority level for agent commands that do not specify on registration.")]
        [Range(0, 255)]
        public int defaultCommandPriority = 128;

        private ArgParser Args;

        internal WebSocketServer server = null;

        public Dictionary<string, AgentRegistrationRecord> RegisteredAgents;

        private ConcurrentQueue<PuppetCommand> commandQueue;
        private Dictionary<string, int> apiTargetIdCounters;
        private Dictionary<string, Dictionary<string, IPuppet>> PuppetsByTarget;

        private List<(float time, PuppetCommand puppet)> commandsInFlight;

        void Awake() {
            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(this);
            }
            Args = new ArgParser();
            Args.AddArg("hmtsocketurl", ArgParser.ArgType.One);
            Args.AddArg("hmtsocketport", ArgParser.ArgType.One);
            Args.ParseArgs();

            commandQueue = new ConcurrentQueue<PuppetCommand>();
            commandsInFlight = new List<(float time, PuppetCommand puppet)>();

            RegisteredAgents = new Dictionary<string, AgentRegistrationRecord>();

            apiTargetIdCounters = new Dictionary<string, int>();
            PuppetsByTarget = new Dictionary<string, Dictionary<string, IPuppet>>();

            foreach (string target in apiTargets) {
                apiTargetIdCounters.Add(target, 0);
                PuppetsByTarget.Add(target, new Dictionary<string, IPuppet>());
            }
        }

        // Start is called before the first frame update
        void Start() {
            if(StartServerOnStart) {
                StartSocketServer();
            }
        }


        public void StartSocketServer() {
            socketPort = Args.GetArgValue("hmtsocketport", socketPort);
            socketUrl = Args.GetArgValue("hmtsocketurl", socketUrl);

            if (socketPort == 80) {
                Debug.LogWarning("HMTPuppetManager Socket set to Port 80, which will probably have permissions issues.");
            }

            if (socketUrl == string.Empty) {
                Debug.LogWarning("HMTPuppetManager Socket url is empty so opening socket is equivalent to local context.");
                server = new WebSocketServer(socketPort);
            }
            else {
                server = new WebSocketServer(socketUrl + ":" + socketPort);
            }

            foreach (string target in apiTargets) {
                server.AddWebSocketService<HMTService>("/" + rootService + "/" + target);
            }

            server.AddWebSocketService<HMTService>("/" + rootService);
            server.Start();

            foreach (string target in apiTargets) {
                Debug.LogFormat("[HMTPuppetManager] Agent Target available at: {0}:{1}/{2}/{3}",
                socketUrl == string.Empty ? "ws://localhost" : socketUrl, socketPort, rootService, target);
            }
        }

        public void EnqueueCommand(PuppetCommand command) {
            commandQueue.Enqueue(command);
        }

        // Update is called once per frame
        void Update() {
            while(commandQueue.Count > 0) {
                if (commandQueue.TryDequeue(out PuppetCommand command)) {
                    if(!apiTargetIdCounters.ContainsKey(command.targetAPI)) {
                        Debug.LogErrorFormat("[HMTPuppetManager] API Target {0} not found. This should not be possible!?", command.targetAPI);
                        command.SendNotFoundResponse(3000, "API Target not found. This should be impossible...");
                        continue;
                    }

                    switch (command.command) {
                        case "register_agent":
                            RegisterAgent(command);
                            break;
                        case "unregister_agent":
                            UnregisterAgent(command);
                            break;
                        case "list_puppets":
                            ListPuppets(command);
                            break;
                        case "execute_action":
                            ExecuteAction(command);
                            break;
                        case "execute_plan":
                            ExecutePlan(command);
                            break;
                        case "get_state":
                            GetState(command);
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

        public void RegisterAgent(PuppetCommand command) {
            string agentId = command.json["agent_id"].ToString();
            byte priority = command.json.TryGetValue("priority", out JToken priorityToken) ? priorityToken.Value<byte>() : (byte)defaultCommandPriority;
            if (RegisteredAgents.ContainsKey(agentId)) {
                RegisteredAgents[agentId].AddPuppet(command.targetAPI, command.targetPuppet);
            }
            else {
                string sessionID = command.json.TryGetValue("session_id", out JToken sessionIDToken) ? sessionIDToken.ToString() : System.Guid.NewGuid().ToString();
                AgentRegistrationRecord record = new AgentRegistrationRecord(agentId, sessionID, command.targetAPI, command.targetPuppet, priority);
                RegisteredAgents.Add(agentId, record);
            }
            //TODO we may want to do the same thing we did in DiceAdventure where this waited for the game to be ready before responding.
            command.SendAcknowledgeResponse();
        }

        public void UnregisterAgent(PuppetCommand command) {
            string agentId = command.json["agent_id"].ToString();
            if (RegisteredAgents.ContainsKey(agentId)) {
                RegisteredAgents[agentId].RemovePuppet(command.targetAPI, command.targetPuppet);
                if (RegisteredAgents[agentId].PuppetCount == 0) {
                    RegisteredAgents.Remove(agentId);
                }
                command.SendAcknowledgeResponse();
            }
            else {
                command.SendErrorResponse(3001, "Agent not registered.");
            }
        }

        public void ListPuppets(PuppetCommand command) {
            command.SendOKResponse(2001, "Puppets Found", string.Format("{\"puppets\":[{0}]}", string.Join(",", PuppetsByTarget[command.targetAPI].Keys)));
        }

        public void ExecuteAction(PuppetCommand command) {
            if(!RegisteredAgents.ContainsKey(command.agentId)) {
                command.SendErrorResponse(3002, "Agent sent actions before being registered.");
            }
            AgentRegistrationRecord agent = RegisteredAgents[command.agentId];

            if (PuppetsByTarget.ContainsKey(command.targetAPI)) {
                if (PuppetsByTarget[command.targetAPI].ContainsKey(command.targetPuppet)) {
                    IPuppet puppet = PuppetsByTarget[command.targetAPI][command.targetPuppet];
                    if (puppet.ActionSupported(command.targetAPI, command.action)) {
                        StartCoroutine(PuppetsByTarget[command.targetAPI][command.targetPuppet].ExecuteAction(command, agent.priority));
                        commandsInFlight.Add((Time.unscaledTime, command));
                    }
                    else {
                        command.SendIllegalResponse(4000, "Action not supported by Puppet", string.Format("{ \"supported_actions\":[{0}]}", string.Join(",", puppet.SupportedActions(command.targetAPI).Select(x => "\"" + x + "\""))));
                    }
                }
                else {
                    command.SendNotFoundResponse(3001, "Puppet not found at API Target");
                }
            }
            else {
                command.SendNotFoundResponse(3000, "API Target not found");
            }
        }

        public void ExecutePlan(PuppetCommand command) {
            if (!RegisteredAgents.ContainsKey(command.agentId)) {
                command.SendErrorResponse(3002, "Agent sent actions before being registered.");
            }
            AgentRegistrationRecord agent = RegisteredAgents[command.agentId];
        }

        public void GetState(PuppetCommand command) {

        }




        /// <summary>
        /// Adds a Puppet to a given API Target and returns its ID for that target.
        /// </summary>
        /// <param name="apiTarget"></param>
        /// <param name="puppet"></param>
        /// <returns></returns>
        public void AddPuppet(string apiTarget, IPuppet puppet) {
            PuppetsByTarget[apiTarget].Add(puppet.PuppetID, puppet);
        }

        /// <summary>
        /// Removes a Puppet fro ma given API Target and returns its ID for that target.
        /// </summary>
        /// <param name="apiTarget"></param>
        /// <param name="puppetId"></param>
        /// <returns></returns>
        public void RemovePuppet(string apiTarget, IPuppet puppet) {
            if (PuppetsByTarget[apiTarget].ContainsKey(puppet.PuppetID)) {
                PuppetsByTarget[apiTarget].Remove(puppet.PuppetID);
            }
        }

        private string GetNewPuppetId(string target) {
            apiTargetIdCounters[target]++;
            return target + "_" + apiTargetIdCounters[target].ToString();
        }
    }

    /// <summary>
    /// This class is just for facilitating the socket interface. 
    /// 
    /// My goal would be for no logic to actually live here and instead by 
    /// handled by the ExecuteAction virtual method in the main HMTInterface class.
    /// </summary>
    public class HMTService : WebSocketBehavior {

        protected override void OnMessage(MessageEventArgs e) {
            Debug.LogFormat("[HMTPuppetManager] recieved command: {0}", e.Data);

            JObject json;
            try {
                json = JObject.Parse(e.Data);
            }
            catch (Newtonsoft.Json.JsonReaderException ex) {
                Debug.LogErrorFormat("[HMTPuppetManager] Error parsing JSON: {0}", ex.Message);
                Context.WebSocket.Send(string.Format("{\"status\": \"ERROR\",\"message\": \"Invalid JSON\",\"content\": {0}}", e.Data));
                return;
            }
            
            PuppetCommand newCommand = new PuppetCommand(Context.RequestUri.Segments[Context.RequestUri.Segments.Length - 1], json, this);
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

}