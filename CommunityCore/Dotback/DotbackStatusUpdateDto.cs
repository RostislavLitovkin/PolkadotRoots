using Substrate.NetApi.Model.Types.Primitive;
using System.Text.Json.Serialization;

namespace CommunityCore.Dotback
{
    internal sealed class DotbackStatusUpdateDto : IScaleEncodable
    {
        [JsonPropertyName("eventId")]
        public required long EventId { get; set; }

        [JsonPropertyName("address")]
        public required string Address { get; set; }

        [JsonPropertyName("paid")]
        public bool? Paid { get; set; }

        [JsonPropertyName("rejected")]
        public bool? Rejected { get; set; }

        [JsonPropertyName("subscanUrl")]
        public string? SubscanUrl { get; set; }

        public byte[] Encode()
        {
            // Encode only the key and provided fields, matching signing expectations
            var parts = new List<byte>();
            parts.AddRange(Helpers.ScaleEncodeString(EventId.ToString()));
            parts.AddRange(Helpers.ScaleEncodeString(Address));
            if (Paid.HasValue) parts.AddRange(new Bool(Paid.Value).Encode());
            parts.AddRange(Helpers.ScaleEncodeString("x"));
            if (Rejected.HasValue) parts.AddRange(new Bool(Rejected.Value).Encode());
            if (SubscanUrl is not null) parts.AddRange(Helpers.ScaleEncodeString(SubscanUrl));
            return parts.ToArray();
        }
    }
}
