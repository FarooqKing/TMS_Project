using System;
using System.Collections.Generic;

namespace TMS_Project.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string? RoleName { get; set; }

    public string? RoleDescription { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool? MDelete { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
