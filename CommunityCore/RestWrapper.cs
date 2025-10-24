using Substrate.NetApi;
using Substrate.NetApi.Model.Types;
using Substrate.NetApi.Model.Types.Primitive;
using System.Text.Json.Serialization;

namespace CommunityCore
{
    internal record RestWrapper<T> where T : IScaleEncodable
    {
        [JsonPropertyName("value")]
        public T Value { get; }

        [JsonPropertyName("signature")]
        public string Signature { get; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; }

        public RestWrapper(Account account, T value)
        {
            Value = value;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Signature = Sign(account);
        }

        private string Sign(Account account)
        {
            byte[] encoded = [
                .. new U64((ulong)Timestamp).Encode(),
                .. Value.Encode()
            ];

            var hash = HashExtension.Blake2(encoded, 128);

            var signature = account.Sign(hash);

            return Utils.Bytes2HexString(signature);
        }

    }
}
