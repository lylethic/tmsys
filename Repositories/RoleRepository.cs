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
using server.Application.Search;

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

    public async Task<CursorPaginatedResult<Role>> GetAllAsync(BaseSearch request)
    {
        var where = new List<string>();
        var param = new DynamicParameters();
        where.Add("deleted = FALSE");
        if (!string.IsNullOrEmpty(request.Keyword))
        {
            where.Add("(name ILIKE @Keyword OR description ILIKE @Keyword)");
            param.Add("Keyword", $"%{request.Keyword}%");
        }
        return await this.GetListCursorBasedAsync<Role>(
           request: request,
           extraWhere: string.Join(" AND ", where),
           extraParams: param,
           orderDirection: request.Ascending ? "ASC" : "DESC",
           idColumn: "id"
         );
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