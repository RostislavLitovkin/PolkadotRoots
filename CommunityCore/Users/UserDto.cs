using System.Text.Json.Serialization;

namespace CommunityCore.Users
{
    public sealed class UserDto : IScaleEncodable
    {
        [JsonPropertyName("address")]
        public required string Address { get; set; }

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("profilePicture")]
        public string? ProfilePicture { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("roles")]
        public List<string> Roles { get; set; } = [];

        public string FullName => $"{FirstName} {LastName}";

        public byte[] Encode()
        {
            return [
                .. Helpers.ScaleEncodeString(Address),
                .. FirstName is not null ? Helpers.ScaleEncodeString(FirstName) : [],
                .. LastName is not null ? Helpers.ScaleEncodeString(LastName) : [],
                .. ProfilePicture is not null ? Helpers.ScaleEncodeString(ProfilePicture) : [],
                .. Description is not null ? Helpers.ScaleEncodeString(Description) : [],
            ];
        }
    }
}
