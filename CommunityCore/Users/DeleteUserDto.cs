using System.Text.Json.Serialization;

namespace CommunityCore.Users
{
    internal record DeleteUserDto : IScaleEncodable
    {
        [JsonPropertyName("address")]
        public required string Address { get; set; }
        public byte[] Encode()
        {
            return Helpers.ScaleEncodeString(Address);
        }
    }
}
