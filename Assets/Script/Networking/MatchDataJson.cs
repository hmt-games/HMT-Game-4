using System.Collections.Generic;
using Nakama.TinyJson;
using UnityEngine;

public static class MatchDataJson
{
    public static string ActionAndTarget(string action, int target)
    {
        var values = new Dictionary<string, string>
        {
            {"action", action},
            {"target", target.ToString()}
        };

        return values.ToJson();
    }
}
