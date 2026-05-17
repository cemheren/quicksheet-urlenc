using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuickSheetUrlenc;

class Program
{
    static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    static void Main()
    {
        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            try
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;
                string type = root.GetProperty("type").GetString() ?? "";

                if (type == "init")
                {
                    var resp = new { type = "register", name = "urlenc", version = "1.0.0", prefix = "urlenc" };
                    Console.WriteLine(JsonSerializer.Serialize(resp, JsonOpts));
                    Console.Out.Flush();
                }
                else if (type == "activate")
                {
                    HandleActivate(root);
                }
            }
            catch (Exception ex)
            {
                var err = new { type = "error", message = ex.Message };
                Console.WriteLine(JsonSerializer.Serialize(err, JsonOpts));
                Console.Out.Flush();
            }
        }
    }

    static void HandleActivate(JsonElement root)
    {
        string id = root.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "" : "";

        // Read params: first param is the input string, optional second is mode (encode/decode)
        string input = "";
        string mode = "auto";

        if (root.TryGetProperty("params", out var paramsArr) && paramsArr.ValueKind == JsonValueKind.Array)
        {
            var enumerator = paramsArr.EnumerateArray();
            if (enumerator.MoveNext()) input = enumerator.Current.GetString() ?? "";
            if (enumerator.MoveNext())
            {
                string m = (enumerator.Current.GetString() ?? "").Trim().ToLowerInvariant();
                if (m == "encode" || m == "enc" || m == "e") mode = "encode";
                else if (m == "decode" || m == "dec" || m == "d") mode = "decode";
                else if (m == "component" || m == "comp" || m == "c") mode = "component";
                else if (m == "path" || m == "p") mode = "path";
            }
        }

        var cells = new List<object>();
        int row = 0;

        if (string.IsNullOrEmpty(input))
        {
            cells.Add(new { r = 0, c = 0, v = "⚠ Usage: urlenc: <text>[,encode|decode|component|path]" });
        }
        else if (mode == "auto")
        {
            // Auto-detect: if input contains % sequences, decode; otherwise encode both forms
            bool hasPercent = input.Contains('%') && System.Text.RegularExpressions.Regex.IsMatch(input, @"%[0-9A-Fa-f]{2}");

            if (hasPercent)
            {
                string decoded = Uri.UnescapeDataString(input);
                cells.Add(new { r = row, c = 0, v = $"Decoded: {decoded}" });
                row++;
                cells.Add(new { r = row, c = 0, v = $"Re-encoded: {Uri.EscapeDataString(decoded)}" });
            }
            else
            {
                string component = Uri.EscapeDataString(input);
                cells.Add(new { r = row, c = 0, v = $"Component: {component}" });
                row++;
                // Also show full URI encoding (preserves :/?#[]@!$&'()*+,;=)
                string full = EscapeFullUri(input);
                cells.Add(new { r = row, c = 0, v = $"Full URI: {full}" });
                row++;
                string decoded = Uri.UnescapeDataString(input);
                if (decoded != input)
                {
                    cells.Add(new { r = row, c = 0, v = $"Decoded: {decoded}" });
                }
            }
        }
        else if (mode == "encode")
        {
            cells.Add(new { r = 0, c = 0, v = Uri.EscapeDataString(input) });
        }
        else if (mode == "decode")
        {
            cells.Add(new { r = 0, c = 0, v = Uri.UnescapeDataString(input) });
        }
        else if (mode == "component")
        {
            cells.Add(new { r = 0, c = 0, v = Uri.EscapeDataString(input) });
        }
        else if (mode == "path")
        {
            // Encode for path segment — same as component but preserve /
            string encoded = Uri.EscapeDataString(input).Replace("%2F", "/");
            cells.Add(new { r = 0, c = 0, v = encoded });
        }

        var resp = new { type = "write", id, cells };
        Console.WriteLine(JsonSerializer.Serialize(resp, JsonOpts));
        Console.Out.Flush();
    }

    static string EscapeFullUri(string input)
    {
        // Like encodeURI in JS — encode everything except :/?#[]@!$&'()*+,;=-._~
        var sb = new StringBuilder();
        foreach (char ch in input)
        {
            if (IsUnreservedOrReserved(ch))
                sb.Append(ch);
            else
            {
                foreach (byte b in Encoding.UTF8.GetBytes(ch.ToString()))
                    sb.AppendFormat("%{0:X2}", b);
            }
        }
        return sb.ToString();
    }

    static bool IsUnreservedOrReserved(char ch)
    {
        // RFC 3986 unreserved + reserved characters
        if (ch >= 'A' && ch <= 'Z') return true;
        if (ch >= 'a' && ch <= 'z') return true;
        if (ch >= '0' && ch <= '9') return true;
        return "-._~:/?#[]@!$&'()*+,;=".Contains(ch);
    }
}
