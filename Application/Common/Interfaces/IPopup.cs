using System;
using server.Application.Request;
using server.Application.Request.Search;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface IPopup : IRepository<Popup>
{
    Task<CursorPaginatedResult<Popup>> GetPopupPageAsync(PopupSearch request);
}
