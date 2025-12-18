using System;
using System.ComponentModel.DataAnnotations.Schema;
using server.Common.Domain.Entities;
using server.Common.Domain.Request.Create;
using server.Common.Domain.Request.Update;
using server.Common.Interfaces;

namespace server.Application.DTOs;

public class ProjectTypeDto
{

}

public class CreateProjectType : DomainCreate
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class UpdateProjectType : DomainUpdate
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class ProjectTypeModel : DomainModel, IHasTotalCount
{
    public string Name { get; set; }
    public string Description { get; set; }
}