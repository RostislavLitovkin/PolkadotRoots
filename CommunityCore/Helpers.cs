using System.Net;
using System.Text.Json;

namespace CommunityCore
{

    internal class Helpers
    {
        internal static async Task<T> ReadOrThrowAsync<T>(HttpResponseMessage resp, HttpStatusCode? expected = null)
        {
            if (expected.HasValue && resp.StatusCode != expected.Value)
                throw await CreateApiExceptionAsync(resp).ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode)
                throw await CreateApiExceptionAsync(resp).ConfigureAwait(false);

            var payload = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                var value = JsonSerializer.Deserialize<T>(payload, CommunityApiOptions.SerializerOptions);
                if (value is null)
                    throw new CommunityApiException("Empty or invalid JSON.", resp.StatusCode, payload);
                return value;
            }
            catch (JsonException jx)
            {
                throw new CommunityApiException("Failed to deserialize response.", resp.StatusCode, payload, jx);
            }
        }

        internal static async Task<CommunityApiException> CreateApiExceptionAsync(HttpResponseMessage resp)
        {
            var body = resp.Content is null ? null : await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            return new CommunityApiException($"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}", resp.StatusCode, body);
        }
    }
}
