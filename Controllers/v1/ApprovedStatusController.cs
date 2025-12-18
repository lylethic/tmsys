
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Application.Request;
using server.Application.Request.Search;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;

namespace server.Controllers.v1
{
    [Route("v{version:apiVersion}/approvedStatus")]
    [ApiController]
    public class ApprovedStatusController : BaseApiController
    {
        private readonly IApprovedStatus _approvedStatus;
        public ApprovedStatusController(
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogManager logger,
            IApprovedStatus approvedStatus)
        : base(mapper, httpContextAccessor, logger)
        {
            this._approvedStatus = approvedStatus;
            this._mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ApprovedSearch request)
        {
            try
            {
                var result = await _approvedStatus.GetTaskStatusPageAsync(request);
                return Success(result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByID(Guid id)
        {
            try
            {
                var result = await _approvedStatus.GetByIDAsync(id);
                return Success(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return Error(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateAsync(UpSertApprovedStatus request)
        {
            try
            {
                var mapp = _mapper.Map<Approved_status>(request);
                var result = await _approvedStatus.AddAsync(mapp);
                return Success(result);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpDelete("{approvedStatusId}")]
        public async Task<IActionResult> Delete(Guid approvedStatusId)
        {
            try
            {
                await _approvedStatus.DeleteItemAsync(approvedStatusId);
                return Success(true);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPatch]
        public async Task<IActionResult> Put(UpSertApprovedStatus item)
        {
            try
            {
                var mapp = _mapper.Map<Approved_status>(item);
                await _approvedStatus.UpdateItemAsync(mapp);
                return Success(true);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}
