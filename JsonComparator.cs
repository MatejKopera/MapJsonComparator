using Newtonsoft.Json.Linq;

namespace MapJsonComparator;

public static class JsonComparator
{
    public static void CompareJsonTokens(JToken a, JToken b, string path, List<string> diffs)
    {
        if (a.Type != b.Type)
        {
            diffs.Add($"Different types: {path}: ({a.Type}, {b.Type})"); // should not happen
            return;
        }

        switch (a.Type)
        {
            case JTokenType.Object:
                CompareJsonObjects((JObject)a, (JObject)b, path, diffs);
                break;

            case JTokenType.Array:
                CompareJsonArrays((JArray)a, (JArray)b, path, diffs);
                break;

            default:
                CompareJsonValues((JValue)a, (JValue)b, path, diffs);
                break;
        }
    }

    private static void CompareJsonArrays(JArray arrA, JArray arrB, string path, List<string> diffs)
    {
        // use "id" if possible
        bool hasIdA = arrA.All(t => t.Type == JTokenType.Object && ((JObject)t).Property("id")?.Value.Type == JTokenType.String);
        bool hasIdB = arrB.All(t => t.Type == JTokenType.Object && ((JObject)t).Property("id")?.Value.Type == JTokenType.String);

        if (hasIdA && hasIdB)
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
            var dictA = arrA.Cast<JObject>().ToDictionary(getKey);
            var dictB = arrB.Cast<JObject>().ToDictionary(getKey);
            var allIds = new HashSet<string>(dictA.Keys);
            allIds.UnionWith(dictB.Keys);

            foreach (var id in allIds)
            {
                var childPath = $"{path}[id={id}]";
                if (!dictA.ContainsKey(id))
                    diffs.Add($"added: {childPath}");
                else if (!dictB.ContainsKey(id))
                    diffs.Add($"removed: {childPath}");
                else
                    CompareJsonObjects(dictA[id], dictB[id], childPath, diffs);
            }
        }
        else
        {
            // Fallback: if id not exists, compare by index
            int max = Math.Max(arrA.Count, arrB.Count);
            for (int i = 0; i < max; i++)
            {
                var childPath = $"{path}[{i}]";
                if (i >= arrA.Count)
                    diffs.Add($"added: {childPath}");
                else if (i >= arrB.Count)
                    diffs.Add($"removed: {childPath}");
                else
                    CompareJsonTokens(arrA[i], arrB[i], childPath, diffs);
            }
        }
    }

    private static void CompareJsonValues(JValue valA, JValue valB, string path, List<string> diffs)
    {
        if (!object.Equals(valA, valB))
            diffs.Add($"changed: {path}: (old=\"{valB}\", new=\"{valA}\")");
    }

    private static void CompareJsonObjects(JObject objA, JObject objB, string path, List<string> diffs)
    {
        var allKeys = new HashSet<string>(objA.Properties().Select(p => p.Name));
        allKeys.UnionWith(objB.Properties().Select(p => p.Name));
        foreach (var key in allKeys)
        {
            var childPath = string.IsNullOrEmpty(path) ? key : $"{path}.{key}";
            if (!objA.TryGetValue(key, out var va))
                diffs.Add($"added: {childPath}");
            else if (!objB.TryGetValue(key, out var vb))
                diffs.Add($"removed: {childPath}");
            else
                CompareJsonTokens(va, vb, childPath, diffs);
        }
    }
}
