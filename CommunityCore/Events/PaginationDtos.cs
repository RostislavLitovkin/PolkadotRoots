using System.Text.Json.Serialization;

namespace CommunityCore.Events
{
    public sealed class PageDto<T>
    {
        [JsonPropertyName("totalElements")] public long TotalElements { get; set; }
        [JsonPropertyName("totalPages")] public int TotalPages { get; set; }
        [JsonPropertyName("first")] public bool First { get; set; }
        [JsonPropertyName("last")] public bool Last { get; set; }
        [JsonPropertyName("size")] public int Size { get; set; }
        [JsonPropertyName("content")] public List<T> Content { get; set; } = new();
        [JsonPropertyName("number")] public int Number { get; set; }
        [JsonPropertyName("sort")] public SortDto? Sort { get; set; }
        [JsonPropertyName("numberOfElements")] public int NumberOfElements { get; set; }
        [JsonPropertyName("pageable")] public PageableObjectDto? Pageable { get; set; }
        [JsonPropertyName("empty")] public bool Empty { get; set; }
    }

    public sealed class PageableObjectDto
    {
        [JsonPropertyName("offset")] public long Offset { get; set; }
        [JsonPropertyName("sort")] public SortDto? Sort { get; set; }
        [JsonPropertyName("unpaged")] public bool Unpaged { get; set; }
        [JsonPropertyName("pageNumber")] public int PageNumber { get; set; }
        [JsonPropertyName("pageSize")] public int PageSize { get; set; }
        [JsonPropertyName("paged")] public bool Paged { get; set; }
    }

    public sealed class SortDto
    {
        // Spring Data "Sort" summary object
        [JsonPropertyName("empty")] public bool? Empty { get; set; }
        [JsonPropertyName("sorted")] public bool? Sorted { get; set; }
        [JsonPropertyName("unsorted")] public bool? Unsorted { get; set; }
    }
}
