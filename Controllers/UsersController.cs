using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TMS_Project.Models;

namespace TMS.Controllers
{
    [CustomAuthorization]

    public class UsersController : Controller
    {
        private readonly TmsContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly int UserID;

        public UsersController(TmsContext context , IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            UserID = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
         
            
        }

        // GET: Users
        // GET: Users
        public async Task<IActionResult> Index()
        {

            ViewData["RoleId"] = new SelectList(_context.Roles.Where(x => x.MDelete == false || x.MDelete == null), "RoleId", "RoleName");
            ViewData["ManagerId"] = new SelectList(_context.Users.Where(x => x.MDelete == false || x.MDelete == null), "UserId", "UserName");
            var tmsContext = await _context.Users.Include(s => s.Role).Include(x => x.Manager).ToListAsync();


            return View(tmsContext);
        }
        // Get Team

        public async Task<IActionResult> Team(int? id)
        {
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserName");
            ViewData["Project"] = new SelectList(_context.Projects, "ProjectId", "ProjectName");
            var teamContext = new List<User>();
            if (id is null)
            {
                if (_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role) == "Admin")
                {
                    teamContext = await _context.Users.Include(x => x.Role).Include(x => x.Manager).ToListAsync();
                }
                else
                {
                    teamContext = await _context.Users.Include(x => x.Role).Include(x => x.Manager).Where(x => x.ManagerId == UserID).ToListAsync();
                }
            }
            else
            {
                teamContext = await _context.Users.Include(x => x.Role).Include(x => x.Manager).Where(x => x.ManagerId == id).ToListAsync();

            }

            return View(teamContext);
        }

        // GET: Users/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    ViewData["ManagerId"] = new SelectList(_context.Users.Where(x => x.MDelete == false || x.MDelete == null), "UserId", "UserName");
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var user = await _context.Users
        //        .Include(u => u.Role)
        //        .FirstOrDefaultAsync(m => m.UserId == id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    return PartialView("_Details", user);
        //}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.Role.RoleName == "Team member")
            {
                // Assign user to team lead
                ViewData["ManagerId"] = new SelectList(_context.Users.Where(x => x.Role.RoleName == "Team Lead" && (x.MDelete == false || x.MDelete == null)), "UserId", "UserName", user.UserId);
            }
            else if (user.Role.RoleName == "Team Lead")
            {
                // Assign user to manager
                ViewData["ManagerId"] = new SelectList(_context.Users.Where(x => x.Role.RoleName == "Project Manager" && (x.MDelete == false || x.MDelete == null)), "UserId", "UserName", user.UserId);
            }

            return PartialView("_Details", user);
        }

        // GET: Users/Create


        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,UserFirstName,UserLastName,Email,RoleId,Password,Contact,ManagerId")] TMS_Project.Models.User user)
        {
            ActivityLog activityLog = new ActivityLog();
            activityLog.ProjectId = null;
            activityLog.TaskId = null;
            activityLog.UserId = UserID;
            try
            {
                ViewData["RoleId"] = new SelectList(_context.Roles.Where(x => x.MDelete == false || x.MDelete == null), "RoleId", "RoleName", user.RoleId);
                ViewData["ManagerId"] = new SelectList(_context.Users.Where(x => x.MDelete == false || x.MDelete == null), "UserId", "UserName", user.ManagerId);
                if (ModelState.IsValid)
                {
                    if (user.UserId == null || user.UserId <= 0)
                    {
                        user.CreatedBy = UserID;
                        user.UpdatedBy = UserID;
                        user.CreatedDate = DateTime.Now;
                        user.UpdatedAt = DateTime.Now;
                        if (user.ManagerId <= 0)
                        {
                            user.ManagerId = null;
                        }
                        user.MDelete = false;


                        activityLog.LogText = $"User has been created at {DateTime.Now} bu user id {UserID}";
                        activityLog.ProjectId = null;
                        activityLog.TaskId = null;


                        _context.Add(user);

                        await _context.SaveChangesAsync();


                        _context.Add(activityLog);
                        await _context.SaveChangesAsync();

                    }
                    else
                    {
                        var userPrevious = await _context.Users.FindAsync(user.UserId);
                        _context.Entry(userPrevious).State = EntityState.Detached;
                        user.UpdatedBy = UserID;
                       
                        user.UpdatedAt = DateTime.Now;
                        user.CreatedBy = userPrevious.CreatedBy;
                        user.ManagerId = userPrevious.ManagerId;
                        user.MDelete = false;
                        _context.Update(user);
                        await _context.SaveChangesAsync();


                        activityLog.LogText = $"User has been update at {DateTime.Now} bu user id {UserID}";
                        _context.Add(activityLog);
                        await _context.SaveChangesAsync();

                    }

                    return Json(new {Success=true});
                }
                else
                {

                return PartialView("_Create", user);
                }
            }
            catch (Exception exp)
            {
                ViewBag.ErrorMsg = exp.Message;
                return PartialView("_Create", user);
            }

        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["RoleId"] = new SelectList(_context.Roles.Where(x => x.MDelete == false || x.MDelete == null), "RoleId", "RoleName", user.RoleId);
            ViewData["ManagerId"] = new SelectList(_context.Users.Where(x => x.MDelete == false || x.MDelete == null), "UserId", "UserName", user.ManagerId);
            return PartialView("_Create", user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["ManagerId"] = new SelectList(_context.Users.Where(x => x.MDelete == false || x.MDelete == null), "UserId", "UserName", user.ManagerId);

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ActivityLog activityLog = new ActivityLog();
            activityLog.ProjectId = null;
            activityLog.TaskId = null;
            activityLog.UserId = UserID;
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Set MDelete to true to mark the user as deleted
                user.MDelete = true;
                user.UpdatedAt = DateTime.Now;
                user.UpdatedBy = UserID;
                _context.Update(user);
            }

            await _context.SaveChangesAsync();
            activityLog.LogText = $"User has been Deleted at {DateTime.Now} bu user id {UserID}";
            _context.Add(activityLog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }
        //POST method for Assigned TO

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTo([Bind("UserId,UserName,TeamLeadId,ManagerId")] User user)
        {
            ActivityLog activityLog = new ActivityLog();
            activityLog.ProjectId = null;
            activityLog.TaskId = null;
            activityLog.UserId = UserID;
            var assingto = await _context.Users.Where(u => u.Role.RoleId == 6).ToListAsync();
            ViewBag.Managers = new SelectList(assingto, "UserId", "UserName", user.ManagerId);

            var teamlead = await _context.Users.Where(u => u.Role.RoleId == 10).ToListAsync();
            ViewBag.teamlead = new SelectList(teamlead, "UserId", "UserName", user.ManagerId);

            if (user.UserId < 0)
            {
                return NotFound();
            }


            try
            {
                var userPrevious = await _context.Users.FindAsync(user.UserId);

                User newUser = user;
                user = userPrevious;

                user.UpdatedBy = UserID;
                user.UpdatedAt = DateTime.Now;
                user.ManagerId = newUser.ManagerId;

                _context.Update(user);
                await _context.SaveChangesAsync();
                activityLog.LogText = $"User has been Deleted at {DateTime.Now} bu user id {UserID}";
                _context.Add(activityLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(user.UserId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }



        }

        private bool ProjectExists(int userId)
        {
            throw new NotImplementedException();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
