using System;
using Newtonsoft.Json;
using server.Domain.Entities;

namespace server.Application.Models;

public class EntendTaskModel : Tasks
{
    [JsonProperty("extend_user")]
    public virtual ExtendUser? ExtendUser { get; set; }
    public virtual Project? ExtendProject { get; set; }
    public virtual UserModel? ExtendManagerProject { get; set; }
}
