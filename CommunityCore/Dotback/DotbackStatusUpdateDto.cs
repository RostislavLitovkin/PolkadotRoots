using Substrate.NetApi.Model.Types.Primitive;
using System.Globalization;
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

        [JsonPropertyName("dotAmountPaid")]
        public double? DotAmountPaid { get; set; }

        [JsonPropertyName("unixDatePaid")]
        public long? UnixDatePaid { get; set; }

        public byte[] Encode()
        {
            var parts = new List<byte>();
            parts.AddRange(Helpers.ScaleEncodeString(EventId.ToString()));
            parts.AddRange(Helpers.ScaleEncodeString(Address));
            if (Paid.HasValue) parts.AddRange(new Bool(Paid.Value).Encode());
            // separator to mirror server encoding
            parts.AddRange(Helpers.ScaleEncodeString("x"));
            if (Rejected.HasValue) parts.AddRange(new Bool(Rejected.Value).Encode());
            if (SubscanUrl is not null) parts.AddRange(Helpers.ScaleEncodeString(SubscanUrl));
            if (DotAmountPaid.HasValue) parts.AddRange(Helpers.ScaleEncodeString(DotAmountPaid.Value.ToString(CultureInfo.InvariantCulture)));
            if (UnixDatePaid.HasValue) parts.AddRange(new U64((ulong)UnixDatePaid.Value).Encode());
            return parts.ToArray();
        }
    }
}
