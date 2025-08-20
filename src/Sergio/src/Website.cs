using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sergio;

public sealed record WebsiteConfig(
    string Domain,
    string Root,
    int CacheExpirationSeconds
);

public static class Website {
    public static List<WebsiteConfig>? LoadFromJson(string jsonFilePath) {
        if (!File.Exists(jsonFilePath)) {
            return default;
        }

        return JsonSerializer.Deserialize(
            utf8Json: File.OpenRead(jsonFilePath),
            jsonTypeInfo: WebsiteConfigJsonContext.Default.ListWebsiteConfig);
    }
}

[JsonSerializable(typeof(List<WebsiteConfig>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class WebsiteConfigJsonContext : JsonSerializerContext { }
