using System;
using System.Collections.Generic;

namespace TMS_Project.Models;

public partial class Status : ISoftDeleteTable
{
    public int StatusId { get; set; }

    public string? StatusName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool? MDelete { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
