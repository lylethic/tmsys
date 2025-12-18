using System;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

namespace server.Application.Request;

public class PaginationRequest
{
    private int _page = 1;
    private int _pageSize = 10;
    [FromQuery(Name = "pageIndex")]
    [DefaultValue(1)]
    public int PageIndex
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }
    [FromQuery(Name = "pageSize")]
    [DefaultValue(20)]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is >= 1 and < 101 ? value : 10;
    }

    /// <summary>
    /// 1 DESC, 2 ASC
    /// </summary>
    [FromQuery(Name = "orderBy")]
    [DefaultValue(1)]
    public int? OrderBy
    {
        get; set;
    }
}

public class PaginatedResult<T>
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
    public List<T> Data { get; set; } = [];
}
