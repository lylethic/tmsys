using System.Data;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Domain.Entities;
using Dapper;
using server.Common.Exceptions;
using server.Application.Request;
using AutoMapper;
using server.Common.Interfaces;
using Medo;

namespace server.Repositories;

public class RoleRepository(IDbConnection connection) : SimpleCrudRepository<Role, Guid>(connection), IRoleRepository
{
    public async Task<Role> AddAsync(Role entity)
    {
        if (entity is null)
            throw new BadRequestException("Please enter your role.");
        entity.Id = Uuid7.NewUuid7().ToGuid();
        var sql = """
            INSERT INTO roles (id, name, description) 
            VALUES (@Id, @Name, @Description)
        """;
        try
        {
            var queryResult = await _connection.ExecuteAsync(sql, entity);

            if (queryResult > 0)
            {
                var result = await GetByIdAsync(entity.Id);
                if (result is not null)
                    return result;
                throw new BadRequestException("Role created, but failed to retrieve it.");
            }
            throw new BadRequestException("Failed to insert role into the database.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<bool> DeleteItemAsync(Guid id)
    {
        try
        {
            var existingRole = await GetByIdAsync(id)
                ?? throw new NotFoundException("Role not found");

            var result = await base.DeleteByIdAsync(id);
            if (result)
                return true;
            throw new BadRequestException("Failed to delete role.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<PaginatedResult<Role>> GetAllAsync(PaginationRequest request)
    {
        var sql = """
            SELECT * FROM roles
            ORDER BY created_at DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM roles;
        """;
        var parameters = new
        {
            Offset = (request.PageIndex - 1) * request.PageSize,
            PageSize = request.PageSize
        };

        try
        {
            using var multi = await _connection.QueryMultipleAsync(sql, parameters);
            var result = multi.Read<Role>().ToList();
            var totalRecords = multi.ReadSingle<int>();

            return new PaginatedResult<Role>
            {
                Data = result.Count > 0 ? result : [],
                TotalCount = totalRecords,
                Page = request.PageIndex,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<Role> GetByIdAsync(Guid id)
    {
        var sql = "SELECT * FROM roles WHERE id = @Id";
        var result = await _connection.QuerySingleOrDefaultAsync<Role>(sql, new { Id = id })
            ?? throw new NotFoundException("Role not found");
        return result;
    }

    public async Task<bool> UpdateItemAsync(Guid id, Role entity)
    {
        try
        {
            var existingRole = await GetByIdAsync(id)
                ?? throw new NotFoundException("Role not found");
            entity.Id = id;
            var sql = """
                UPDATE roles
                SET name = @Name, description = @Description
                WHERE id = @Id
            """;
            await _connection.ExecuteAsync(sql, entity);
            return true;
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }
}