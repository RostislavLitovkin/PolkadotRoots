using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CommunityCore.Users;

public sealed class CommunityUsersApiClient
{
    public const string BASEPATH = "/api/users";

    private readonly HttpClient http;
    private readonly CommunityApiOptions options;
    public CommunityUsersApiClient(HttpClient httpClient, CommunityApiOptions? options = null)
    {
        this.http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.options = options ?? new CommunityApiOptions();

        if (this.http.BaseAddress is null)
            this.http.BaseAddress = this.options.BaseAddress;
    }

    // GET /api/users/
    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        using var resp = await http.GetAsync($"{BASEPATH}/", ct).ConfigureAwait(false);
        return await Helpers.ReadOrThrowAsync<List<UserDto>>(resp).ConfigureAwait(false);
    }

    // GET /api/users/{address}
    public async Task<UserDto?> GetAsync(string address, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("address is required", nameof(address));
        using var resp = await http.GetAsync($"{BASEPATH}/{address}", ct).ConfigureAwait(false);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        return await Helpers.ReadOrThrowAsync<UserDto>(resp).ConfigureAwait(false);
    }

    // POST /api/users/  (creates; 201; 400/409 error)
    public async Task<UserDto> CreateAsync(UserDto user, CancellationToken ct = default)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));
        using var resp = await http.PostAsync($"{BASEPATH}/", JsonContent.Create(user, options: CommunityApiOptions.SerializerOptions), ct).ConfigureAwait(false);
        return await Helpers.ReadOrThrowAsync<UserDto>(resp, expected: HttpStatusCode.Created).ConfigureAwait(false);
    }

    // PUT /api/users/{address}  (upsert)
    public async Task<UserDto> PutAsync(string address, UserDto user, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("address is required", nameof(address));
        if (user is null) throw new ArgumentNullException(nameof(user));
        // Server ignores body.address and uses path, but we set it for clarity
        user.Address = address;
        using var resp = await http.PutAsync(
            $"{BASEPATH}/{address}",
            JsonContent.Create(user, options: CommunityApiOptions.SerializerOptions), ct).ConfigureAwait(false);

        return await Helpers.ReadOrThrowAsync<UserDto>(resp).ConfigureAwait(false);
    }

    // PATCH /api/users/{address}  (partial update: only non-null props are sent)
    public async Task<UserDto?> PatchAsync(string address, UserDto patch, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("address is required", nameof(address));
        if (patch is null) throw new ArgumentNullException(nameof(patch));

        // We send camelCase JSON omitting nulls (so only changes go over the wire)
        var json = JsonSerializer.Serialize(patch, CommunityApiOptions.SerializerOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var req = new HttpRequestMessage(HttpMethod.Patch, $"{BASEPATH}/{address}")
        {
            Content = content
        };

        using var resp = await http.SendAsync(req, ct).ConfigureAwait(false);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        return await Helpers.ReadOrThrowAsync<UserDto>(resp).ConfigureAwait(false);
    }

    // DELETE /api/users/{address}
    public async Task<bool> DeleteAsync(string address, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("address is required", nameof(address));
        using var resp = await http.DeleteAsync($"{BASEPATH}/{address}", ct).ConfigureAwait(false);
        return resp.StatusCode switch
        {
            HttpStatusCode.NoContent => true,
            HttpStatusCode.NotFound => false,
            _ => throw await Helpers.CreateApiExceptionAsync(resp).ConfigureAwait(false)
        };
    }
}
