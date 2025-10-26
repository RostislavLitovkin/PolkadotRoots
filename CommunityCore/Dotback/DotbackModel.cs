using Substrate.NetApi.Model.Types;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CommunityCore.Dotback
{
    public sealed class CommunityDotbacksApiClient
    {
        public const string BASEPATH = "/api/dotbacks";

        private readonly HttpClient http;
        private readonly CommunityApiOptions options;

        public CommunityDotbacksApiClient(HttpClient httpClient, CommunityApiOptions? options = null)
        {
            http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.options = options ?? new CommunityApiOptions();

            if (http.BaseAddress is null)
                http.BaseAddress = this.options.BaseAddress;
        }

        // POST /api/dotbacks/{eventId}/{address}
        public async Task<DotbackDto> UpsertAsync(Account account, DotbackRegistrationDto registration, CancellationToken ct = default)
        {
            if (account is null) throw new ArgumentNullException(nameof(account));
            if (registration.UsdAmount <= 0) throw new ArgumentOutOfRangeException(nameof(registration.UsdAmount));
            if (string.IsNullOrWhiteSpace(registration.ImageUrl)) throw new ArgumentException("imageUrl is required", nameof(registration.ImageUrl));

            var address = account.Value;

            var wrapped = new RestWrapper<DotbackRegistrationDto>(account, registration);

            using var resp = await http.PostAsync(
                $"{BASEPATH}/{registration.EventId}/{address}",
                JsonContent.Create(wrapped, options: CommunityApiOptions.SerializerOptions), ct
            ).ConfigureAwait(false);

            // API returns 201 (created) or 200 (updated)
            return await Helpers.ReadOrThrowAsync<DotbackDto>(resp).ConfigureAwait(false);
        }

        // GET /api/dotbacks?eventId=...&address=...
        public async Task<IReadOnlyList<DotbackDto>> SearchAsync(long? eventId = null, string? address = null, CancellationToken ct = default)
        {
            if (eventId is null && string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Provide eventId, address or both");

            var qp = new List<string>();
            if (eventId is not null) qp.Add($"eventId={eventId}");
            if (!string.IsNullOrWhiteSpace(address)) qp.Add($"address={address}");
            var url = qp.Count > 0 ? $"{BASEPATH}?{string.Join("&", qp)}" : BASEPATH;

            using var resp = await http.GetAsync(url, ct).ConfigureAwait(false);

            if (eventId is not null && !string.IsNullOrWhiteSpace(address))
            {
                if (resp.StatusCode == HttpStatusCode.NotFound) return Array.Empty<DotbackDto>();
                var single = await Helpers.ReadOrThrowAsync<DotbackDto>(resp).ConfigureAwait(false);
                return new[] { single };
            }

            return await Helpers.ReadOrThrowAsync<List<DotbackDto>>(resp).ConfigureAwait(false);
        }

        // Convenience helpers
        public async Task<IReadOnlyList<DotbackDto>> ListByEventAsync(long eventId, CancellationToken ct = default)
            => await SearchAsync(eventId: eventId, address: null, ct).ConfigureAwait(false);

        public async Task<IReadOnlyList<DotbackDto>> ListByAddressAsync(string address, CancellationToken ct = default)
            => await SearchAsync(eventId: null, address: address, ct).ConfigureAwait(false);

        // GET /api/dotbacks/{eventId}/{address}
        public async Task<DotbackDto?> GetAsync(long eventId, string address, CancellationToken ct = default)
        {
            using var resp = await http.GetAsync($"{BASEPATH}/{eventId}/{address}", ct).ConfigureAwait(false);
            if (resp.StatusCode == HttpStatusCode.NotFound) return null;
            return await Helpers.ReadOrThrowAsync<DotbackDto>(resp).ConfigureAwait(false);
        }

        // PATCH /api/dotbacks/{eventId}/{address}/status
        public async Task<DotbackDto?> UpdateStatusAsync(Account admin, long eventId, string address, bool? paid = null, bool? rejected = null, string? subscanUrl = null, CancellationToken ct = default)
        {
            if (admin is null) throw new ArgumentNullException(nameof(admin));

            var update = new DotbackStatusUpdateDto
            {
                EventId = eventId,
                Address = address,
                Paid = paid,
                Rejected = rejected,
                SubscanUrl = subscanUrl
            };

            var wrapped = new RestWrapper<DotbackStatusUpdateDto>(admin, update);

            var json = JsonSerializer.Serialize(wrapped, CommunityApiOptions.SerializerOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var req = new HttpRequestMessage(HttpMethod.Patch, $"{BASEPATH}/{eventId}/{address}/status")
            {
                Content = content
            };

            using var resp = await http.SendAsync(req, ct).ConfigureAwait(false);
            if (resp.StatusCode == HttpStatusCode.NotFound) return null;
            return await Helpers.ReadOrThrowAsync<DotbackDto>(resp).ConfigureAwait(false);
        }
    }
}
