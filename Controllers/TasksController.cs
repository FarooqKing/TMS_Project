using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Services;
using TMS_Project.Models;

namespace TMS.Controllers
{
    [CustomAuthorization]

    public class TasksController : Controller
    {
        private readonly TmsContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly int UserId;
        private readonly Services.IMailService _mailService;

        public TasksController(TmsContext context, IHttpContextAccessor httpcontextAccessor, Services.IMailService mailService)
        {
            _context = context;
            _httpContextAccessor = httpcontextAccessor;
            UserId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            _mailService = mailService;
        }

        // GET: Tasks
        public async Task<IActionResult> Index(int id)
        {
            var currentUserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            // Retrieve tasks based on user's role
            IQueryable<TMS_Project.Models.Task> tasksQuery;

            if (User.IsInRole("Admin"))
            {
                // Admin sees all tasks if project ID is specified
                tasksQuery =
                    _context.Tasks.Where(t => t.ProjectId == id);
            }
            else if (User.IsInRole("Team Lead") || User.IsInRole("Project Manager"))
            {

                // Retrieve the current project
                var project = await _context.Projects.FindAsync(id) 
;

                // Check if the current user is the manager or team lead of the project
                if (project.ManagerId == currentUserId || project.TeamLeadId == currentUserId)
                {
                    // Retrieve tasks assigned to the project
                    tasksQuery = _context.Tasks.Where(t => t.ProjectId == id && (t.MDelete == false || t.MDelete == null));

                    // Log the generated SQL query for debugging
                    Console.WriteLine(tasksQuery.ToQueryString());
                }
                else
                {
                    // If the current user is not authorized to view tasks for the project, redirect to access denied page
                    return RedirectToAction("AccessDenied", "Error");
                }

            }
            else // Team member
            {
                // Team member sees only tasks assigned to them
                tasksQuery = _context.Tasks.Where(t =>
                    t.AssignedTo == currentUserId &&
                    (t.MDelete == false || t.MDelete == null));
            }

            var tasks = await tasksQuery
                .Include(t => t.Project)
                .Include(t => t.Status) // Include Status navigation property
                .Include(t => t.AssignedToNavigation)
                .ToListAsync();

            // Populate ViewData if needed
            if (id == null)
            {
                ViewData["ProjectId"] = new SelectList(_context.Projects.Where(x => x.TeamLeadId == currentUserId && (x.MDelete == false || x.MDelete == null)), "ProjectId", "ProjectName");

            }
            else
            {
                ViewData["ProjectId"] = new SelectList(_context.Projects.Where(x => x.ProjectId == id && (x.MDelete == false || x.MDelete == null)), "ProjectId", "ProjectName");

            }
            ViewData["StatusId"] = new SelectList(_context.Statuses.Where(x => x.MDelete == false || x.MDelete == null), "StatusId", "StatusName");
            var teamMembers = new List<User>();

            if (_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role) == "Admin")
            {
                teamMembers = await _context.Users.ToListAsync();
            }
            else
            {
                teamMembers = await _context.Users.Where(x => x.ManagerId == UserId).ToListAsync();
            }

            ViewBag.AssignedTo = new SelectList(teamMembers, "UserId", "UserName");

            return View(tasks);
        }


        //public async Task<IActionResult> Index(int? id)
        //{
        //    var currentUserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        //    var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

        //    // Retrieve tasks based on user's role and project ID
        //    IQueryable<TMS_Project.Models.Task> tasksQuery;

        //    if (id.HasValue) // If project ID is provided, filter tasks by project
        //    {
        //        tasksQuery = _context.Tasks.Where(t =>
        //            t.ProjectId == id &&
        //            (t.MDelete == false || t.MDelete == null));

        //        // Additional filtering based on user role
        //        if (User.IsInRole("Team Lead"))
        //        {
        //            tasksQuery = tasksQuery.Where(t =>
        //                t.AssignedToNavigation.ManagerId == currentUserId);
        //        }
        //        else if (User.IsInRole("Team Member"))
        //        {
        //            tasksQuery = tasksQuery.Where(t =>
        //                t.AssignedTo == currentUserId);
        //        }
        //    }
        //    else // If project ID is not provided, show all tasks based on user role
        //    {
        //        tasksQuery = _context.Tasks.Where(t =>
        //            (t.MDelete == false || t.MDelete == null));

        //        // Additional filtering based on user role
        //        if (User.IsInRole("Team Lead"))
        //        {
        //            var currentUserTeamId = _context.Users
        //                .Where(u => u.ManagerId == currentUserId)
        //                .Select(u => u.ManagerId)
        //                .FirstOrDefault();

        //            tasksQuery = tasksQuery.Where(t =>
        //                t.AssignedToNavigation.ManagerId == currentUserTeamId);
        //        }
        //        else if (User.IsInRole("Team Member"))
        //        {
        //            tasksQuery = tasksQuery.Where(t =>
        //                t.AssignedTo == currentUserId);
        //        }
        //    }

        //    var tasks = await tasksQuery
        //        .Include(t => t.Project)
        //        .Include(t => t.Status)
        //        .Include(t => t.AssignedToNavigation)
        //        .ToListAsync();

        //    // Provide data for dropdowns if needed
        //    ViewData["ProjectId"] = new SelectList(_context.Projects.Where(x => x.MDelete == false || x.MDelete == null), "ProjectId", "ProjectName");
        //    ViewData["StatusId"] = new SelectList(_context.Statuses.Where(x => x.MDelete == false || x.MDelete == null), "StatusId", "StatusName");

        //    return View(tasks);
        //}



        // GET: Tasks/Details/5



        public async Task<IActionResult> TaskDetail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Status)
                .Include(t => t.AssignedToNavigation)
                .Include(t => t.Followups.OrderByDescending(x => x.FollowupId))

                .FirstOrDefaultAsync(m => m.TaskId == id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }



        // POST: Tasks/Create
        //  To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("TaskId,Title,Description,DueDate,Priority,StatusId,AssignedTo,ProjectId")] TMS_Project.Models.Task task)
        //{
        //    ActivityLog activityLog = new ActivityLog();
        //    activityLog.ProjectId = null;
        //    activityLog.TaskId = null;
        //    activityLog.UserId = UserId;

        //    ViewData["ProjectId"] = new SelectList(_context.Projects, "ProjectId", "ProjectName", task.ProjectId);

        //    ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusName", task.StatusId);

        //    //  ViewData["AssignedTo"] = new SelectList(_context.Users, "UserId", "UserName", task.AssignedToNavigation);

        //    var teamMembers = new List<User>();

        //    if (_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role) == "Admin")
        //    {
        //        teamMembers = await _context.Users.ToListAsync();
        //    }
        //    else
        //    {
        //        teamMembers = await _context.Users.Where(x => x.ManagerId == UserId).ToListAsync();
        //    }

        //    // Create a SelectList with filtered team members
        //    ViewData["AssignedTo"] = new SelectList(teamMembers, "UserId", "UserName", task.AssignedToNavigation);

        //    // Create a SelectList with filtered team members
        //    ViewData["AssignedTo"] = new SelectList(teamMembers, "UserId", "UserName", task.AssignedToNavigation);


        //    if (ModelState.IsValid)
        //    {
        //        if (task.TaskId == null || task.TaskId <= 0)
        //        {
        //            task.CreatedBy = UserId;
        //            task.UpdatedBy = UserId;
        //            task.UpdatedAt = DateTime.Now;
        //            task.CreatedAt = DateTime.Now;
        //            task.MDelete = false;
        //            activityLog.LogText = $"Task Has been Created At {DateTime.Now} By UserId {UserId}";
        //            activityLog.ProjectId = null;
        //            activityLog.TaskId = null;


        //            _context.Add(task);
        //            await _context.SaveChangesAsync();

        //            _context.Add(activityLog);
        //            await _context.SaveChangesAsync();

        //        }
        //        else
        //        {
        //            var taskPrevious = await _context.Tasks.FindAsync(task.TaskId);
        //            _context.Entry(taskPrevious).State = EntityState.Detached;
        //            task.UpdatedBy = UserId;
        //            task.CreatedBy = taskPrevious.CreatedBy;
        //            _context.Update(task);
        //            task.UpdatedAt = DateTime.Now;
        //            task.CreatedAt = DateTime.Now;
        //            task.MDelete = false;
        //            await _context.SaveChangesAsync();


        //            activityLog.LogText = $"Task Has been Updated At {DateTime.Now} By UserId {UserId}";

        //            _context.Add(activityLog);
        //            await _context.SaveChangesAsync();

        //        }
        //        return Json(new { Success = true });

        //    }
        //    else { return PartialView("_Create", task); }


        //}

        public async Task<IActionResult> Create([Bind("TaskId,Title,Description,DueDate,Priority,StatusId,AssignedTo,ProjectId")] TMS_Project.Models.Task task)
        {
            ActivityLog activityLog = new ActivityLog();
            activityLog.ProjectId = null;
            activityLog.TaskId = null;
            activityLog.UserId = UserId;

            ViewData["ProjectId"] = new SelectList(_context.Projects, "ProjectId", "ProjectName", task.ProjectId);


            ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusName", task.StatusId);

            var teamMembers = new List<User>();

            if (_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role) == "Admin")
            {
                teamMembers = await _context.Users.ToListAsync();
            }
            else
            {
                teamMembers = await _context.Users.Where(x => x.ManagerId == UserId).ToListAsync();
            }

            ViewData["AssignedTo"] = new SelectList(teamMembers, "UserId", "UserName", task.AssignedToNavigation);



            if (ModelState.IsValid)
            {
                if (task.TaskId == null || task.TaskId <= 0)
                {
                    task.CreatedBy = UserId;
                    task.UpdatedBy = UserId;
                    task.UpdatedAt = DateTime.Now;
                    task.CreatedAt = DateTime.Now;
                    task.MDelete = false;
                    activityLog.LogText = $"Task Has been Created At {DateTime.Now} By UserId {UserId}";
                    activityLog.ProjectId = null;
                    activityLog.TaskId = null;


                    _context.Add(task);
                    await _context.SaveChangesAsync();

                    _context.Add(activityLog);
                    await _context.SaveChangesAsync();

                    //var user = _context.Users.Where(x => x.UserId == UserId).FirstOrDefault();

                    //var assignTo = _context.Users.Where(x => x.UserId == task.AssignedTo).FirstOrDefault();

                    //string Message = $"New Task {task.Title} has been assigned to you by {user.UserName} at {DateTime.Now.ToString("dd MMM, yyyy hh:mm tt")}";



                    //await _mailService.SendMailAsync(assignTo.Email, "New Task Assigned", Message);

                }
                else
                {
                    var taskPrevious = await _context.Tasks.FindAsync(task.TaskId);
                    _context.Entry(taskPrevious).State = EntityState.Detached;
                    task.UpdatedBy = UserId;
                    task.CreatedBy = taskPrevious.CreatedBy;
                    _context.Update(task);
                    task.UpdatedAt = DateTime.Now;
                    task.CreatedAt = DateTime.Now;
                    task.MDelete = false;
                    await _context.SaveChangesAsync();


                    activityLog.LogText = $"Task Has been Updated At {DateTime.Now} By UserId {UserId}";

                    _context.Add(activityLog);
                    await _context.SaveChangesAsync();

                }
                return Json(new { Success = true });

            }
            else { return PartialView("_Create", task); }


        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "ProjectId", "ProjectName", task.ProjectId);
            ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusName", task.StatusId);
            // ViewData["AssignedTo"] = new SelectList(_context.Users, "UserId", "UserName", task.AssignedTo);
            var teamMembers = new List<User>();

            if (_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role) == "Admin")
            {
                teamMembers = await _context.Users.ToListAsync();
            }
            else
            {
                teamMembers = await _context.Users.Where(x => x.ManagerId == UserId).ToListAsync();
            }

            // Create a SelectList with filtered team members
            ViewData["AssignedTo"] = new SelectList(teamMembers, "UserId", "UserName", task.AssignedToNavigation);

            // Create a SelectList with filtered team members
            ViewData["AssignedTo"] = new SelectList(teamMembers, "UserId", "UserName", task.AssignedToNavigation);


            return PartialView("_Create", task);
        }

        // POST: Tasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TaskId,Title,Description,DueDate,Priority,StatusId,AssignedTo,ProjectId")] TMS_Project.Models.Task task)
        {
            if (id != task.TaskId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var taskPrevious = await _context.Tasks.FindAsync(id);
                    _context.Entry(taskPrevious).State = EntityState.Detached;
                    task.UpdatedBy = UserId;
                    task.CreatedBy = taskPrevious.CreatedBy;
                    _context.Update(task);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskExists(task.TaskId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "ProjectId", "ProjectName", task.ProjectId);
            ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusName", task.StatusId);
            ViewData["AssignedTo"] = new SelectList(_context.Users, "UserId", "UserName", task.AssignedTo);

            return View(task);
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Status)
                .FirstOrDefaultAsync(m => m.TaskId == id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ActivityLog activityLog = new ActivityLog();
            activityLog.ProjectId = null;
            activityLog.TaskId = null;
            activityLog.UserId = UserId;
            var task = await _context.Tasks.FindAsync(id);

            _context.Remove(task);

            if (task != null)
            {
                task.MDelete = true;
                task.UpdatedAt = DateTime.Now;
                task.UpdatedBy = UserId;
                _context.Tasks.Update(task);
            }
            await _context.SaveChangesAsync();

            activityLog.LogText = $"Task has been Deleted at{DateTime.Now} By UserId{UserId}";
            _context.Add(activityLog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
  

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.TaskId == id);
        }

   


    }


}
