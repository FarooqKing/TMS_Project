using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TMS_Project.Models;

namespace TMS.Controllers
{
    [CustomAuthorization]

    //[Authorize(Roles = "Admin")]
    public class StatusController : Controller
    {
        private readonly TmsContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly int UserID;

        public StatusController(TmsContext context , IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            UserID = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Status
        public async Task<IActionResult> Index(int? id)
        {
           return View(await _context.Statuses.Where(x=>x.MDelete==false || x.MDelete == null).ToListAsync());
        }

        // GET: Status/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var status = await _context.Statuses
                .FirstOrDefaultAsync(m => m.StatusId == id);
            if (status == null)
            {
                return NotFound();
            }

            return PartialView("_Details",status);
        }

        // GET: Status/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Status/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StatusId,StatusName")] Status status)
        {
            ActivityLog activityLog = new ActivityLog();
            activityLog.ProjectId = null;
            activityLog.TaskId = null;
            activityLog.UserId = UserID;
            if (ModelState.IsValid)
            {
                if(status.StatusId == null || status.StatusId <= 0)
                {
                    status.CreatedBy = UserID;
                    status.UpdatedBy = UserID;
                    status.MDelete = false;
                    status.CreatedAt = DateTime.Now;
                    status.UpdatedAt = DateTime.Now;
                    _context.Add(status);
                    await _context.SaveChangesAsync();
                    activityLog.LogText = $"Status has been Created at {DateTime.Now} by user id {UserID}";
                    _context.Add(activityLog);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var statusPrevious = await _context.Statuses.FindAsync(status.StatusId);
                    _context.Entry(statusPrevious).State = EntityState.Detached;
                    status.UpdatedBy = UserID;
                    status.CreatedBy = statusPrevious.CreatedBy;
                    status.CreatedAt=statusPrevious.CreatedAt;
                    status.MDelete=statusPrevious.MDelete;
                    status.UpdatedAt = DateTime.Now;
                    _context.Update(status);
                    await _context.SaveChangesAsync();
                    activityLog.LogText = $"Status has been update at {DateTime.Now} by user id {UserID}";
                    _context.Add(activityLog);
                    await _context.SaveChangesAsync();
                }
                return Json(new { success = true });
                
            }
            else
            {

            return PartialView("_Create",status);
            }
           

        }

        // GET: Status/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var status = await _context.Statuses.FindAsync(id);
            if (status == null)
            {
                return NotFound();
            }
            return PartialView("_Create",status);
        }

        // POST: Status/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StatusId,StatusName")] Status status)
        {
            if (id != status.StatusId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
					var statusPrevious = await _context.Statuses.FindAsync(id);
					_context.Entry(statusPrevious).State = EntityState.Detached;
					status.UpdatedBy = UserID;
					status.CreatedBy = statusPrevious.CreatedBy;
					_context.Update(status);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StatusExists(status.StatusId))
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
            return View(status);
        }

        // GET: Status/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var status = await _context.Statuses
                .FirstOrDefaultAsync(m => m.StatusId == id);
            if (status == null)
            {
                return NotFound();
            }

            return View(status);
        }

        // POST: Status/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ActivityLog activityLog = new ActivityLog();
            activityLog.ProjectId = null;
            activityLog.TaskId = null;
            activityLog.UserId = UserID;
            var status = await _context.Statuses.FindAsync(id);
            if (status != null)
            {
                status.MDelete = true;
                status.UpdatedAt = DateTime.Now;
                status.UpdatedBy = UserID;
                _context.Update(status);
            }

            await _context.SaveChangesAsync();
            activityLog.LogText = $"Status has been Deleted at {DateTime.Now} by user id {UserID}";
            _context.Add(activityLog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StatusExists(int id)
        {
            return _context.Statuses.Any(e => e.StatusId == id);
        }
    }
}
