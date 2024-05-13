using System;
using System.Collections.Generic;

namespace TMS_Project.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? UserFirstName { get; set; }

    public string? UserLastName { get; set; }

    public string? Email { get; set; }

    public int? ManagerId { get; set; }

    public int? RoleId { get; set; }

    public string? Password { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? MDelete { get; set; }

    public string? Contact { get; set; }

    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

    public virtual ICollection<User> InverseManager { get; set; } = new List<User>();

    public virtual User? Manager { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Project> ProjectManagers { get; set; } = new List<Project>();

    public virtual ICollection<Project> ProjectTeamLeads { get; set; } = new List<Project>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    public string UserName
    {
        get
        {
            return $"{UserFirstName} {UserLastName}";
        }
    }
}
