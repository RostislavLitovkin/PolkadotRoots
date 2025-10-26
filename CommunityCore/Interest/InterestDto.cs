using System.Text.Json.Serialization;

namespace CommunityCore.Interest
{
    public sealed class InterestDto
    {
        [JsonPropertyName("eventId")]
        public required long EventId { get; set; }

        [JsonPropertyName("address")]
        public required string Address { get; set; }

        [JsonPropertyName("interestedAt")]
        public required long InterestedAt { get; set; }
    }
}
