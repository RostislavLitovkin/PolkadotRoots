using System.Text.Json.Serialization;

namespace CommunityCore.Events
{
    public sealed class DeleteEventDto : IScaleEncodable
    {
        [JsonPropertyName("id")]
        public required long Id { get; set; }

        public byte[] Encode()
        {
            return Helpers.ScaleEncodeString(Id.ToString());
        }
    }
}
