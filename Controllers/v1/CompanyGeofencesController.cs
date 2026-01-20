using System;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;

namespace server.Controllers.v1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/company-geofences")]
public class CompanyGeofencesController : BaseApiController
{
    private readonly ICompanyGeofence _companyGeofenceRepo;

    public CompanyGeofencesController(
        ICompanyGeofence companyGeofenceRepo,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogManager logger) : base(mapper, httpContextAccessor, logger)
    {
        _companyGeofenceRepo = companyGeofenceRepo;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        try
        {
            var result = await _companyGeofenceRepo.GetActiveAsync();
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _companyGeofenceRepo.GetByIdAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CompanyGeofenceUpsert dto)
    {
        try
        {
            var entity = _mapper.Map<CompanyGeofence>(dto);
            var result = await _companyGeofenceRepo.AddAsync(entity);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CompanyGeofenceUpsert dto)
    {
        try
        {
            var entity = _mapper.Map<CompanyGeofence>(dto);
            var result = await _companyGeofenceRepo.UpdateItemAsync(id, entity);
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
            var result = await _companyGeofenceRepo.DeleteItemAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }
}
