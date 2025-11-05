using Substrate.NetApi.Model.Types;
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

    // POST /api/users/
    public async Task<UserDto> CreateAsync(Account account, UserDto user, CancellationToken ct = default)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));

        var wrappedUser = new RestWrapper<UserDto>(account, user);
        var address = account.Value;

        using var resp = await http.PostAsync($"{BASEPATH}/{address}", JsonContent.Create(wrappedUser, options: CommunityApiOptions.SerializerOptions), ct).ConfigureAwait(false);
        return await Helpers.ReadOrThrowAsync<UserDto>(resp, expected: HttpStatusCode.Created).ConfigureAwait(false);
    }

    // PUT /api/users/{address}
    public async Task<UserDto> PutAsync(Account account, UserDto user, CancellationToken ct = default)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));

        var wrappedUser = new RestWrapper<UserDto>(account, user);
        var address = account.Value;

        using var resp = await http.PutAsync(
            $"{BASEPATH}/{address}",
            JsonContent.Create(wrappedUser, options: CommunityApiOptions.SerializerOptions), ct).ConfigureAwait(false);

        return await Helpers.ReadOrThrowAsync<UserDto>(resp).ConfigureAwait(false);
    }

    // PATCH /api/users/{address}
    public async Task<UserDto?> PatchAsync(Account account, UserDto patch, CancellationToken ct = default)
    {
        if (patch is null) throw new ArgumentNullException(nameof(patch));

        var wrappedUser = new RestWrapper<UserDto>(account, patch);
        var address = account.Value;

        var json = JsonSerializer.Serialize(wrappedUser, CommunityApiOptions.SerializerOptions);
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
    public async Task<bool> DeleteAsync(Account account, string deleteUserAddress, CancellationToken ct = default)
    {

        if (string.IsNullOrWhiteSpace(deleteUserAddress)) throw new ArgumentException("address is required", nameof(deleteUserAddress));

        var delete = new DeleteUserDto
        {
            Address = deleteUserAddress
        };

        var wrappedDelete = new RestWrapper<DeleteUserDto>(account, delete);
        var address = account.Value;

        var json = JsonSerializer.Serialize(wrappedDelete, CommunityApiOptions.SerializerOptions);

        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var req = new HttpRequestMessage(HttpMethod.Delete, $"{BASEPATH}/{address}")
        {
            Content = content,
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
