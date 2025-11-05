using Substrate.NetApi.Model.Types.Primitive;
using System.Text.Json.Serialization;

namespace CommunityCore.Interest
{
    internal sealed class InterestRegistrationDto : IScaleEncodable
    {
        [JsonPropertyName("eventId")]
        public required long EventId { get; set; }

        [JsonPropertyName("address")]
        public required string Address { get; set; }

        [JsonPropertyName("interestedAt")]
        public required long InterestedAt { get; set; }

        public byte[] Encode()
        {
            return [
                .. Helpers.ScaleEncodeString(EventId.ToString()),
                .. Helpers.ScaleEncodeString(Address),
                .. new U64((ulong)InterestedAt).Encode()
            ];
        }
    }
}
