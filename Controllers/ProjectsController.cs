using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TMS_Project.Models;

namespace TMS.Controllers
{
    [CustomAuthorization]

    //[Authorize(Roles = "Project Manager , Admin")]
    public class ProjectsController : Controller
    {
        private readonly TmsContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly int UserID;



        public ProjectsController(TmsContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            UserID = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // GET: Projects
        //public async Task<IActionResult> Index(int id)
        //{
        //    var managerUsers = await _context.Users.Where(u => (u.RoleId == 1008) && (u.MDelete == false || u.MDelete == null)).ToListAsync();
        //    var Statuses = await _context.Statuses.Where(x =>( x.MDelete == false || x.MDelete == null)).ToListAsync();

        //    ViewData["ManagerId"] = new SelectList(managerUsers, "UserId", "UserName");
        //    ViewData["StatusId"] = new SelectList(Statuses, "StatusID", "StatusName");

        //    var currentUserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        //    var teamMembers = new List<User>();

        //    if (_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role) == "Admin")
        //    {
        //        teamMembers = await _context.Users.ToListAsync();
        //    }
        //    else
        //    {
        //        teamMembers = await _context.Users.Where(x => x.ManagerId == UserID).ToListAsync();
        //    }

        //    ViewBag.teamlead = new SelectList(teamMembers, "UserId", "UserName");
        //    // Retrieve the projects based on user's role and assignment
        //    var projects = Enumerable.Empty<Project>();

        //    if (User.IsInRole("Admin"))
        //    {
        //        // For admin, show all projects
        //        projects = await _context.Projects
        //            .Where(p => p.MDelete == false || p.MDelete == null)
        //            .Include(p => p.Manager)
        //            .Include(p => p.TeamLead)
        //            .ToListAsync();
        //    }
        //    else if (User.IsInRole("Project Manager"))
        //    {
        //        // For project manager, show only projects where they are the manager
        //        projects = await _context.Projects
        //            .Where(p => p.ManagerId == currentUserId && (p.MDelete == false || p.MDelete == null))
        //            .Include(p => p.Manager)
        //            .Include(p => p.TeamLead)
        //            .ToListAsync();
        //    }
        //    else if (User.IsInRole("Team Lead"))
        //    {
        //        // For team lead, show only projects where they are the team lead
        //        projects = await _context.Projects
        //            .Where(p => p.TeamLeadId == currentUserId && (p.MDelete == false || p.MDelete == null))
        //            .Include(p => p.Manager)
        //            .Include(p => p.TeamLead)
        //            .ToListAsync();
        //    }

        //    return View(projects);
        //}

        public async Task<IActionResult> Index(int? id)
        {
            // Your existing code to fetch manager users and statuses remains unchanged
            var managerUsers = await _context.Users.Where(u => (u.RoleId == 1008) && (u.MDelete == false || u.MDelete == null)).ToListAsync();
            var Statuses = await _context.Statuses.Where(x => (x.MDelete == false || x.MDelete == null)).ToListAsync();

            // Rest of your existing code for setting ViewData and ViewBag remains unchanged

            var currentUserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var teamMembers = new List<User>();

            // Your existing code for determining team members based on user's role remains unchanged

            // Retrieve the projects based on user's role and assignment
            var projects = Enumerable.Empty<Project>();

            if (id != null)
            {
                // If status ID is provided (clicked on a status name), filter projects by that status ID
                projects = await _context.Projects
                    .Where(p => p.StatusId == id && (p.MDelete == false || p.MDelete == null))
                    .Include(p => p.Manager)
                    .Include(p => p.TeamLead)
                    .ToListAsync();
            }
            else
            {
                // No status ID provided (not clicked on a status name), show all projects based on user's role
                if (User.IsInRole("Admin"))
                {
                    // For admin, show all projects
                    projects = await _context.Projects
                        .Where(p => p.MDelete == false || p.MDelete == null)
                        .Include(p => p.Manager)
                        .Include(p => p.TeamLead)
                        .ToListAsync();
                }
                else if (User.IsInRole("Project Manager"))
                {
                    // For project manager, show only projects where they are the manager
                    projects = await _context.Projects
                        .Where(p => p.ManagerId == currentUserId && (p.MDelete == false || p.MDelete == null))
                        .Include(p => p.Manager)
                        .Include(p => p.TeamLead)
                        .ToListAsync();
                }
                else if (User.IsInRole("Team Lead"))
                {
                    // For team lead, show only projects where they are the team lead
                    projects = await _context.Projects
                        .Where(p => p.TeamLeadId == currentUserId && (p.MDelete == false || p.MDelete == null))
                        .Include(p => p.Manager)
                        .Include(p => p.TeamLead)
                        .ToListAsync();
                }
            }

            return View(projects);
        }


        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Manager)
                .Include(p => p.TeamLead)
                .FirstOrDefaultAsync(m => m.ProjectId == id);
            if (project == null)
            {
                return NotFound();
            }

            return PartialView("_Details", project);
        }

        // GET: Projects/Create

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,ProjectName,ManagerId")] Project project)
        {
            ActivityLog activityLog = new ActivityLog();
            activityLog.ProjectId = null;
            activityLog.TaskId = null;
            activityLog.UserId = UserID;
            try
            {
                ViewData["ManagerId"] = new SelectList(_context.Users.Where(x => x.MDelete == false || x.MDelete == null), "UserId", "UserName", project.ManagerId);
                if (ModelState.IsValid)
                {
                    if (project.ProjectId == null || project.ProjectId <= 0)
                    {
                        project.CreatedBy = UserID;
                        project.UpdatedBy = UserID;
                        project.UpdatedAt = DateTime.Now;
                        project.CreatedAt = DateTime.Now;

                        _context.Add(project);
                        await _context.SaveChangesAsync();
                        activityLog.LogText = $"Project has been Created at {DateTime.Now} by user id {UserID}";
                        _context.Add(activityLog);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        var projectPrevious = await _context.Projects.FindAsync(project.ProjectId);
                        _context.Entry(projectPrevious).State = EntityState.Detached;
                        project.UpdatedBy = UserID;
                        project.CreatedBy = projectPrevious.CreatedBy;
                        project.TeamLeadId = projectPrevious.TeamLeadId;
                        project.MDelete = projectPrevious.MDelete;
                        project.CreatedAt = projectPrevious.CreatedAt;
                        project.UpdatedAt = DateTime.Now;

                        _context.Update(project);
                        await _context.SaveChangesAsync();
                        activityLog.LogText = $"Project has been Updated at {DateTime.Now} by user id {UserID}";
                        _context.Add(activityLog);
                        await _context.SaveChangesAsync();

                    }

                    return Json(new { success = true });
                }
                else
                {
                    return PartialView("_Create", project);

                }


            }
            catch (Exception exp)
            {
                ViewBag.ErrorMsg = exp.Message;
                return PartialView("_Create", project);
            }
        }


        // Get: Project Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            // Assuming there is a property called RoleId in your User model to represent the role
            int managerRoleId = 1008;
            int teamleadRoleId = 1010;

            // Filter users based on their role
            var managers = _context.Users.Where(x => x.RoleId == managerRoleId);
            var teamleads = _context.Users.Where(x => x.RoleId == teamleadRoleId);
            // Select users for the SelectList
            ViewData["ManagerId"] = new SelectList(managers, "UserId", "UserName", project.ManagerId);

            return PartialView("_Create", project);
        }


        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProjectId,ProjectName,TeamLeadId,ManagerId")] Project project)
        {
            if (id != project.ProjectId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var projectPrevious = await _context.Projects.FindAsync(id);
                    _context.Entry(projectPrevious).State = EntityState.Detached;
                    project.UpdatedBy = UserID;
                    project.CreatedBy = projectPrevious.CreatedBy;
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.ProjectId))
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
            ViewData["ManagerId"] = new SelectList(_context.Users.Where(x => x.MDelete == false || x.MDelete == null), "UserId", "UserName", project.ManagerId);
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Manager)
                .FirstOrDefaultAsync(m => m.ProjectId == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ActivityLog activityLog = new ActivityLog();
            activityLog.ProjectId = null;
            activityLog.TaskId = null;
            activityLog.UserId = UserID;
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                project.MDelete = true;
                project.UpdatedAt = DateTime.Now;
                project.UpdatedBy = UserID;
                _context.Update(project);
            }

            await _context.SaveChangesAsync();
            activityLog.LogText = $"Project has been Deleted at {DateTime.Now} by user id {UserID}";
            _context.Add(activityLog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.ProjectId == id);
        }

        public async Task<IActionResult> AssignedUser(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Manager)
                .FirstOrDefaultAsync(m => m.ProjectId == id);

            if (project == null)
            {
                return NotFound();
            }



            var managers = await _context.Users.Where(u => u.Role.RoleId == 1008).ToListAsync();
            ViewBag.Managers = new SelectList(managers, "UserId", "UserName");
            var teamMembers = new List<User>();

            if (_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role) == "Admin")
            {
                teamMembers = await _context.Users.ToListAsync();
            }
            else
            {
                teamMembers = await _context.Users.Where(x => x.ManagerId == UserID).ToListAsync();
            }

            ViewBag.teamlead = new SelectList(teamMembers, "UserId", "UserName");


            return PartialView("_AssignedUser", project);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignedUser([Bind("ProjectId,ProjectName,TeamLeadId,ManagerId")] Project project)
        {
            ActivityLog activityLog = new ActivityLog();
            activityLog.ProjectId = null;
            activityLog.TaskId = null;
            activityLog.UserId = UserID;
            var managers = await _context.Users.Where(u => u.Role.RoleId == 1008).ToListAsync();
            ViewBag.Managers = new SelectList(managers, "UserId", "UserName", project.ManagerId);

            var teamlead = await _context.Users.Where(u => u.Role.RoleId == 1010).ToListAsync();
            ViewBag.teamlead = new SelectList(teamlead, "UserId", "UserName", project.TeamLeadId);

            if (project.ProjectId < 0)
            {
                return NotFound();
            }


            try
            {
                var projectPrevious = await _context.Projects.FindAsync(project.ProjectId);

                Project newProject = project;
                project = projectPrevious;

                project.UpdatedBy = UserID;
                project.UpdatedAt = DateTime.Now;
                project.TeamLeadId = newProject.TeamLeadId;

                _context.Update(project);
                await _context.SaveChangesAsync();
                activityLog.LogText = $"Project has been Assign to User at {DateTime.Now} by user id {UserID}";
                _context.Add(activityLog);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(project.ProjectId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Json(new { success = true });


        }

        public async Task<IActionResult> ProjectStatus(int? id)
        {
            var project = await _context.Projects.FindAsync(id);
            var Statuses = await _context.Statuses.Where(x => (x.MDelete == false)).ToListAsync();

            ViewData["StatusId"] = new SelectList(Statuses, "StatusID", "StatusName",project.StatusId);

            if (id == null)
            {
                return NotFound();
            }

            if (project == null)
            {
                return NotFound();
            }
            return PartialView("_ProjectStatus", project);
        }

        [HttpPost]
        public async Task<IActionResult> ProjectStatus([Bind("ProjectId,StatusId,StatusName")] Project project)
        {
            if (project == null || project.ProjectId < 0)
            {
                return NotFound();
            }

            try
            {
                var projectToUpdate = await _context.Projects.FindAsync(project.ProjectId);
                if (projectToUpdate == null)
                {
                    return NotFound();
                }

                // Update relevant properties of the projectToUpdate object
                projectToUpdate.StatusId = project.StatusId;
                projectToUpdate.Status.StatusName = project.Status.StatusName; // Assuming you want to update the status name too
                projectToUpdate.UpdatedBy = UserID;
                projectToUpdate.UpdatedAt = DateTime.Now;

                _context.Update(projectToUpdate);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(project.ProjectId))
                {
                    return NotFound();
                }
                else
                {
                    throw; // Consider handling this exception case further as needed
                }
            }
        }
    }
}
