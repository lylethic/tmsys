using System;
using System.Data;
using Dapper;
using Medo;
using server.Application.Common.Interfaces;
using server.Application.Common.Respository;
using server.Application.Models;
using server.Application.Request;
using server.Application.Request.Search;
using server.Common.Exceptions;
using server.Domain.Entities;
using server.Services;

namespace server.Repositories
{
    public class UserDepartmentRepository : SimpleCrudRepository<UserDepartment, Guid>, IUserDepartment
    {
        private readonly IAssistantService _assistantService;
        private readonly IDepartment _departmentRepo;
        private readonly IUserRepository _userRepo;
        public UserDepartmentRepository(
            IDbConnection connection,
            IAssistantService assistantService,
            IUserRepository userRepository,
            IDepartment deparmentRepo
            ) : base(connection)
        {
            _connection = connection;
            _assistantService = assistantService;
            _departmentRepo = deparmentRepo;
            _userRepo = userRepository;
        }

        public async Task<UserDepartment> AddAsync(UserDepartment entity)
        {
            _ = await _departmentRepo.GetByIdAsync(entity.Department_id)
         ?? throw new NotFoundException("Department not found!");
            _ = await _userRepo.GetByIdAsync(entity.User_id)
                ?? throw new NotFoundException("User not found!");

            entity.Id = Uuid7.NewUuid7().ToGuid();
            entity.Created_by = Guid.Parse(_assistantService.UserId);

            var sql = """
                INSERT INTO public.user_departments(
                    id, user_id, department_id, 
                    is_primary, start_date, end_date, 
                    active, deleted, created, updated, created_by, updated_by
                ) 
                VALUES(
                    @Id, @User_id, @Department_id, 
                    @Is_primary, @Start_date, @End_date, 
                    @Active, @Deleted, @Created, @Updated, @Created_by, @Updated_by)
            """;

            var rows = await _connection.ExecuteAsync(sql, entity);

            if (rows == 0)
                throw new BadRequestException("Insert failed");

            return await GetByIdAsync(entity.Id)
                ?? throw new BadRequestException("Failed to load created record");
        }

        public async Task<bool> DeleteItemAsync(Guid id)
        {
            await base.SoftDeleteAsync(id);
            return true;
        }

        public async Task<CursorPaginatedResult<UserDepartmentModel>> GetDepartmentPageAsync(UserDepartmentSearch request)
        {
            var where = new List<string>();
            var param = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(request.Start_date))
            {
                if (!DateOnly.TryParse(request.Start_date, out var startDate))
                {
                    throw new ArgumentException("Invalid startDate format. Expected yyyy-MM-dd");
                }

                where.Add("ud.start_date >= @StartDate");
                param.Add("StartDate", startDate);
            }

            if (!string.IsNullOrWhiteSpace(request.End_date))
            {
                if (!DateOnly.TryParse(request.End_date, out var endDate))
                {
                    throw new ArgumentException("Invalid endDate format. Expected yyyy-MM-dd");
                }

                where.Add("ud.end_date <= @EndDate");
                param.Add("EndDate", endDate);
            }

            if (request.UserId != Guid.Empty)
            {
                where.Add("ud.user_id = @UserId");
                param.Add("UserId", request.UserId);
            }

            if (request.DepartmentId != Guid.Empty)
            {
                where.Add("ud.department_id = @DepartmentId");
                param.Add("DepartmentId", request.DepartmentId);
            }

            if (request.Cursor.HasValue)
            {
                where.Add("ud.id > @Cursor");
                param.Add("Cursor", request.Cursor.Value);
            }
            param.Add("Limit", request.PageSize + 1);

            var sql = $"""
                SELECT
                    ud.*,
                    u.*,
                    d.*
                FROM user_departments ud
                JOIN users u ON u.id = ud.user_id
                JOIN departments d ON d.id = ud.department_id
                {(where.Count != 0 ? "WHERE " + string.Join(" AND ", where) : "")}
                ORDER BY ud.id ASC
                LIMIT @Limit
            """;

            var rows = await _connection.QueryAsync<
                UserDepartment,
                User,
                Department,
                UserDepartmentModel>(
                sql,
                (ud, user, dept) => new UserDepartmentModel
                {
                    UserDepartment = ud,
                    User = user,
                    Department = dept
                },
                param,
                splitOn: "Id,Id"
            );

            var list = rows.ToList();

            // next page
            var hasNextPage = list.Count > request.PageSize;

            if (hasNextPage)
            {
                list.RemoveAt(list.Count - 1);
            }

            return new CursorPaginatedResult<UserDepartmentModel>
            {
                Data = list,
                HasNextPage = hasNextPage,
                NextCursor = hasNextPage
                    ? list.Last().UserDepartment.Id
                    : null,
                NextCursorSortOrder = null,
                Total = null
            };
        }

        public async Task<bool> UpdateItemAsync(Guid id, UserDepartment entity)
        {
            _ = await base.GetByIdAsync(id)
                ?? throw new NotFoundException("User's department not found!");
            var sql = """
                UPDATE public.user_departments
                SET 
                    user_id = @User_id,
                    department_id = @Department_id,
                    is_primary = @Is_primary,
                    start_date = @Start_date,
                    end_date = @End_date,
                    active = @Active,
                    updated = @Updated,
                    updated_by = @Updated_by
            """;
            var result = await _connection.ExecuteAsync(sql, entity);
            if (result > 0) return true;
            throw new BadRequestException("Failed to update user's department");
        }
    }
}