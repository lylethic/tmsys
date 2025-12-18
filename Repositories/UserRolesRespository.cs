using System;
using System.Data;
using Dapper;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.Request;
using server.Common.Exceptions;
using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Repositories;

public class UserRolesRespository(IDbConnection connection) : SimpleCrudRepository<UserRolesRespository, Guid>(connection), IUserRoles
{
    public async Task<User_roles> AddAsync(User_roles entity)
    {
        const string sql = """
            INSERT INTO user_roles (user_id, role_id) 
            VALUES (@userId, @RoleId)
        """;

        try
        {
            var result = await _connection.ExecuteAsync(sql, entity);
            if (result == 0) throw new BadRequestException("Failed to insert user role into the database.");

            return entity;
            throw new BadRequestException("user role created, but failed to retrieve it.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<bool> DeleteItemAsync(Guid id)
    {
        const string sql = """
            DELETE FROM user_roles WHERE user_id = @Id
        """;

        try
        {
            var result = await _connection.ExecuteAsync(sql, new { Id = id });
            if (result == 0) throw new BadRequestException("Failed to delete user role.");
            return true;
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<PaginatedResult<User_roles>> GetAllAsync(PaginationRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateItemAsync(Guid id, User_roles entity)
    {
        throw new NotImplementedException();
    }

    public async Task<User_roles> GetByIdAsync(Guid id)
    {
        var sql = "SELECT * FROM user_roles WHERE user_id = @Id";
        var result = await _connection.QuerySingleOrDefaultAsync<User_roles>(sql, new { Id = id })
            ?? throw new NotFoundException("user role not found.");
        return result;
    }
}
