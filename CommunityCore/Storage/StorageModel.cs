using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CommunityCore.Storage
{
    public sealed class StorageApiClient
    {
        public const string BASEPATH = "/api/storage";

        private readonly HttpClient http;
        private readonly CommunityApiOptions options;

        public StorageApiClient(HttpClient httpClient, CommunityApiOptions? options = null)
        {
            this.http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.options = options ?? new CommunityApiOptions();

            if (this.http.BaseAddress is null)
                this.http.BaseAddress = this.options.BaseAddress;
        }

        public async Task<StorageUploadDto> UploadImageAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder,
            CancellationToken ct = default
        )
        {
            if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));
            if (!fileStream.CanRead) throw new ArgumentException("Stream must be readable", nameof(fileStream));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name is required", nameof(fileName));

            if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Content type must be image/*, got '{contentType}'", nameof(contentType));

            using var form = new MultipartFormDataContent();
            using var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            form.Add(fileContent, name: "file", fileName: fileName);

            form.Add(new StringContent(folder, Encoding.UTF8), name: "folder");

            using var resp = await http.PostAsync($"{BASEPATH}/upload-image", form, ct).ConfigureAwait(false);

            if (resp.StatusCode != HttpStatusCode.OK)
                throw await Helpers.CreateApiExceptionAsync(resp).ConfigureAwait(false);

            return await Helpers.ReadOrThrowAsync<StorageUploadDto>(resp).ConfigureAwait(false);
        }

        public async Task<string> GetImageAsync(
            string fileName,
            string folder,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name is required", nameof(fileName));
            if (string.IsNullOrWhiteSpace(folder))
                throw new ArgumentException("Folder is required", nameof(folder));

            var key = $"{folder}/{fileName}";

            using var resp = await http.GetAsync($"{BASEPATH}/image?key={Uri.EscapeDataString(key)}", ct).ConfigureAwait(false);

            if (resp.StatusCode != HttpStatusCode.OK)
                throw await Helpers.CreateApiExceptionAsync(resp).ConfigureAwait(false);

            var payload = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            try
            {
                using var doc = JsonDocument.Parse(payload);
                if (doc.RootElement.TryGetProperty("url", out var urlProp) && urlProp.GetString() is string url && !string.IsNullOrWhiteSpace(url))
                    return url;

                throw new InvalidOperationException("Response JSON does not contain a valid 'url' field.");
            }
            catch (JsonException jx)
            {
                throw new InvalidOperationException("Failed to parse response JSON for presigned URL.", jx);
            }
        }
    }
}
