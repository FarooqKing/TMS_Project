using System;
using System.Collections.Generic;

namespace TMS_Project.Models;

public partial class Notification : ISoftDeleteTable
{
    public int NotificatonId { get; set; }

    public string? NotificatonDetails { get; set; }

    public int? TaskId { get; set; }

    public int NotificationTo { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool? MDelete { get; set; }

    public virtual User NotificationToNavigation { get; set; } = null!;

    public virtual Task? Task { get; set; }
}
