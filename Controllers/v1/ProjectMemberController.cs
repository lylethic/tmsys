using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Application.Request.Search;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;

namespace server.Controllers.v1
{
    [Route("v{version:apiVersion}/projectMember")]
    [ApiController]
    public class ProjectMemberController : BaseApiController
    {
        private readonly IProjectMember _repo;
        public ProjectMemberController(IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogManager logger,
            IProjectMember repo) : base(mapper, httpContextAccessor, logger)
        {
            this._repo = repo;
            this._mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ProjectMemberSearch request)
        {
            try
            {
                var result = await _repo.GetProjectMemberPageAsync(request);
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
                var result = await _repo.GetProjectMemberAsync(id);
                return Success(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return Error(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(ProjectMemberCreate request)
        {
            try
            {
                var mapp = _mapper.Map<ProjectMember>(request);
                var result = await _repo.AddAsync(mapp);
                return Success(result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _repo.DeleteItemAsync(id);
                return Success(true);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put(Guid id, ProjectMemberUpdate item)
        {
            try
            {
                var mapp = _mapper.Map<ProjectMember>(item);
                await _repo.UpdateItemAsync(id, mapp);
                return Success(true);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}
