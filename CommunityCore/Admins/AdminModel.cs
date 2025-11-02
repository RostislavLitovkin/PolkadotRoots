using System.Net;

namespace CommunityCore.Admins
{
    public sealed class CommunityAdminsApiClient
    {
        public const string BASEPATH = "/api/admins";

        private readonly HttpClient http;
        private readonly CommunityApiOptions options;

        public CommunityAdminsApiClient(HttpClient httpClient, CommunityApiOptions? options = null)
        {
            http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.options = options ?? new CommunityApiOptions();

            if (http.BaseAddress is null)
                http.BaseAddress = this.options.BaseAddress;
        }

        // GET /api/admins/
        public async Task<IReadOnlyList<string>> GetAllAsync(CancellationToken ct = default)
        {
            using var resp = await http.GetAsync($"{BASEPATH}/", ct).ConfigureAwait(false);
            return await Helpers.ReadOrThrowAsync<List<string>>(resp).ConfigureAwait(false);
        }
    }
}
