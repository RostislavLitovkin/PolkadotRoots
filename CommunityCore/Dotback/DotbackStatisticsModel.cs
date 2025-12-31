using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using CommunityCore;

namespace CommunityCore.Dotback
{
    public sealed class CommunityDotbackStatisticsApiClient
    {
        public const string BASEPATH = "/api/dotbacks/statistics";

        private readonly HttpClient http;
        private readonly CommunityApiOptions options;

        public CommunityDotbackStatisticsApiClient(HttpClient httpClient, CommunityApiOptions? options = null)
        {
            http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.options = options ?? new CommunityApiOptions();

            if (http.BaseAddress is null)
                http.BaseAddress = this.options.BaseAddress;
        }

        public async Task<byte[]?> ExportEventCsvAsync(long eventId, double? dotPriceUsd = null, CancellationToken ct = default)
        {
            var qp = new List<string>();
            if (dotPriceUsd.HasValue)
            {
                qp.Add($"dotPriceUsd={dotPriceUsd.Value.ToString(CultureInfo.InvariantCulture)}");
            }

            var suffix = qp.Count > 0 ? $"?{string.Join("&", qp)}" : string.Empty;

            using var req = new HttpRequestMessage(HttpMethod.Get, $"{BASEPATH}/{eventId}/csv{suffix}");
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/csv"));

            using var resp = await http.SendAsync(req, ct).ConfigureAwait(false);

            if (resp.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (!resp.IsSuccessStatusCode)
                throw await Helpers.CreateApiExceptionAsync(resp).ConfigureAwait(false);

            return await resp.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
        }
    }
}
