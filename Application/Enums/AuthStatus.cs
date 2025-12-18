using System;

namespace server.Application.Enums;

public enum AuthStatus
{
    UnprocessableContent = 422,
    Success = 200,
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    InternalServerError = 500
}
