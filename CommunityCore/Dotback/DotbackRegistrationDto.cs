using Substrate.NetApi.Model.Types.Primitive;
using System.Text.Json.Serialization;

namespace CommunityCore.Dotback
{
    public record DotbackRegistrationDto : IScaleEncodable
    {
        [JsonPropertyName("eventId")]
        public required long EventId { get; set; }

        [JsonPropertyName("address")]
        public required string Address { get; set; }

        [JsonPropertyName("usdAmount")]
        public required double UsdAmount { get; set; }

        [JsonPropertyName("imageUrl")]
        public required string ImageUrl { get; set; }

        public byte[] Encode()
        {
            return [
                .. Helpers.ScaleEncodeString(EventId.ToString()),
                .. Helpers.ScaleEncodeString(Address),
                .. new U64((ulong)(UsdAmount * 100)).Encode(),
                .. Helpers.ScaleEncodeString(ImageUrl),
            ];
        }
    }
}
