using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.DTOs;
using server.Application.Enums;
using server.Application.Models;
using server.Application.Request;
using server.Common.CoreConstans;
using server.Common.Exceptions;
using server.Common.Interfaces;
using server.Common.Utils;
using server.Domain.Entities;
using server.Services;
using System.Data;

namespace server.Repositories;

public class UserRepository : SimpleCrudRepository<User, Guid>, IUserRepository
{
    private readonly IRoleRepository _roleRepo;
    private readonly IMailService _gmailService;
    private readonly ILogManager _logManager;
    private IAssistantService _assistantService;
    private readonly IWebHostEnvironment _env;
    private readonly ICloudinaryService _cloudinaryService;

    public UserRepository(
        IDbConnection connection,
        IRoleRepository roleRepo,
        IMailService gmailService,
        ILogManager logManager,
        IAssistantService assistantService,
        IWebHostEnvironment env,
        ICloudinaryService cloudinaryService
    )
        : base(connection)
    {
        _roleRepo = roleRepo;
        _gmailService = gmailService;
        _logManager = logManager;
        _assistantService = assistantService;
        _env = env;
        this._cloudinaryService = cloudinaryService;
    }

    /// <summary>
    /// Add a new user/employee.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="BadRequestException"></exception>
    /// <exception cref="InternalErrorException"></exception>
    public async Task<User> AddAsync(User entity)
    {
        if (entity is null)
            throw new BadRequestException("Invalid data provided.");

        // Validate email
        var isValidEmail = ValidatorHepler.EmailValidation(entity.Email);
        if (!isValidEmail)
            throw new BadRequestException("Invalid email address.");


        entity.id = Uuid7.NewUuid7().ToGuid();
        entity.Password = BCrypt.Net.BCrypt.HashPassword(entity.Password);
        entity.Role_id = Guid.Parse("0fb06e55-320e-4085-8b73-91a8e0ee59b3");

        const string insertUserSql = """
            INSERT INTO users (id, name, email, password, profilepic, city) 
            VALUES (@Id, @Name, @Email, @Password, @ProfilePic, @City)
        """;

        const string insertuserRoleSql = """
            INSERT INTO user_roles (user_id, role_id)
            VALUES (@userId, @RoleId)
        """;

        try
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            await _connection.ExecuteAsync(insertUserSql, entity);

            // Insert into user_roles
            await _connection.ExecuteAsync(insertuserRoleSql, new
            {
                userId = entity.id,
                RoleId = entity.Role_id
            });

            // Get the inserted user
            var inserted = await GetByIdAsync(entity.id)
                ?? throw new BadRequestException("user created, but failed to retrieve it.");

            // --- Load Email Template ---
            var templatePath = Path.Combine(_env.ContentRootPath, "wwwroot", "WelcomeEmail.html");

            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"Email template not found at {templatePath}");

            var body = await File.ReadAllTextAsync(templatePath);

            // Replace placeholders
            body = body.Replace("{{Email}}", entity.Email)
                       .Replace("{{Name}}", entity.Name ?? "User")
                       .Replace("{{AppName}}", "Loopy");

            var subject = "Welcome to Loopy!";

            // Send email
            var emailRequest = new SendEmailRequest(entity.Email, subject, body);
            await _gmailService.SendEmailAsync(emailRequest);

            return inserted;
        }
        catch (Exception ex)
        {
            throw new InternalErrorException($"Failed to create user: {ex.Message}");
        }
    }

    public async Task<bool> DeleteItemAsync(Guid id)
    {
        try
        {
            var existinguser = await GetByIdAsync(id)
                ?? throw new NotFoundException("user not found");
            var updatedBy = Guid.Parse(_assistantService.UserId);
            const string sql = """
                UPDATE  users SET 
                deleted = true,
                active = false,
                updated = @updated,
                updated_by = @updated_by,
                WHERE id = @Id
            """;
            var result = await _connection.ExecuteAsync(sql, new { Id = id, updated = DateTime.UtcNow, updated_by = updatedBy });
            if (result > 0)
                return true;
            throw new BadRequestException("Failed to delete user.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<bool> DeleteItemsAsync(params Guid[] ids)
    {
        try
        {
            if (ids == null || ids.Length == 0)
                throw new BadRequestException("No IDs provided.");

            var updatedBy = Guid.Parse(_assistantService.UserId);

            const string sql = """
                UPDATE users
                SET 
                    deleted = true,
                    active = false,
                    updated = @updated,
                    updated_by = @updated_by
                WHERE id = ANY(@Ids)
            """;

            var result = await _connection.ExecuteAsync(sql, new
            {
                Ids = ids,
                updated = DateTime.UtcNow,
                updated_by = updatedBy
            });

            if (result > 0)
                return true;

            throw new BadRequestException("No users were deleted.");
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    public async Task<PaginatedResult<UserPermissionModel>> GetAllAsync(UserSearch? request)
    {
        const string sql = """
            SELECT * FROM get_list_of_users_with_roles_permissions(
                @email_filter,
                @name_filter,
                @is_active_filter,
                @role_name_filter,
                @page_index,
                @page_size,
                @order_by
            );
        """;

        return await GetListWithPaginationAndFilters<UserSearch, UserPermissionModel>(
            filter: request,
            sqlQuery: sql,
            parameterMapper: filter => new
            {
                email_filter = filter?.Email,
                name_filter = filter?.Name,
                is_active_filter = filter?.IsActive,
                role_name_filter = filter?.Role_name,
                page_index = filter != null ? filter.PageIndex : 1,
                page_size = filter != null ? filter.PageSize : 20,
                order_by = filter?.OrderBy
            });
    }

    /// <summary>
    /// Get user with permissions and roles.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="BadRequestException"></exception>
    public async Task<User_Permisson_Dto> GetUserWithPermissionAsync(Guid id)
    {
        const string sql = """
            SELECT 
                a.id AS user_id,
                a.email,
                a.active,
                a.created,
                a.updated,
                ar.role_id,
                p.name AS permission_name
            FROM users a
            JOIN user_roles ar ON a.id = ar.user_id
            LEFT JOIN role_permissions rp ON ar.role_id = rp.role_id
            LEFT JOIN permissions p ON rp.permission_id = p.id
            WHERE a.id = @Id AND a.active = true;
        """;

        var rows = await _connection.QueryAsync(sql, new { Id = id });

        if (!rows.Any())
            throw new NotFoundException("user not found");

        User_Permisson_Dto? user = null;

        foreach (var row in rows)
        {
            if (user is null)
            {
                Guid userId = row.user_id;
                Guid roleId = row.role_id;

                user = new User_Permisson_Dto
                {
                    Id = userId,
                    Email = row.email,
                    Active = row.active,
                    Role_id = roleId,
                    Permissions = []
                };
            }

            if (row.permission_name != null && !user.Permissions.Contains(row.permission_name))
            {
                user.Permissions.Add(row.permission_name);
            }
        }
        if (user != null)
            return user;
        return null!;
    }

    public async Task<User> GetByIdAsync(Guid id)
    {
        const string sql = """
            SELECT *
            FROM users 
            WHERE id = @Id
        """;
        var result = await _connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id })
            ?? throw new NotFoundException("user not found");
        return result;
    }

    public async Task<bool> UpdateItemAsync(Guid id, User entity)
    {
        if (entity is null)
            throw new BadRequestException("Invalid data provided.");
        var existingUser = await GetByIdAsync(id);
        if (existingUser == null)
            throw new NotFoundException("user not found");

        if (entity.Username is not null || !string.IsNullOrWhiteSpace(entity.Username))
            existingUser.Username = entity.Username;

        if (entity.Name is not null || !string.IsNullOrWhiteSpace(entity.Name))
            existingUser.Name = entity.Name;

        if (entity.City is not null || !string.IsNullOrWhiteSpace(entity.City))
            existingUser.City = entity.City;

        if (!string.IsNullOrWhiteSpace(entity.ProfilePic))
            existingUser.ProfilePic = entity.ProfilePic;
        if (entity.Is_send_email.HasValue)
            existingUser.Is_send_email = entity.Is_send_email;

        entity.Updated_by = Guid.Parse(_assistantService.UserId);
        const string sql = """
                UPDATE users
                SET 
                    username = @Username,
                    name = @Name,
                    profilepic = @ProfilePic,
                    city = @City,
                    is_send_email = @Is_send_email,
                    updated_by = @Updated_by,
                    updated = @Updated,
                    active = @active
                WHERE id = @Id
            """;
        try
        {
            await _connection.ExecuteAsync(sql, existingUser);
            return true;
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }

    /// <summary>
    /// Register a new user. Role User
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="BadRequestException"></exception>
    /// <exception cref="NotFoundException"></exception>
    /// <exception cref="InternalErrorException"></exception>
    public async Task<User> RegisterUser(User entity)
    {
        if (entity is null)
            throw new BadRequestException("Invalid data provided.");

        // Validate email
        var isValidEmail = ValidatorHepler.EmailValidation(entity.Email);
        if (!isValidEmail)
            throw new BadRequestException("Invalid email address.");

        // --- Check for duplicate email ---
        var existingUser = await _connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM users WHERE email = @Email",
            new { entity.Email });

        if (existingUser != null)
            throw new BadRequestException($"Email already exists: {entity.Email}");

        entity.id = Uuid7.NewUuid7().ToGuid();
        entity.Role_id = Guid.Parse("4e141ecb-fdc6-47c5-be03-1622af1a8e65"); // user
        entity.Password = BCrypt.Net.BCrypt.HashPassword(entity.Password);
        entity.Created_by = Guid.Parse(_assistantService.UserId);

        _ = await _roleRepo.GetByIdAsync(entity.Role_id)
            ?? throw new NotFoundException($"Role not found: {entity.Role_id}");

        const string insertUserSql = """
            INSERT INTO users (id, name, email, password, profilepic, city, created, created_by, active, deleted) 
            VALUES (@Id, @Name, @Email, @Password, @ProfilePic, @City, @Created, @Created_By, @Active, @Deleted)
        """;

        const string insertUserRoleSql = """
            INSERT INTO user_roles (user_id, role_id)
            VALUES (@UserId, @RoleId)
        """;

        if (_connection.State != ConnectionState.Open)
            _connection.Open();

        using var transaction = _connection.BeginTransaction();
        try
        {
            // Insert into users
            await _connection.ExecuteAsync(insertUserSql, entity, transaction);

            // Insert into user_roles
            await _connection.ExecuteAsync(insertUserRoleSql, new
            {
                UserId = entity.id,
                RoleId = entity.Role_id
            }, transaction);

            transaction.Commit();

            // Get the inserted user
            var inserted = await GetByIdAsync(entity.id)
                ?? throw new BadRequestException("User created, but failed to retrieve it.");

            // --- Load Email Template ---
            var templatePath = Path.Combine(_env.ContentRootPath, "wwwroot", "Resource/WelcomeEmail.html");

            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"Email template not found at {templatePath}");

            var body = await File.ReadAllTextAsync(templatePath);

            // Replace placeholders
            body = body.Replace("{{Email}}", entity.Email)
                       .Replace("{{Name}}", entity.Name ?? "User")
                       .Replace("{{AppName}}", "Loopy");

            var subject = "Welcome to Loopy!";

            // Send email
            var emailRequest = new SendEmailRequest(entity.Email, subject, body);
            await _gmailService.SendEmailAsync(emailRequest);

            return inserted;
        }
        catch (Exception ex)
        {
            throw new InternalErrorException($"Failed to create user: {ex.Message}");
        }
    }

    public Task<PaginatedResult<User>> GetAllAsync(PaginationRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateLoginTime(Guid id, string accessToken)
    {
        var lastLogin = DateTime.UtcNow;
        var updateToken = """
            UPDATE users
            SET token = @token, last_login_time = @lastLogin
            WHERE id = @id;
        """;

        await _connection.ExecuteAsync(updateToken, new { token = accessToken, lastLogin, id });
    }

    public async Task<User> GetEmailAsync(string email)
    {
        var isValidEmail = ValidatorHepler.EmailValidation(email);
        if (isValidEmail == false)
            throw new BadRequestException("Invalid email");
        var existingEmail = """
            SELECT id, name, email, password
            FROM users
            WHERE LOWER(email) = LOWER(@Email) AND active = true AND deleted = false;
        """;
        var user = await _connection.QuerySingleOrDefaultAsync<User>(existingEmail, new { Email = email });

        if (user == null)
            throw new NotFoundException("User not found");
        return user;
    }

    public async Task<UserRolesAndPermissions> GetUserRolesAndPermissionsAsync(Guid userId)
    {
        var sql = """
            SELECT DISTINCT 
                r.id as role_id, r.name as role_name,
                p.id as permission_id, p.name as permission_name
            FROM user_roles ar
                INNER JOIN roles r ON ar.role_id = r.id
                INNER JOIN role_permissions rp ON r.id = rp.role_id
                INNER JOIN permissions p ON rp.permission_id = p.id
            WHERE ar.user_id = @UserId
        """;

        var rolePermissionMap = new Dictionary<Guid, Role>();
        var allPermissions = new List<Permission>();

        var results = await _connection.QueryAsync(sql, new { UserId = userId });

        foreach (var row in results)
        {
            var roleId = (Guid)row.role_id;
            var roleName = (string)row.role_name;
            var permissionId = (Guid)row.permission_id;
            var permissionName = (string)row.permission_name;

            if (!rolePermissionMap.ContainsKey(roleId))
            {
                rolePermissionMap[roleId] = new Role { id = roleId, Name = roleName, Permissions = [] };
            }

            var permission = new Permission { id = permissionId, Name = permissionName };
            rolePermissionMap[roleId].Permissions.Add(permission);

            if (!allPermissions.Any(p => p.id == permissionId))
            {
                allPermissions.Add(permission);
            }
        }

        return new UserRolesAndPermissions
        {
            Roles = [.. rolePermissionMap.Values],
            Permissions = allPermissions
        };
    }

    public async Task<bool> SetPassword(Guid userId, string newPassword)
    {
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
        string sql = "UPDATE users SET password = @password WHERE id = @userId";
        int affected = await _connection.ExecuteAsync(sql, new { password = hashedPassword, userId = userId });

        if (affected > 0)
            return true;
        return false;
    }

    public async Task ClearToken(Guid id)
    {
        var clearTokenSql = "UPDATE users SET token = NULL WHERE id = @id";
        await _connection.ExecuteAsync(clearTokenSql, new { id });
    }

    public async Task<string> UpdateAvatar(Guid userId, IFormFile file)
    {
        var existingUser = await GetByIDAsync(userId);
        if (existingUser == null)
            throw new NotFoundException("User not found");
        var result = await _cloudinaryService.UploadImageAsync(
            file,
            ImageUploadQuality.Thumbnail,
            folderName: CoreConstants.FOLDERCLOUDINARY,
            entityType: CoreConstants.EntityTypeUpload.Project,
            entityId: null,
            description: "Avatar"
        );
        var getURL = result.Url;
        var updatedBy = Guid.Parse(_assistantService.UserId);
        const string sql = """
                UPDATE users
                SET 
                    profilepic = @getURL,
                    updated_by = @updatedBy,
                    updated = @Updated
                WHERE id = @userId
            """;
        try
        {
            await _connection.ExecuteAsync(sql, new { getURL, updatedBy, Updated = DateTime.UtcNow, userId });
            return getURL;
        }
        catch (Exception ex)
        {
            throw new InternalErrorException(ex.Message);
        }
    }
}