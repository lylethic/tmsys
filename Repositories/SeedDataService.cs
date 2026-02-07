using Dapper;
using System.Data;

namespace server.Repositories
{
    public class SeedDataService
    {
        private readonly IDbConnection _connection;
        public SeedDataService(IDbConnection connection)
        {
            this._connection = connection;
        }

        public async Task SeedPermission()
        {
            var sql = """
                INSERT INTO permissions (id, name, description) VALUES
                (generate_uuid_v7(), 'READ', 'Read permission'),
                (generate_uuid_v7(), 'WRITE', 'Write permission'),
                (generate_uuid_v7(), 'DELETE', 'Delete permission'),
                (generate_uuid_v7(), 'EDIT', 'Update permission');
            """;
            await _connection.ExecuteAsync(sql);
        }

        public async Task SeedRole()
        {
            var sql = """
                INSERT INTO roles (id, name, description) VALUES
                (generate_uuid_v7(), 'Admin', 'Administrator role with full permissions'),
                (generate_uuid_v7(), 'User', 'Standard user role with limited permissions'),
                (generate_uuid_v7(), 'Employee', 'staff');
            """;
            await _connection.ExecuteAsync(sql);
        }
    }
}
