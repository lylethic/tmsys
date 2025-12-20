using System;
using System.Text.Json.Serialization;
using server.Application.Enums;

namespace server.Common.Models;

public class AuthResponse
{
    [JsonPropertyName("status")]
    public int Status { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; } = "Login success";
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;     // JWT access token
    [JsonPropertyName("tokenExpiredTime")]
    public DateTime TokenExpiredTime { get; set; } // Expiration datetime of the token

    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;
    [JsonPropertyName("refreshTokenExpiredTime")]
    public DateTime RefreshTokenExpiredTime { get; set; }

    [JsonPropertyName("data")]
    public object? Data { get; set; } = null!;
    [JsonPropertyName("errors")]
    public List<Error>? Errors { get; set; } = [];

    public AuthResponse() { }

    public AuthResponse(AuthStatus status, string message)
    {
        Status = (int)status;
        Message = message;
    }

    public AuthResponse(AuthStatus status, string token, DateTime tokenExpireTime, string refreshToken, DateTime refreshTokenExpireTime, object data)
    {
        this.Status = (int)status;
        this.Message = "Success";
        this.Token = token;
        this.TokenExpiredTime = tokenExpireTime;
        this.RefreshToken = refreshToken;
        this.RefreshTokenExpiredTime = refreshTokenExpireTime;
        this.Data = data;
    }

    public AuthResponse(AuthStatus status, string token, string message, List<Error>? errors = null)
    {
        this.Status = (int)status;
        this.Message = message;
        this.Errors = errors;
    }
}

public class Error
{
    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    public Error() { }

    public Error(string field, string message)
    {
        this.Field = field;
        this.Message = message;
    }
}