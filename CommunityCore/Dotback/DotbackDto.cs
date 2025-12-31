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

        [JsonPropertyName("eventId")] public long? EventIdFlat { get; set; }

        [JsonPropertyName("address")] public string? AddressFlat { get; set; }

        [JsonPropertyName("userAddress")] public string? UserAddressFlat { get; set; }

        [JsonPropertyName("usdAmount")] public double UsdAmount { get; set; }
        [JsonPropertyName("imageUrl")] public string ImageUrl { get; set; } = string.Empty;
        [JsonPropertyName("paid")] public bool Paid { get; set; }
        [JsonPropertyName("rejected")] public bool Rejected { get; set; }
        [JsonPropertyName("subscanUrl")] public string? SubscanUrl { get; set; }
        [JsonPropertyName("unixDateOfRequest")] public long? UnixDateOfRequest { get; set; }
        [JsonPropertyName("dotAmountPaid")] public double? DotAmountPaid { get; set; }
        [JsonPropertyName("unixDatePaid")] public long? UnixDatePaid { get; set; }

        [JsonIgnore]
        public long EventId => Id?.EventId ?? EventIdFlat ?? 0;

        [JsonIgnore]
        public string Address => Id?.UserAddress ?? AddressFlat ?? UserAddressFlat ?? string.Empty;
    }
}
