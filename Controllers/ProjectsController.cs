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
using TMS.Models;
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
        private readonly Services.IMailService _mailService;

        public ProjectsController(TmsContext context, IHttpContextAccessor httpcontextAccessor, Services.IMailService mailService)
        {
            _context = context;
            _httpContextAccessor = httpcontextAccessor;
            UserID = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            _mailService = mailService;
        }





        public async Task<IActionResult> Index(int? id)
        {

            var managerUsers = await _context.Users.Where(u => u.RoleId == 2).ToListAsync();
            var Statuses = await _context.Statuses.ToListAsync();



            var currentUserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var teamMembers = new List<User>();


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

                if (User.IsInRole("Admin"))
                {

                    projects = await _context.Projects
                        .Where(p => p.MDelete == false || p.MDelete == null)
                        .Include(p => p.Manager)
                        .Include(p => p.TeamLead)
                        .ToListAsync();
                }
                else if (User.IsInRole("Project Manager"))
                {

                    projects = await _context.Projects
                        .Where(p => p.ManagerId == currentUserId && (p.MDelete == false || p.MDelete == null))
                        .Include(p => p.Manager)
                        .Include(p => p.TeamLead)
                        .ToListAsync();
                }
                else if (User.IsInRole("Team Lead"))
                {

                    projects = await _context.Projects
                        .Where(p => p.TeamLeadId == currentUserId && (p.MDelete == false || p.MDelete == null))
                        .Include(p => p.Manager)
                        .Include(p => p.TeamLead)
                        .ToListAsync();
                }
            }
            ViewData["ManagerId"] = new SelectList(_context.Users.Where(x => x.RoleId == 1008 && (x.MDelete==false ||x.MDelete ==null)), "UserId", "UserName");
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
                ViewData["ManagerId"] = new SelectList(_context.Users.Where(x => x.RoleId == 1008), "UserId", "UserName", project.ManagerId);
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


                        var user = _context.Users.Where(x => x.UserId == UserID).FirstOrDefault();

                        var assignTo = _context.Users.Where(x => x.UserId == project.ManagerId).FirstOrDefault();

                        string Message = $"New Project {project.ProjectName} has been assigned to you by {user.UserName} at {DateTime.Now.ToString("dd MMM, yyyy hh:mm tt")}";



                        await _mailService.SendMailAsync(assignTo.Email, "New Project Assigned", Message);

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
            int managerRoleId = 2;
            int teamleadRoleId = 6;

            // Filter users based on their role
            var managers = _context.Users.Where(x => x.RoleId == managerRoleId);
            var teamleads = _context.Users.Where(x => x.RoleId == teamleadRoleId);
            // Select users for the SelectList
            ViewData["ManagerId"] = new SelectList(_context.Users.Where(x => x.RoleId == 1008), "UserId", "UserName", project.ManagerId);

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



            var managers = await _context.Users.Where(u => u.Role.RoleId == 1008 && (u.MDelete==false || u.MDelete==null )).ToListAsync();
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
            var managers = await _context.Users.Where(u => u.Role.RoleId == 1008 && (u.MDelete ==false || u.MDelete==null)).ToListAsync();
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
              



                activityLog.LogText = $"Project has been Assign to User at {DateTime.Now} by user id {UserID}";
                _context.Add(activityLog);
                await _context.SaveChangesAsync();
                //var user = _context.Users.Where(x => x.UserId == UserID).FirstOrDefault();

                //var assignTo = _context.Users.Where(x => x.UserId == project.ManagerId).FirstOrDefault();

                //string Message = $"New Project {project.ProjectName} has been assigned to you by {user.UserName} at {DateTime.Now.ToString("dd MMM, yyyy hh:mm tt")}";



                //await _mailService.SendMailAsync(assignTo.Email, "New Project Assigned", Message);

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
                    throw;
                }
            }
           


        }

        public async Task<IActionResult> ProjectStatus(int? id)
        {
            var project = await _context.Projects.FindAsync(id);
            var Statuses = await _context.Statuses.Where(x => (x.MDelete == false)).ToListAsync();

            ViewData["StatusId"] = new SelectList(Statuses, "StatusID", "StatusName", project.StatusId);

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
        public async Task<IActionResult> ProjectStatus([Bind("ProjectId,StatusId,StatusName,Comment")] Project project)
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

                var newProject = project;

                project = projectToUpdate;

                project.StatusId = newProject.StatusId;

                project.UpdatedBy = UserID;
                project.UpdatedAt = DateTime.Now;

                _context.Projects.Update(projectToUpdate);
                await _context.SaveChangesAsync();

                var comments = newProject.Comment;

                comments.ProjectId = newProject.ProjectId;
                comments.CreatedBy = UserID;
                comments.CreatedAt = DateTime.Now;
                comments.UpdatedBy = UserID;
                comments.UpdatedAt = DateTime.Now;


                _context.Comments.Add(comments);
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
                    throw;
                }
            }
        }
    }
}
