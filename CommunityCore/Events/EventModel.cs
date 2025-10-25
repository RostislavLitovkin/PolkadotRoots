using Substrate.NetApi.Model.Types;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CommunityCore.Events
{
    public sealed class CommunityEventsApiClient
    {
        public const string BASEPATH = "/api/events";

        private readonly HttpClient http;
        private readonly CommunityApiOptions options;

        public CommunityEventsApiClient(HttpClient httpClient, CommunityApiOptions? options = null)
        {
            http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.options = options ?? new CommunityApiOptions();

            if (http.BaseAddress is null)
                http.BaseAddress = this.options.BaseAddress;
        }

        // GET /api/events/
        public async Task<IReadOnlyList<EventDto>> GetAllAsync(CancellationToken ct = default)
        {
            using var resp = await http.GetAsync($"{BASEPATH}/", ct).ConfigureAwait(false);
            return await Helpers.ReadOrThrowAsync<List<EventDto>>(resp).ConfigureAwait(false);
        }

        // GET /api/events/{id}
        public async Task<EventDto?> GetAsync(long id, CancellationToken ct = default)
        {
            using var resp = await http.GetAsync($"{BASEPATH}/{id}", ct).ConfigureAwait(false);
            if (resp.StatusCode == HttpStatusCode.NotFound) return null;
            return await Helpers.ReadOrThrowAsync<EventDto>(resp).ConfigureAwait(false);
        }

        // POST /api/events/{signer}
        public async Task<EventDto> CreateAsync(Account account, EventDto eventData, CancellationToken ct = default)
        {
            if (eventData is null) throw new ArgumentNullException(nameof(eventData));

            // Ensure server creates the ID
            eventData.Id = null;

            var wrapped = new RestWrapper<EventDto>(account, eventData);
            var signer = account.Value;

            using var resp = await http.PostAsync(
                $"{BASEPATH}/{signer}",
                JsonContent.Create(wrapped, options: CommunityApiOptions.SerializerOptions), ct
            ).ConfigureAwait(false);

            return await Helpers.ReadOrThrowAsync<EventDto>(resp, expected: HttpStatusCode.Created).ConfigureAwait(false);
        }

        // PUT /api/events/{id}/{signer}
        public async Task<EventDto> PutAsync(Account account, long? id, EventDto eventData, CancellationToken ct = default)
        {
            if (eventData is null) throw new ArgumentNullException(nameof(eventData));

            eventData.Id = id;
            var wrapped = new RestWrapper<EventDto>(account, eventData);
            var signer = account.Value;

            using var resp = await http.PutAsync(
                $"{BASEPATH}/{id}/{signer}",
                JsonContent.Create(wrapped, options: CommunityApiOptions.SerializerOptions), ct
            ).ConfigureAwait(false);

            return await Helpers.ReadOrThrowAsync<EventDto>(resp).ConfigureAwait(false);
        }

        // PATCH /api/events/{id}/{signer}
        public async Task<EventDto?> PatchAsync(Account account, long id, EventDto patch, CancellationToken ct = default)
        {
            if (patch is null) throw new ArgumentNullException(nameof(patch));

            var wrapped = new RestWrapper<EventDto>(account, patch);
            var signer = account.Value;

            var json = JsonSerializer.Serialize(wrapped, CommunityApiOptions.SerializerOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var req = new HttpRequestMessage(HttpMethod.Patch, $"{BASEPATH}/{id}/{signer}")
            {
                Content = content
            };

            using var resp = await http.SendAsync(req, ct).ConfigureAwait(false);
            if (resp.StatusCode == HttpStatusCode.NotFound) return null;
            return await Helpers.ReadOrThrowAsync<EventDto>(resp).ConfigureAwait(false);
        }

        // DELETE /api/events/{id}/{signer}
        public async Task<bool> DeleteAsync(Account account, long id, CancellationToken ct = default)
        {
            var delete = new DeleteEventDto { Id = id };
            var wrapped = new RestWrapper<DeleteEventDto>(account, delete);
            var signer = account.Value;

            var json = JsonSerializer.Serialize(wrapped, CommunityApiOptions.SerializerOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var req = new HttpRequestMessage(HttpMethod.Delete, $"{BASEPATH}/{id}/{signer}")
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
