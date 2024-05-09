using System;
using System.Collections.Generic;
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

    //[Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly TmsContext _context;
        //this is session of userid that which user is added 
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly int UserID;
        public RolesController(TmsContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            UserID = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // GET: Roles
        public async Task<IActionResult> Index()
        {
            return View(await _context.Roles.Where(x=>x.MDelete==false|| x.MDelete == null).ToListAsync());
        }

        // GET: Roles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(m => m.RoleId == id);
            if (role == null)
            {
                return NotFound();
            }

            return PartialView("_Details", role);
        }

        // GET: Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoleId,RoleName,RoleDescription")] Role role)
        {
            ActivityLog activityLog = new ActivityLog();
            activityLog.ProjectId = null;
            activityLog.TaskId = null;
            activityLog.UserId = UserID;
            if (ModelState.IsValid)
            {
                if(role.RoleId == null || role.RoleId <= 0)
                {
                   
                    role.CreatedBy = UserID;
                    role.UpdatedBy = UserID;
                    role.CreatedAt= DateTime.Now;
                    role.UpdatedAt= DateTime.Now;

                    _context.Add(role);
                    await _context.SaveChangesAsync();
                    activityLog.LogText = $"Role has been Created at {DateTime.Now} by user id {UserID}";
                    _context.Add(activityLog);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var rolePrevious = await _context.Roles.FindAsync(role.RoleId);
                    _context.Entry(rolePrevious).State = EntityState.Detached;
                    role.UpdatedBy = 1;
                    role.CreatedBy = rolePrevious.CreatedBy;
                    _context.Update(role);
                    await _context.SaveChangesAsync();
                    activityLog.LogText = $"Role has been Updated at {DateTime.Now} by user id {UserID}";
                    _context.Add(activityLog);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true });
            }
            else
            {
                return PartialView("_Create", role);

            }


        }

        // GET: Roles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return PartialView("_Create",role);
        }

        // POST: Roles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoleId,RoleName,RoleDescription")] Role role)
        {
            if (id != role.RoleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
					var rolePrevious = await _context.Roles.FindAsync(id);
					_context.Entry(rolePrevious).State = EntityState.Detached;
					role.UpdatedBy = 1;
                    role.CreatedBy = rolePrevious.CreatedBy;
                    _context.Update(role);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoleExists(role.RoleId))
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
            return View(role);
        }

        // GET: Roles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(m => m.RoleId == id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ActivityLog activityLog = new ActivityLog();
            activityLog.ProjectId = null;
            activityLog.TaskId = null;
            activityLog.UserId = UserID;
            var role = await _context.Roles.FindAsync(id);
            if (role != null)
            {
               
                role.MDelete = true;
                role.UpdatedAt = DateTime.Now;
                role.UpdatedBy = UserID;
                _context.Update(role);
            }
           
            await _context.SaveChangesAsync();
            activityLog.LogText = $"Role has been Deleted at {DateTime.Now} by user id {UserID}";
            _context.Add(activityLog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoleExists(int id)
        {
            return _context.Roles.Any(e => e.RoleId == id);
        }
    }
}
