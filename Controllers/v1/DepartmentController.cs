
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Application.Request;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;
using server.Services;

namespace server.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/departments")]
    public class DepartmentController : BaseApiController
    {
        private readonly IDepartment _repo;
        private readonly NotificationService _notificationService;
        public DepartmentController(IDepartment departmentRepo,
            NotificationService notificationService,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogManager logger) : base(mapper, httpContextAccessor, logger)
        {
            this._notificationService = notificationService;
            this._repo = departmentRepo;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] List<UpSertDepartment> dto)
        {
            try
            {
                var mapped = _mapper.Map<List<Department>>(dto);
                var result = await _repo.AddAsync(mapped);
                return Success(result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByID(Guid id)
        {
            try
            {
                var result = await _repo.GetByIdAsync(id);
                return Success(result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(DepartmentSearch search)
        {
            try
            {
                var result = await _repo.GetDepartmentPageAsync(search);
                return Success(result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(Guid id)
        {
            try
            {
                var result = await _repo.DeleteItemAsync(id);
                return Success(result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(Guid id, UpSertDepartment dto)
        {
            try
            {
                var mapped = _mapper.Map<Department>(dto);
                var result = await _repo.UpdateItemAsync(id, mapped);
                return Success(result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}
