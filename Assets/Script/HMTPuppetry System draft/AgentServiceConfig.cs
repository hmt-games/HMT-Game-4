namespace HMT.Puppetry {
    public struct AgentServiceConfig {
        public string ServiceTarget { get; internal set; }
        public string AgentId { get; internal set; }
        public string PuppetId { get; internal set; }
        public byte CommandPriority { get; internal set; }
        public string APIKey { get; internal set; }

        public static AgentServiceConfig NONE = new AgentServiceConfig("NONE", string.Empty, string.Empty, 255, string.Empty);

        public AgentServiceConfig(string serviceTarget, string agentId, string puppetId, byte commandPriority, string apiKey) {
            ServiceTarget = serviceTarget;
            AgentId = agentId;
            PuppetId = puppetId;
            CommandPriority = commandPriority;
            APIKey = apiKey;
        }

        public AgentServiceConfig(string serviceTarget, string agentId, string puppetId, byte commandPriority) {
            ServiceTarget = serviceTarget;
            AgentId = agentId;
            PuppetId = puppetId;
            CommandPriority = commandPriority;
            APIKey = string.Empty;
        }

        public AgentServiceConfig (string puppetId, byte commandPriority) {
            ServiceTarget = "NONE";
            AgentId = string.Empty;
            PuppetId = puppetId;
            CommandPriority = commandPriority;
            APIKey = string.Empty;
        }
    }


}