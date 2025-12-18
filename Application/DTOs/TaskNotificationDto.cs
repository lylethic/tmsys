using System;

namespace server.Application.DTOs;

public class TaskNotificationDto
{
    public Guid Task_id { get; set; }
    public string Task_name { get; set; }
    public Guid User_Id_To_Notify { get; set; }
    public DateTime Next_Update_Due_Date { get; set; }
    public int Days_Overdue_Or_Remaining { get; set; }
}
