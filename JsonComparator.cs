using Newtonsoft.Json.Linq;

namespace MapJsonComparator;

public static class JsonComparator
{
    public static void CompareJsonTokens(JToken oldToken, JToken newToken, string path, List<string> diffs)
    {
        if (oldToken.Type != newToken.Type)
        {
            diffs.Add($"Different types: {path}: ({oldToken.Type}, {newToken.Type})"); // should not happen
            return;
        }

        switch (oldToken.Type)
        {
            case JTokenType.Object:
                CompareJsonObjects((JObject)oldToken, (JObject)newToken, path, diffs);
                break;

            case JTokenType.Array:
                CompareJsonArrays((JArray)oldToken, (JArray)newToken, path, diffs);
                break;

            default:
                CompareJsonValues((JValue)oldToken, (JValue)newToken, path, diffs);
                break;
        }
    }

    private static void CompareJsonArrays(JArray oldArray, JArray newArray, string path, List<string> diffs)
    {
        // use "id" if possible
        bool hasIdOld = oldArray.All(token => token.Type == JTokenType.Object && ((JObject)token).Property("id")?.Value.Type == JTokenType.String);
        bool hasIdNew = newArray.All(token => token.Type == JTokenType.Object && ((JObject)token).Property("id")?.Value.Type == JTokenType.String);

        if (hasIdOld && hasIdNew)
        {
            Func<JObject, string> getKey = static o =>
            {
                if (o.Property("nodeId") != null)
                {
                    return o.Value<string>("nodeId");
                }
                else
                {
                    return o.Value<string>("id");
                }
            };
            var oldDict = oldArray.Cast<JObject>().ToDictionary(getKey);
            var newDict = newArray.Cast<JObject>().ToDictionary(getKey);
            var allIds = new HashSet<string>(oldDict.Keys);
            allIds.UnionWith(newDict.Keys);

            foreach (var id in allIds)
            {
                var childPath = $"{path}[id={id}]";

                if (!oldDict.ContainsKey(id))
                    diffs.Add($"added: {childPath}");
                else if (!newDict.ContainsKey(id))
                    diffs.Add($"removed: {childPath}");
                else
                    CompareJsonObjects(oldDict[id], newDict[id], childPath, diffs);
            }
        }
        else
        {
            // Fallback: if id not exists, compare by index
            int max = Math.Max(oldArray.Count, newArray.Count);
            for (int i = 0; i < max; i++)
            {
                var childPath = $"{path}[{i}]";
                if (i >= oldArray.Count)
                    diffs.Add($"added: {childPath}");
                else if (i >= newArray.Count)
                    diffs.Add($"removed: {childPath}");
                else
                    CompareJsonTokens(oldArray[i], newArray[i], childPath, diffs);
            }
        }
    }

    private static void CompareJsonValues(JValue oldValue, JValue newValue, string path, List<string> diffs)
    {
        if (!object.Equals(oldValue, newValue))
            diffs.Add($"changed: {path} (old=\"{oldValue}\", new=\"{newValue}\")");
    }

    private static void CompareJsonObjects(JObject oldObject, JObject newObject, string path, List<string> diffs)
    {
        var oldKeys = new HashSet<string>(oldObject.Properties().Select(p => p.Name));
        var newKeys = new HashSet<string>(newObject.Properties().Select(p => p.Name));

        var addedKeys = newKeys.Except(oldKeys);
        var removedKeys = oldKeys.Except(newKeys);
        var changedKeys = oldKeys.Intersect(newKeys);

        foreach (var key in addedKeys)
        {
            var childPath = string.IsNullOrEmpty(path) ? key : $"{path}.{key}";
            diffs.Add($"added: {childPath}");
        }

        foreach (var key in removedKeys)
        {
            var childPath = string.IsNullOrEmpty(path) ? key : $"{path}.{key}";
            diffs.Add($"removed: {childPath}");
        }

        foreach (var key in changedKeys)
        {
            var childPath = string.IsNullOrEmpty(path) ? key : $"{path}.{key}";
            CompareJsonTokens(oldObject[key], newObject[key], childPath, diffs);
        }
    }
}
