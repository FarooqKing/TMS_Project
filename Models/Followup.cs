using System;
using System.Collections.Generic;

namespace TMS_Project.Models;

public partial class Followup : ISoftDeleteTable
{
    public int FollowupId { get; set; }

    public DateTime? DueDate { get; set; }

    public string? Remarks { get; set; }

    public string? Status { get; set; }

    public int? TaskId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool? MDelete { get; set; }

    public string? FollowupTitle { get; set; }

    public virtual Task? Task { get; set; }
}
