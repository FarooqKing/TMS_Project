using System;
using System.Collections.Generic;

namespace TMS_Project.Models;

public partial class Task : ISoftDeleteTable
{
    public int TaskId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public int? Priority { get; set; }

    public int? StatusId { get; set; }

    public int? AssignedTo { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ProjectId { get; set; }

    public bool? MDelete { get; set; }

    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

    public virtual User? AssignedToNavigation { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Followup> Followups { get; set; } = new List<Followup>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Project? Project { get; set; }

    public virtual Status? Status { get; set; }
}
