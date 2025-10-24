using System.Text.Json.Serialization;

namespace CommunityCore.Storage
{
    public record StorageUploadDto
    {
        [JsonPropertyName("key")]
        public required string Key { get; set; }

        [JsonPropertyName("url")]
        public required string Url { get; set; }

        [JsonPropertyName("contentType")]
        public required string ContentType { get; set; }
    }
}
