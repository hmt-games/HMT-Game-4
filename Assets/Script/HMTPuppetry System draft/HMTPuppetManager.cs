using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using WebSocketSharp.Server;
using System.Collections.Concurrent;
using System.Net;
using System.Security.Policy;

namespace HMT.Puppetry {
    public class HMTPuppetManager : MonoBehaviour {
        public static HMTPuppetManager Instance { get; private set; }
        private static int SERVICE_TARGET_COUNTER = 0;

        public enum PuppetryStatus {
            Intializing,
            Running,
            Paused
        }

        public PuppetryStatus Status { get; private set; } = PuppetryStatus.Intializing;

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
        [Tooltip("The threshold for automatic responses to commands. If a Command is not responded to by the target puppet in this time, a generic acknoweldgement will be sent. Note that this is in terms of unscaledTime not regular time so it does not respect speed up or pausing.")]
        public float autoResponseThreshold = 3f;
        [Tooltip("The default priority level for agent commands that do not specify on registration.")]
        [Range(0, 255)]
        public int defaultCommandPriority = 128;

        internal HttpServer server = null;

        private ArgParser Args;
        private ConcurrentQueue<PuppetCommand> commandQueue;
        private List<(float time, PuppetCommand puppet)> commandsInFlight;
        private Dictionary<string, IPuppet> PuppetIndex = new Dictionary<string, IPuppet>();

        private Dictionary<string, HashSet<AgentServiceConfig>> configsByAgent = new Dictionary<string, HashSet<AgentServiceConfig>>();
        private Dictionary<string, HashSet<AgentServiceConfig>> configsByPuppet = new Dictionary<string, HashSet<AgentServiceConfig>>();
        private Dictionary<string, AgentServiceConfig> configsByService = new Dictionary<string, AgentServiceConfig>();

        public HashSet<AgentServiceConfig> GetAgentConfigs(string agent) {
            if (!configsByAgent.ContainsKey(agent)) {
                return new HashSet<AgentServiceConfig>();
            }
            return configsByAgent[agent];
        }

        public HashSet<AgentServiceConfig> GetPuppetConfigs(string puppet) {
            if (!configsByPuppet.ContainsKey(puppet)) {
                return new HashSet<AgentServiceConfig>();
            }
            return configsByPuppet[puppet];
        }

        public AgentServiceConfig GetServiceConfig(string service) {
            if (!configsByService.ContainsKey(service)) {
                return AgentServiceConfig.NONE;
            }
            return configsByService[service];
        }


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

            PuppetIndex = new Dictionary<string, IPuppet>();


            commandQueue = new ConcurrentQueue<PuppetCommand>();
            commandsInFlight = new List<(float time, PuppetCommand puppet)>();
        }

        // Start is called before the first frame update
        void Start() {
            if (StartServerOnStart) {
                StartHMTServer();
            }
        }

        private void OnDestroy() {
            if (server != null) {
                server.Stop();
            }
        }

        public void StartHMTServer() {
            socketPort = Args.GetArgValue("hmtsocketport", socketPort);

            if (socketPort == 80) {
                Debug.LogWarning("HMTPuppetManager Socket set to Port 80, this might cause permissions issues.");
            }

            server = new HttpServer(socketPort);
            server.OnGet += Server_OnGet;
            server.OnPost += Server_OnPost;
            server.Start();

            Debug.LogFormat("[HMTPuppetManager] Server Started {0}", server.DocumentRootPath);

            Status = PuppetryStatus.Running;
        }

        private void Server_OnPost(object sender, HttpRequestEventArgs e) {
            string[] full_path = e.Request.RawUrl.Split('/');
            string path = full_path[full_path.Length - 1];
            Debug.LogFormat("[HMTPuppetManager] POST Request recieved at {0}", path);

            switch (path) {
                case "register_agent":
                    JObject postData = e.GetJsonPostData();
                    string agentId = postData["agent_id"].ToString();
                    string puppetId = postData["puppet_id"].ToString();
                    string sessionID = postData.TryGetDefault("session_id", System.Guid.NewGuid().ToString());
                    byte priority = postData.TryGetDefault("priority", (byte)defaultCommandPriority);
                    
                    if(!PuppetIndex.ContainsKey(puppetId)) {
                        Debug.LogWarning("Puppet ID not found in Puppet Index");
                        e.SendBasicResponse((int)HttpStatusCode.BadRequest, "Puppet ID not found in Puppet Index");
                        return;
                    }
                    
                    AgentServiceConfig config = LaunchNewServiceTarget(agentId, puppetId, priority);

                    JObject response = new JObject {
                        //{ "service_target", string.Format("ws://localhost:{0}/{1}/{2}", socketPort, rootService, config.ServiceTarget) },
                        { "service_target", string.Format("ws://localhost:{0}/{1}", socketPort, config.ServiceTarget) },
                        { "session_id", sessionID },
                        { "agent_id", agentId },
                        { "puppet_id", puppetId },
                        { "priority", priority},
                        { "action_set", new JArray(PuppetIndex[puppetId].SupportedActions) }
                    };
                    if (useAPIKeys) {
                        response["api_key"] = config.APIKey;
                    }
                    e.SendJsonResponse(response);
                    break;
                case "list_puppets":
                    e.SendJsonResponse(ListPuppets());
                    break;
                default:
                    e.SendBasicResponse((int)HttpStatusCode.NotFound, "path does not exist");
                    break;
            }
        }

        private void Server_OnGet(object sender, HttpRequestEventArgs e) {
            string[] full_path = e.Request.RawUrl.Split('/');
            string path = full_path[full_path.Length - 1];
            Debug.LogFormat("[HMTPuppetManager] GET Request recieved {0}", path);

            switch (path) {
                //TODO we should just pull the relevant piece off the end of the URL this is a short term hack
                case "list_puppets":
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

        internal AgentServiceConfig LaunchNewServiceTarget(string agent_id, string puppet_id, byte priority) {
            string newServiceTarget = string.Empty;
            string apiKey = useAPIKeys ? System.Guid.NewGuid().ToString() : string.Empty;
            if (useSequentialServiceTargets) {
                newServiceTarget = (SERVICE_TARGET_COUNTER++).ToString();
            }
            else {
                newServiceTarget = System.Guid.NewGuid().ToString();
            }

            AgentServiceConfig record = new AgentServiceConfig(newServiceTarget, agent_id, puppet_id, priority,apiKey);

            server.AddWebSocketService<HMTPuppetService>("/" + newServiceTarget, s => {
                s.ServiceConfig = record;
                s.ActionSet = PuppetIndex[puppet_id].SupportedActions;
            });

            Debug.LogFormat("[HMTPuppetManager] Launched Service target {0}, {1}, {2}, {3}", newServiceTarget, agent_id, puppet_id, apiKey);

            if (!configsByAgent.ContainsKey(record.AgentId)) {
                configsByAgent[record.AgentId] = new HashSet<AgentServiceConfig>();
            }
            configsByAgent[record.AgentId].Add(record);
            if (!configsByPuppet.ContainsKey(record.PuppetId)) {
                configsByPuppet[record.PuppetId] = new HashSet<AgentServiceConfig>();
            }
            configsByPuppet[record.PuppetId].Add(record);
            configsByService[record.ServiceTarget] = record;

            return record;
        }

        internal void EnqueueCommand(PuppetCommand command) {
            commandQueue.Enqueue(command);
        }

        // Update is called once per frame
        void Update() {
            while(commandQueue.Count > 0) {
                if (commandQueue.TryDequeue(out PuppetCommand command)) {
                    IPuppet puppet = PuppetIndex[command.TargetPuppet];
                    switch (command.Command) {
                        case PuppetCommandType.EXECUTE_ACTION:
                            puppet.ExecuteAction(command);
                            commandsInFlight.Add((Time.unscaledTime, command));
                            break;
                        case PuppetCommandType.EXECUTE_PLAN:
                            puppet.ExecutePlan(command);
                            commandsInFlight.Add((Time.unscaledTime, command));
                            break;
                        case PuppetCommandType.GET_STATE:
                            JObject state = puppet.GetState(command);
                            command.SendStateResponse(state);
                            break;
                        case PuppetCommandType.COMMUNICATE:
                            puppet.ExecuteCommunicate(command);
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
            Debug.LogFormat("[HMTPuppetManager] Adding Puppet: {0}", puppet.PuppetID);
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

        public void PausePuppetInterface() {
            Status = PuppetryStatus.Paused;
        }

        public void RestartPuppetInterface() {
            Status = PuppetryStatus.Running;
        }
    }


}