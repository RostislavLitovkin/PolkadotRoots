using System.Text.Json.Serialization;

namespace CommunityCore.Interest
{
    internal sealed class DeleteInterestDto : IScaleEncodable
    {
        [JsonPropertyName("eventId")]
        public required long EventId { get; set; }

        [JsonPropertyName("address")]
        public required string Address { get; set; }

        public byte[] Encode()
        {
            return [
                .. Helpers.ScaleEncodeString(EventId.ToString()),
                .. Helpers.ScaleEncodeString(Address)
            ];
        }
    }
}
