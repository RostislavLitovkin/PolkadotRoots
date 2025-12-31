using Substrate.NetApi.Model.Types.Primitive;
using System.Text.Json.Serialization;

namespace CommunityCore.Events
{
    public sealed class EventDto : IScaleEncodable
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; }

        [JsonPropertyName("organizatorAddresses")]
        public List<string>? OrganizatorAddresses { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("image")]
        public string? Image { get; set; }

        [JsonPropertyName("lumaUrl")]
        public string? LumaUrl { get; set; }

        [JsonPropertyName("website")]
        public string? Website { get; set; }

        [JsonPropertyName("mapsUrl")]
        public string? MapsUrl { get; set; }

        [JsonPropertyName("googleMeetsUrl")]
        public string? GoogleMeetsUrl { get; set; }

        [JsonPropertyName("zoomUrl")]
        public string? ZoomUrl { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("emailAddress")]
        public string? EmailAddress { get; set; }

        [JsonPropertyName("capacity")]
        public uint? Capacity { get; set; }

        [JsonPropertyName("price")]
        public string? Price { get; set; }

        [JsonPropertyName("supportsDotback")]
        public bool? SupportsDotback { get; set; }

        [JsonPropertyName("telegram")]
        public string? Telegram { get; set; }

        [JsonPropertyName("discord")]
        public string? Discord { get; set; }

        [JsonPropertyName("x")]
        public string? X { get; set; }

        [JsonPropertyName("youtube")]
        public string? Youtube { get; set; }

        [JsonPropertyName("eventbrite")]
        public string? Eventbrite { get; set; }

        [JsonPropertyName("importedFrom")]
        public string? ImportedFrom { get; set; }

        [JsonPropertyName("timeStart")]
        public long? TimeStart { get; set; }

        [JsonPropertyName("timeEnd")]
        public long? TimeEnd { get; set; }

        public byte[] Encode()
        {
            var encodedAddresses = new List<byte>();

            foreach (var address in OrganizatorAddresses ?? [])
            {
                encodedAddresses.AddRange(Helpers.ScaleEncodeString(address));
            }

            return [
                .. (Id.HasValue ? Helpers.ScaleEncodeString(Id.Value.ToString()) : Array.Empty<byte>()),
                .. encodedAddresses,
                .. (Name is not null ? Helpers.ScaleEncodeString(Name) : Array.Empty<byte>()),
                .. (Description is not null ? Helpers.ScaleEncodeString(Description) : Array.Empty<byte>()),
                .. (Image is not null ? Helpers.ScaleEncodeString(Image) : Array.Empty<byte>()),
                .. (LumaUrl is not null ? Helpers.ScaleEncodeString(LumaUrl) : Array.Empty<byte>()),
                .. (Website is not null ? Helpers.ScaleEncodeString(Website) : Array.Empty<byte>()),
                .. (MapsUrl is not null ? Helpers.ScaleEncodeString(MapsUrl) : Array.Empty<byte>()),
                .. (GoogleMeetsUrl is not null ? Helpers.ScaleEncodeString(GoogleMeetsUrl) : Array.Empty<byte>()),
                .. (ZoomUrl is not null ? Helpers.ScaleEncodeString(ZoomUrl) : Array.Empty<byte>()),
                .. (Country is not null ? Helpers.ScaleEncodeString(Country) : Array.Empty<byte>()),
                .. (Address is not null ? Helpers.ScaleEncodeString(Address) : Array.Empty<byte>()),
                .. (PhoneNumber is not null ? Helpers.ScaleEncodeString(PhoneNumber) : Array.Empty<byte>()),
                .. (EmailAddress is not null ? Helpers.ScaleEncodeString(EmailAddress) : Array.Empty<byte>()),
                .. (Capacity.HasValue ? new U32(Capacity.Value).Encode() : Array.Empty<byte>()),
                .. (Price is not null ? Helpers.ScaleEncodeString(Price) : Array.Empty<byte>()),
                .. new Bool(SupportsDotback ?? false).Encode(),
                .. (Telegram is not null ? Helpers.ScaleEncodeString(Telegram) : Array.Empty<byte>()),
                .. (Discord is not null ? Helpers.ScaleEncodeString(Discord) : Array.Empty<byte>()),
                .. (X is not null ? Helpers.ScaleEncodeString(X) : Array.Empty<byte>()),
                .. (Youtube is not null ? Helpers.ScaleEncodeString(Youtube) : Array.Empty<byte>()),
                .. (Eventbrite is not null ? Helpers.ScaleEncodeString(Eventbrite) : Array.Empty<byte>()),
                .. (ImportedFrom is not null ? Helpers.ScaleEncodeString(ImportedFrom) : Array.Empty<byte>()),
                .. (TimeStart.HasValue ? new U64((ulong)TimeStart.Value).Encode() : new U64(0).Encode()),
                .. (TimeEnd.HasValue ? new U64((ulong)TimeEnd.Value).Encode() : new U64(0).Encode()),
            ];
        }
    }
}
