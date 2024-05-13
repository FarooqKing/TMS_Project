using System;
using System.Collections.Generic;

namespace TMS_Project.Models;

public partial class ActivityLog
{
    public int LogsId { get; set; }

    public int? ProjectId { get; set; }

    public int? UserId { get; set; }

    public int? TaskId { get; set; }

    public DateTime? DateTime { get; set; }

    public string? LogText { get; set; }

    public virtual Project? Project { get; set; }

    public virtual Task? Task { get; set; }

    public virtual User? User { get; set; }
}
