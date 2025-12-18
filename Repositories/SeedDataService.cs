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
                (gen_random_uuid(), 'READ', 'Read permission'),
                (gen_random_uuid(), 'WRITE', 'Write permission'),
                (gen_random_uuid(), 'DELETE', 'Delete permission'),
                (gen_random_uuid(), 'EDIT', 'Update permission'),
                (gen_random_uuid(), 'AM_READ', 'permission'),
                (gen_random_uuid(), 'AM_CREATE', 'permission'),
                (gen_random_uuid(), 'AM_DELETE', 'permission'),
                (gen_random_uuid(), 'AM_EDIT', 'permission');
            """;
            await _connection.ExecuteAsync(sql);
        }

        public async Task SeedRole()
        {
            var sql = """
                INSERT INTO roles (id, name, description) VALUES
                (gen_random_uuid(), 'Admin', 'Administrator role with full permissions'),
                (gen_random_uuid(), 'User', 'Standard user role with limited permissions'),
                (gen_random_uuid(), 'Employee', 'staff');
            """;
            await _connection.ExecuteAsync(sql);
        }
    }
}
