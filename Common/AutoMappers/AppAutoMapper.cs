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

        //
        CreateMap<PopupDto, Popup>();

        CreateMap<WorkScheduleDto, Work_schedule>();

        // Company geofence
        CreateMap<CompanyGeofenceUpsert, CompanyGeofence>()
            .ForMember(dest => dest.Center_lat, opt => opt.MapFrom(src => src.CenterLat))
            .ForMember(dest => dest.Center_lng, opt => opt.MapFrom(src => src.CenterLng))
            .ForMember(dest => dest.Radius_m, opt => opt.MapFrom(src => src.RadiusM))
            .ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.Active));

        // Submission
        CreateMap<SubmissionDto, Submission>();
        CreateMap<SubmissionCreate, Submission>();
        CreateMap<SubmissionUpdate, Submission>();

        // ProjectMember
        CreateMap<ProjectMemberCreate, ProjectMember>()
            .ForMember(dest => dest.created, opt => opt.MapFrom(src => src.Created))
            .ForMember(dest => dest.created_by, opt => opt.MapFrom(src => src.Created_by))
            .ForMember(dest => dest.updated, opt => opt.MapFrom(src => src.Updated))
            .ForMember(dest => dest.active, opt => opt.MapFrom(src => src.Active))
            .ForMember(dest => dest.deleted, opt => opt.MapFrom(src => src.Deleted));
        CreateMap<ProjectMemberUpdate, ProjectMember>()
            .ForMember(dest => dest.active, opt => opt.MapFrom(src => src.Active))
            .ForMember(dest => dest.updated, opt => opt.MapFrom(src => src.Updated))
            .ForMember(dest => dest.updated_by, opt => opt.MapFrom(src => src.Updated_by))
            .ForMember(dest => dest.deleted, opt => opt.MapFrom(src => src.Deleted));
    }
}
