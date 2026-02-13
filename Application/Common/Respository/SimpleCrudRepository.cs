using System;
using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Reflection;
using System.Text;
using server.Common.Exceptions;
using server.Application.Request;
using server.Common.Interfaces;
using Npgsql;

namespace server.Application.Common.Respository;

public class SimpleCrudRepository<T, ID>(IDbConnection connection) where T : class
{
    protected IDbConnection _connection = connection;
    protected string _dbTableName = typeof(T).GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.TableAttribute>()?.Name
        ?? typeof(T).GetCustomAttribute<TableAttribute>()?.Name
        ?? typeof(T).Name;

    public async Task<IDbTransaction> BeginTransactionAsync()
    {
        if (_connection is NpgsqlConnection npgsqlConn)
        {
            // PostgreSQL async transaction
            return await npgsqlConn.BeginTransactionAsync();
        }

        // Fallback: synchronous transaction for other providers
        return _connection.BeginTransaction();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _connection.GetAllAsync<T>();
    }

    public virtual async Task<T> GetByIDAsync<TKey>(TKey id)
    {
        return await _connection.GetAsync<T>(id);
    }

    /// <summary>
    /// Get one record by id (deleted, active)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="BadRequestException"></exception>
    public virtual async Task<T> GetByIdAsync(ID id)
    {
        var sql = $"""
            SELECT * 
            FROM {_dbTableName}
            WHERE Id = @Id
            AND active = true
            AND deleted = false
            LIMIT 1;
        """;

        return await _connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
    }

    /// <summary>
    /// Get list with pagination
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<PaginatedResult<T>> GetListWithPagination(PaginationRequest request)
    {
        var sql = $"""
            SELECT * FROM {_dbTableName}
            WHERE deleted = false
            ORDER BY created DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM {_dbTableName} WHERE deleted = false;
        """;
        var parameters = new
        {
            Offset = (request.PageIndex - 1) * request.PageSize,
            PageSize = request.PageSize
        };

        using var multi = await _connection.QueryMultipleAsync(sql, parameters);
        var data = multi.Read<T>().ToList();
        var totalCount = multi.ReadSingle<int>();

        return new PaginatedResult<T>
        {
            Data = data.Count > 0 ? data : [],
            TotalCount = totalCount,
            Page = request.PageIndex,
            PageSize = request.PageSize
        };
    }

    /// <summary>
    /// Get list with pagination And FILTERS
    /// </summary>
    public async Task<PaginatedResult<TResult>> GetListWithPaginationAndFilters<TFilter, TResult>(
        TFilter? filter,
        string sqlQuery,
        Func<TFilter?, object> parameterMapper)
        where TResult : class, IHasTotalCount
    {
        try
        {
            var parameters = parameterMapper(filter);
            var result = (await _connection.QueryAsync<TResult>(sqlQuery, parameters)).ToList();

            // Extract Total_count from the first result, casting to IHasTotalCount
            long totalCount = 0;
            if (result.Any())
            {
                IHasTotalCount firstResult = result.First();
                totalCount = firstResult.Total_count ?? 0;
            }

            // Extract PageIndex and PageSize from filter if available, else use defaults
            int pageIndex = 1;
            int pageSize = 10;
            if (filter != null)
            {
                pageIndex = filter.GetType().GetProperty("PageIndex")?.GetValue(filter) as int? ?? 1;
                pageSize = filter.GetType().GetProperty("PageSize")?.GetValue(filter) as int? ?? 10;
            }

            return new PaginatedResult<TResult>
            {
                Data = result,
                TotalCount = (int)totalCount,
                Page = pageIndex,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            throw new InternalErrorException($"Error executing paginated query: {ex.Message}");
        }
    }

    /// <summary>
    /// Get list with pagination and conditions (including deleted column)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="whereClause"></param>
    /// <param name="parameters"></param>
    /// <param name="orderBy"></param>
    /// <returns></returns>
    public async Task<PaginatedResult<T>> GetListWithPagination(
        PaginationRequest request,
        string? whereClause = null,
        object? parameters = null,
        string orderBy = "created DESC")
    {
        var sql = new StringBuilder($"""SELECT * FROM {_dbTableName}""");

        if (!string.IsNullOrWhiteSpace(whereClause))
            sql.Append($" WHERE {whereClause}");

        sql.Append($"""
                ORDER BY {orderBy}
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*) FROM {_dbTableName}
            """
        );

        if (!string.IsNullOrWhiteSpace(whereClause))
            sql.Append($" WHERE {whereClause}");

        var dynamicParams = new DynamicParameters(parameters);
        dynamicParams.Add("Offset", (request.PageIndex - 1) * request.PageSize);
        dynamicParams.Add("PageSize", request.PageSize);

        using var multi = await _connection.QueryMultipleAsync(sql.ToString(), dynamicParams);
        var data = multi.Read<T>().ToList();
        var totalCount = multi.ReadSingle<int>();

        return new PaginatedResult<T>
        {
            Data = data.Count > 0 ? data : [],
            TotalCount = totalCount,
            Page = request.PageIndex,
            PageSize = request.PageSize
        };
    }

    public async Task<T?> GetOneByConditionAsync(string sql, object? param = null)
    {
        return await _connection.QuerySingleOrDefaultAsync<T>(sql, param);
    }

    public async Task<T> UpdateAsync(T entity)
    {
        await _connection.UpdateAsync(entity);
        return entity;
    }

    public async Task SoftDeleteAsync(ID id)
    {
        var sql = $"""
            UPDATE {_dbTableName}
            SET deleted = true,
                active = false
            WHERE id = @id
        """;

        await _connection.ExecuteAsync(sql, new { id });
    }

    public virtual async Task<IEnumerable<T>> ExecuteFunctionAsync<U>(string functionName, U parameters)
    {
        var command = new StringBuilder($"SELECT * FROM {functionName}(");

        if (parameters != null)
        {
            var paramList = new List<string>();

            foreach (var prop in parameters.GetType().GetProperties())
            {
                var value = prop.GetValue(parameters);
                var name = prop.Name;
                var dbType = SimpleCrudRepository<T, ID>.GetPostgresType(prop.PropertyType);

                // Use CAST only if type is known
                if (!string.IsNullOrEmpty(dbType))
                    paramList.Add($"CAST(@{name} AS {dbType})");
                else
                    paramList.Add($"@{name}");
            }

            command.Append(string.Join(", ", paramList));
        }

        command.Append(");");

        try
        {
            var sql = command.ToString();
            Console.WriteLine($"[SQL] {sql}"); // Debug log

            var result = await _connection.QueryAsync<T>(sql, parameters);
            return result;
        }
        catch (Exception ex)
        {
            throw new BadRequestException(ex.Message);
        }
    }

    private static string? GetPostgresType(Type type)
    {
        if (type == typeof(DateTime) || type == typeof(DateTime?))
            return "DATE";
        if (type == typeof(TimeSpan) || type == typeof(TimeSpan?))
            return "TIME";
        if (type == typeof(string))
            return "VARCHAR";
        if (type == typeof(bool) || type == typeof(bool?))
            return "BOOLEAN";
        if (type == typeof(int) || type == typeof(int?))
            return "INTEGER";
        if (type == typeof(Guid) || type == typeof(Guid?))
            return "UUID";

        return null; // fallback, no cast
    }

    /// <summary>
    /// Get list with condition and pagination using cursor. Don't get deleted column
    /// </summary>
    /// <param name="request"></param>
    /// <param name="extraWhere"></param>
    /// <param name="extraParams"></param>
    /// <param name="orderDirection"></param>
    /// <param name="idColumn"></param>
    /// <returns></returns>
    public async Task<CursorPaginatedResult<T>> GetListCursorBasedAsync<T>(
        CursorPaginationRequest request,
        string? extraWhere = null,
        object? extraParams = null,
        string orderDirection = "ASC",
        string idColumn = "id"
    )
    {
        // Normalize direction
        orderDirection = (orderDirection ?? "DESC").ToUpper();
        if (orderDirection != "ASC" && orderDirection != "DESC") orderDirection = "DESC";

        // Build WHERE clauses
        var whereClauses = new List<string>();
        if (!string.IsNullOrWhiteSpace(extraWhere))
            whereClauses.Add($"({extraWhere})");

        if (request.Cursor.HasValue)
        {
            if (orderDirection == "ASC")
                whereClauses.Add($"{idColumn} > @Cursor");
            else
                whereClauses.Add($"{idColumn} < @Cursor");
        }

        var whereSql = whereClauses.Any()
            ? " WHERE " + string.Join(" AND ", whereClauses)
            : "";

        // Build main SQL
        var sql = new StringBuilder();
        sql.Append($"SELECT * FROM {_dbTableName}");
        sql.Append(whereSql);
        sql.Append($" ORDER BY {idColumn} {orderDirection}");
        sql.Append(" LIMIT @Limit;");

        // Prepare parameters
        var param = new DynamicParameters(extraParams);
        param.Add("Cursor", request.Cursor);
        param.Add("Limit", request.PageSize + 1);

        // Debug: print SQL with parameter values substituted
        var debugSql = sql.ToString();
        foreach (var name in param.ParameterNames)
        {
            var value = ((SqlMapper.IParameterLookup)param)[name];
            var display = value switch
            {
                null => "NULL",
                string s => $"'{s}'",
                Guid g => $"'{g}'",
                _ => value.ToString()!
            };
            debugSql = debugSql.Replace($"@{name}", display);
        }
        Console.WriteLine("[SQL] " + debugSql);

        var list = (await _connection.QueryAsync<T>(sql.ToString(), param)).ToList();

        var result = new CursorPaginatedResult<T>();

        if (list.Count > request.PageSize)
        {
            result.HasNextPage = true;
            var pageData = list.Take(request.PageSize).ToList();
            result.Data = pageData;

            var lastItem = pageData[^1];
            var idProp = typeof(T).GetProperty("Id") ?? typeof(T).GetProperty("id");
            if (idProp is not null)
                result.NextCursor = (Guid?)idProp.GetValue(lastItem);
        }
        else
        {
            result.HasNextPage = false;
            result.Data = list;

            if (list.Count > 0)
            {
                var lastItem = list[^1];
                var idProp = typeof(T).GetProperty("Id") ?? typeof(T).GetProperty("id");
                if (idProp is not null)
                    result.NextCursor = (Guid?)idProp.GetValue(lastItem);
            }
        }

        return result;
    }

    public async Task<T> CreateAsync(T entity)
    {
        await _connection.InsertAsync(entity);
        return entity;
    }

    public async Task AddManyAsync(IEnumerable<T> items)
    {
        await _connection.InsertAsync(items);
    }

    public async Task DeleteAsync(T entity)
    {
        await _connection.DeleteAsync(entity);
    }

    public async Task DeleteAsync(IEnumerable<T> entity)
    {
        await _connection.DeleteAsync(entity);
    }

    /// <summary>
    /// Delete an item by its Id
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> DeleteByIdAsync<TKey>(TKey id)
    {
        var entity = (T)Activator.CreateInstance(typeof(T));
        typeof(T).GetProperty("id")!.SetValue(entity, id);
        return await _connection.DeleteAsync(entity);
    }
}
