using System;
using System.Data;
using Dapper;
using Npgsql;

namespace server.Common.Utils;

public class StringArrayHandler : SqlMapper.TypeHandler<string[]>
{
    public override string[] Parse(object value)
    {
        return (string[])value;
    }

    public override void SetValue(IDbDataParameter parameter, string[] value)
    {
        parameter.Value = value;
        if (parameter is NpgsqlParameter npgsqlParameter)
        {
            npgsqlParameter.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text;
        }
    }
}
