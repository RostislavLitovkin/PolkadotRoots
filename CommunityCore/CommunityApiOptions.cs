using System.Text.Json;

namespace CommunityCore
{
    public sealed class CommunityApiOptions
    {
        /// <summary>Base URL of your Spring service, e.g. https://api.example.com</summary>
        public Uri BaseAddress { get; init; } = new Uri("https://community.plutolabs.app");


        public static JsonSerializerOptions SerializerOptions => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }
}
