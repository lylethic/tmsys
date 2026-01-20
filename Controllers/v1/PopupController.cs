using System;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using server.Application.Common.Interfaces;
using server.Application.DTOs;
using server.Application.Request.Search;
using server.Common.Interfaces;
using server.Common.Settings;
using server.Domain.Entities;

namespace server.Controllers.v1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/popups")]
public class PopupController(
    IPopup popupRepo,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor,
    ILogManager logger) : BaseApiController(mapper, httpContextAccessor, logger)
{
    private readonly IPopup _popupRepo = popupRepo;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PopupSearch request)
    {
        try
        {
            var result = await _popupRepo.GetPopupPageAsync(request);
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
            var result = await _popupRepo.GetByIdAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PopupDto dto)
    {
        try
        {
            var request = _mapper.Map<Popup>(dto);
            var result = await _popupRepo.AddAsync(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PopupDto dto)
    {
        try
        {
            var request = _mapper.Map<Popup>(dto);
            var result = await _popupRepo.UpdateItemAsync(id, request);
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
            var result = await _popupRepo.DeleteItemAsync(id);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }
}
