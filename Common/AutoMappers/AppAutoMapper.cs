using System;
using AutoMapper;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using server.Application.DTOs;
using server.Domain.Entities;

namespace server.Common.AutoMappers;

public class AppAutoMapper : Profile
{
    public AppAutoMapper()
    {
        // TAsks 
        CreateMap<TaskDto, Tasks>();
        CreateMap<Tasks, TaskDto>();
        CreateMap<TaskCreate, Tasks>();
        CreateMap<Tasks, TaskCreate>();
        CreateMap<TaskUpdate, Tasks>();
        CreateMap<Tasks, TaskUpdate>();

        CreateMap<TaskCreate, Tasks>();
        CreateMap<TaskUpdate, Tasks>();

        //Project
        CreateMap<ProjectDto, Project>();
        CreateMap<Project, ProjectDto>();
        CreateMap<ProjectCreate, Project>();
        CreateMap<Project, ProjectCreate>();
        CreateMap<ProjectUpdate, Project>();
        CreateMap<Project, ProjectUpdate>();

        CreateMap<ProjectCreate, Project>();
        CreateMap<ProjectUpdate, Project>();

        // Report
        CreateMap<ReportDto, Report>();
        CreateMap<ReportCreate, ReportDto>();
        CreateMap<Report, ReportCreate>();
        CreateMap<ReportUpdate, Report>();
        CreateMap<Report, ReportUpdate>();

        CreateMap<ReportCreate, Report>();
        CreateMap<ReportUpdate, Report>();


        // ProgressUpdate
        CreateMap<ProgressUpdateDto, ProgressUpdate>();
        CreateMap<ProgressUpdate, ProgressUpdateDto>();
        CreateMap<ProgressUpdateCreate, ProgressUpdate>();
        CreateMap<ProgressUpdate, ProgressUpdateCreate>();
        CreateMap<ProgressUpdateUpdate, ProgressUpdate>();
        CreateMap<ProgressUpdate, ProgressUpdateDto>();

        CreateMap<ProgressUpdateCreate, ProgressUpdate>();
        CreateMap<ProgressUpdateUpdate, ProgressUpdate>();

        CreateMap<Client_request_log, Client_request_logCreate>().ReverseMap();

        // ProjectType
        CreateMap<ProjectType, ProjectTypeModel>().ReverseMap();
        CreateMap<CreateProjectType, ProjectType>();
        CreateMap<UpdateProjectType, ProjectType>();

        // ApprovedStatus
        CreateMap<UpSertApprovedStatus, Approved_status>();

        // Department
        CreateMap<UpSertDepartment, Department>();
        CreateMap<UpSertUserDepartment, UserDepartment>();
    }
}
