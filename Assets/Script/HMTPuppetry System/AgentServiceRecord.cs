namespace HMT.Puppetry {
    public struct AgentServiceRecord {
        public string ServiceTarget { get; internal set; }
        public string AgentId { get; internal set; }
        public string PuppetId { get; internal set; }
        public byte CommandPriority { get; internal set; }
        public string APIKey { get; internal set; }

        public static AgentServiceRecord NONE = new AgentServiceRecord("NONE", string.Empty, string.Empty, 255, string.Empty);

        public AgentServiceRecord(string serviceTarget, string agentId, string puppetId, byte commandPriority, string apiKey) {
            ServiceTarget = serviceTarget;
            AgentId = agentId;
            PuppetId = puppetId;
            CommandPriority = commandPriority;
            APIKey = apiKey;
        }

        public AgentServiceRecord(string serviceTarget, string agentId, string puppetId, byte commandPriority) {
            ServiceTarget = serviceTarget;
            AgentId = agentId;
            PuppetId = puppetId;
            CommandPriority = commandPriority;
            APIKey = string.Empty;
        }

        public AgentServiceRecord (string puppetId, byte commandPriority) {
            ServiceTarget = "NONE";
            AgentId = string.Empty;
            PuppetId = puppetId;
            CommandPriority = commandPriority;
            APIKey = string.Empty;
        }
    }


}