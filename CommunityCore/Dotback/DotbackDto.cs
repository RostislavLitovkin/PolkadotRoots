using System.Text.Json.Serialization;

namespace CommunityCore.Dotback
{
    public sealed class DotbackDto
    {
        public sealed class DotbackIdDto
        {
            [JsonPropertyName("eventId")] public long EventId { get; set; }
            [JsonPropertyName("userAddress")] public string UserAddress { get; set; } = string.Empty;
        }

        [JsonPropertyName("id")] public DotbackIdDto? Id { get; set; }

        [JsonPropertyName("usdAmount")] public double UsdAmount { get; set; }
        [JsonPropertyName("imageUrl")] public string ImageUrl { get; set; } = string.Empty;
        [JsonPropertyName("paid")] public bool Paid { get; set; }
        [JsonPropertyName("rejected")] public bool Rejected { get; set; }
        [JsonPropertyName("subscanUrl")] public string? SubscanUrl { get; set; }

        [JsonIgnore]
        public long EventId => Id?.EventId ?? 0;

        [JsonIgnore]
        public string Address => Id?.UserAddress ?? string.Empty;
    }
}
