using Substrate.NetApi.Model.Types;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CommunityCore.Interest
{
    public sealed class CommunityInterestApiClient
    {
        public const string BASEPATH = "/api/events";

        private readonly HttpClient http;
        private readonly CommunityApiOptions options;

        public CommunityInterestApiClient(HttpClient httpClient, CommunityApiOptions? options = null)
        {
            http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.options = options ?? new CommunityApiOptions();

            if (http.BaseAddress is null)
                http.BaseAddress = this.options.BaseAddress;
        }

        // POST /api/events/{eventId}/interest/{address}
        public async Task<InterestDto> RegisterAsync(Account account, long eventId, long interestedAt, CancellationToken ct = default)
        {
            if (account is null) throw new ArgumentNullException(nameof(account));

            var address = account.Value;
            var registration = new InterestRegistrationDto
            {
                EventId = eventId,
                Address = address,
                InterestedAt = interestedAt
            };

            var wrapped = new RestWrapper<InterestRegistrationDto>(account, registration);

            using var resp = await http.PostAsync(
                $"{BASEPATH}/{eventId}/interest/{address}",
                JsonContent.Create(wrapped, options: CommunityApiOptions.SerializerOptions), ct
            ).ConfigureAwait(false);

            // API returns 201 (created) or 200 (updated)
            return await Helpers.ReadOrThrowAsync<InterestDto>(resp).ConfigureAwait(false);
        }

        // GET /api/events/{eventId}/interest
        public async Task<IReadOnlyList<InterestDto>> ListAsync(long eventId, CancellationToken ct = default)
        {
            using var resp = await http.GetAsync($"{BASEPATH}/{eventId}/interest", ct).ConfigureAwait(false);
            return await Helpers.ReadOrThrowAsync<List<InterestDto>>(resp).ConfigureAwait(false);
        }

        // GET /api/events/{eventId}/interest/count
        public async Task<long> CountAsync(long eventId, CancellationToken ct = default)
        {
            using var resp = await http.GetAsync($"{BASEPATH}/{eventId}/interest/count", ct).ConfigureAwait(false);
            return await Helpers.ReadOrThrowAsync<long>(resp).ConfigureAwait(false);
        }

        // DELETE /api/events/{eventId}/interest/{address}
        public async Task<bool> DeleteAsync(Account account, long eventId, CancellationToken ct = default)
        {
            if (account is null) throw new ArgumentNullException(nameof(account));

            var address = account.Value;
            var delete = new DeleteInterestDto
            {
                EventId = eventId,
                Address = address
            };

            var wrapped = new RestWrapper<DeleteInterestDto>(account, delete);
            var json = JsonSerializer.Serialize(wrapped, CommunityApiOptions.SerializerOptions);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var req = new HttpRequestMessage(HttpMethod.Delete, $"{BASEPATH}/{eventId}/interest/{address}")
            {
                Content = content
            };

            using var resp = await http.SendAsync(req, ct).ConfigureAwait(false);

            return resp.StatusCode switch
            {
                HttpStatusCode.NoContent => true,
                HttpStatusCode.NotFound => false,
                _ => throw await Helpers.CreateApiExceptionAsync(resp).ConfigureAwait(false)
            };
        }
    }
}

