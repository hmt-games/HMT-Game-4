using Newtonsoft.Json.Linq;

namespace HMT.Puppetry {

    /// <summary>
    /// A set of levels for use in generating state representations for agents.
    /// 
    /// These levels are roughly analogous to the levels we used in Dice Adventure:
    /// * None: The agent gets to know nothing about the object, mainly included for completeness sake.
    /// * Unseen: The agent knows the object exists but have not seen it yet. Analogous to the black fog of war in Dice Adventure.
    /// * Seen: The agent has seen the object and may remember some details about it, but it is not currently visible. Analogous to the grey fog of war in Dice Adventure.
    /// * Visible: The agent can currently see the object. This would typically include any apparent surface features of the object.
    /// * Full: A full representation of the object, including any hidden or internal features. Should be capable of fulling replicating the object.
    /// </summary>
    public enum HMTStateLevelOfDetail {
        None,
        Unseen,
        Seen,
        Visible,
        Full
    }

    public interface IPuppetPerceivable {
        public string ObjectID { get; }

        public JObject HMTStateRep(HMTStateLevelOfDetail level);
    }
}
