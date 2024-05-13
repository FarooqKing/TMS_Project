using System;
using System.Collections.Generic;

namespace TMS_Project.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public int? TaskId { get; set; }

    public string? ComentText { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool? MDelete { get; set; }

    public int? ProjectId { get; set; }

    public virtual Project? Project { get; set; }

    public virtual Task? Task { get; set; }
}
