using System;
using System.Reflection;
using System.Text;
using Dapper.Contrib.Extensions;

namespace server.Common.Databases;

public class SqlCommandHelper
{
    public static string GetTableName<T>() where T : class
    {
        return typeof(T).GetCustomAttribute<TableAttribute>()?.Name ?? typeof(T).Name;
    }

    public static string GetSelectSql<T>() where T : class
    {
        return $"SELECT * from {GetTableName<T>()}";
    }

    public static string GetSelectSqlWithCondition<T>(object param) where T : class
    {
        var condition = new StringBuilder();
        var start = true;
        foreach (var property in param.GetType().GetProperties())
        {
            var andOperator = start == true ? "" : " AND ";
            condition.Append($"\"{andOperator}{property.Name}\" = @{property.Name}");
            start = false;
        }

        return $"{GetSelectSql<T>()} WHERE {condition}";
    }

    public static string GetStoredProcedureCall<T>(string procedureName, object? param = null, object? paging = null, object? sorting = null)
    {
        var parameters = new List<string>();

        // Add pagination parameters
        if (paging != null)
        {
            foreach (var property in paging.GetType().GetProperties())
            {
                parameters.Add($"@{property.Name}");
            }
        }

        // Add condition parameters
        if (param != null)
        {
            foreach (var property in param.GetType().GetProperties())
            {
                parameters.Add($"@{property.Name}");
            }
        }

        // Add sorting parameter if provided
        if (sorting != null)
        {
            foreach (var property in sorting.GetType().GetProperties())
            {
                parameters.Add($"@{property.Name}");
            }
        }

        // Combine parameters into the EXEC statement
        var parameterString = string.Join(", ", parameters);
        return $"EXEC [dbo].[{procedureName}] {parameterString}";
    }
}
