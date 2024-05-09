using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TMS_Project.Models;

public partial class TmsContext : DbContext
{
    public TmsContext()
    {
    }

    public TmsContext(DbContextOptions<TmsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Followup> Followups { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=UMAR-PC; Database=TMS ; Trusted_Connection=True; TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.LogsId).HasName("PK_ActivityLogs_1");

            entity.Property(e => e.LogText).HasColumnType("text");

            entity.HasOne(d => d.Project).WithMany(p => p.ActivityLogs)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_ActivityLogs_Project");

            entity.HasOne(d => d.Task).WithMany(p => p.ActivityLogs)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("FK_ActivityLogs_Tasks");

            entity.HasOne(d => d.User).WithMany(p => p.ActivityLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ActivityLogs_Users");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comments__C3B4DFAA1AB153B3");

            entity.Property(e => e.CommentId).HasColumnName("CommentID");
            entity.Property(e => e.ComentText).HasColumnType("text");
            entity.Property(e => e.MDelete).HasColumnName("mDelete");
            entity.Property(e => e.TaskId).HasColumnName("TaskID");

            entity.HasOne(d => d.Task).WithMany(p => p.Comments)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("FK_Comments_Tasks");
        });

        modelBuilder.Entity<Followup>(entity =>
        {
            entity.Property(e => e.FollowupTitle).HasMaxLength(50);
            entity.Property(e => e.MDelete).HasColumnName("mDelete");
            entity.Property(e => e.Remarks).HasColumnType("text");
            entity.Property(e => e.Status).HasMaxLength(15);

            entity.HasOne(d => d.Task).WithMany(p => p.Followups)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("FK_Followups_Tasks");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificatonId);

            entity.Property(e => e.MDelete).HasColumnName("mDelete");
            entity.Property(e => e.NotificatonDetails).HasColumnType("text");

            entity.HasOne(d => d.NotificationToNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.NotificationTo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_Users");

            entity.HasOne(d => d.Task).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("FK_Notifications_Tasks");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Project");

            entity.Property(e => e.MDelete).HasColumnName("mDelete");
            entity.Property(e => e.ProjectName).HasMaxLength(50);
            entity.Property(e => e.StatusId).HasColumnName("StatusID");

            entity.HasOne(d => d.Manager).WithMany(p => p.ProjectManagers)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK_Project_Users");

            entity.HasOne(d => d.Status).WithMany(p => p.Projects)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK_Project_Status");

            entity.HasOne(d => d.TeamLead).WithMany(p => p.ProjectTeamLeads)
                .HasForeignKey(d => d.TeamLeadId)
                .HasConstraintName("FK_Project_Users1");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.MDelete).HasColumnName("mDelete");
            entity.Property(e => e.RoleDescription).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.ToTable("Status");

            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.MDelete).HasColumnName("mDelete");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK__Tasks__7C6949D12BB5B8EB");

            entity.Property(e => e.TaskId).HasColumnName("TaskID");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.MDelete).HasColumnName("mDelete");
            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.AssignedToNavigation).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.AssignedTo)
                .HasConstraintName("FK_Tasks_Users");

            entity.HasOne(d => d.Project).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Tasks_Project");

            entity.HasOne(d => d.Status).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK_Tasks_Status");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Contact).HasMaxLength(15);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.MDelete).HasColumnName("mDelete");
            entity.Property(e => e.Password).HasMaxLength(10);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.UserFirstName)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UserLastName)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Manager).WithMany(p => p.InverseManager)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK_Users_Users");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_Users_Roles");
        });

    modelBuilder.Entity<Project>().HasQueryFilter(c => c.MDelete==false || c.MDelete==null);
    modelBuilder.Entity<User>().HasQueryFilter(c => c.MDelete==false || c.MDelete==null);
    modelBuilder.Entity<Task>().HasQueryFilter(c => c.MDelete==false || c.MDelete==null);
    modelBuilder.Entity<Followup>().HasQueryFilter(c => c.MDelete==false || c.MDelete==null);
    modelBuilder.Entity<Role>().HasQueryFilter(c => c.MDelete==false || c.MDelete==null);
    modelBuilder.Entity<Status>().HasQueryFilter(c => c.MDelete==false || c.MDelete==null);
    modelBuilder.Entity<Comment>().HasQueryFilter(c => c.MDelete==false || c.MDelete==null);
        modelBuilder.Entity<Notification>().HasQueryFilter(c => c.MDelete == false || c.MDelete == null);
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
