using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HMT.Puppetry {

    public enum PuppetGroupMembershipType {
        Category,
        Fixed,
        Custom,
    }

    public enum PuppetGroupStateRepresentation {
        Target,
        Union,
        Custom,
    }

    public interface IPuppetGroup : IPuppet {

        public PuppetGroupMembershipType MembershipType { get; }
        
        public PuppetGroupStateRepresentation StateRepresentation { get; }

        public HashSet<string> SubPuppets { get; }

        public void AddSubPuppet(IPuppet puppet);

        public void RemoveSubPuppet(IPuppet puppet_id);

        public bool ContainsSubPuppet(string puppet_id);
    }

}