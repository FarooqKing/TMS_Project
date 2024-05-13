using System;
using System.Collections.Generic;

namespace TMS_Project.Models;

public partial class Project
{
    public int ProjectId { get; set; }

    public string? ProjectName { get; set; }

    public int? TeamLeadId { get; set; }

    public int? ManagerId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? MDelete { get; set; }

    public int? StatusId { get; set; }

    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual Comment ? Comment { get; set; }

    public virtual User? Manager { get; set; }
    public virtual Status? Status { get; set; }

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    public virtual User? TeamLead { get; set; }
}
