using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace server.Application.Request;

public class CursorPaginationRequest
{
    // for stable unique secondary key (id)
    [FromQuery(Name = "cursor")]
    public Guid? Cursor { get; set; }      // id of last item from previous page

    [FromQuery(Name = "cursor_sort_order")]
    public short? CursorSortOrder { get; set; } // sort_order of last item from previous page

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; set; } = 20;

    // note: this is used only as a fallback if sort direction not provided
    [FromQuery(Name = "ascending")]
    public bool Ascending { get; set; } = false;
}

public class CursorPaginatedResult<T>
{
    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = new();

    [JsonPropertyName("nextCursor")]
    public Guid? NextCursor { get; set; }

    [JsonPropertyName("nextCursorSortOrder")]
    public short? NextCursorSortOrder { get; set; }

    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage { get; set; }

    [JsonPropertyName("total")]
    public long? Total { get; set; } = null;
}
