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

    //[Authorize(Roles = "Team Lead")]
    public class FollowupsController : Controller
    {
        private readonly TmsContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly int UserID;
        public FollowupsController(TmsContext context , IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            UserID = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        }

        // GET: Followups
        public async Task<IActionResult> Index()
        {
            var tmsContext = _context.Followups.Where(x => x.MDelete == false || x.MDelete == null).Include(f => f.Task);
            ViewData["TaskId"] = new SelectList(_context.Tasks.Where(x => x.MDelete == false || x.MDelete == null), "TaskId", "Title");
            return View(await tmsContext.ToListAsync());
        }

        // GET: Followups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var followup = await _context.Followups
                .Include(f => f.Task)
                .FirstOrDefaultAsync(m => m.FollowupId == id);
            if (followup == null)
            {
                return NotFound();
            }

            return View(followup);
        }

        // GET: Followups/Create


        // POST: Followups/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FollowupId,DueDate,Remarks,Status,TaskId,FollowupTitle")] Followup followup)
        {
            ActivityLog activityLog = new ActivityLog();
            activityLog.ProjectId = null;
            activityLog.TaskId = null;
            activityLog.UserId = UserID;
            if (ModelState.IsValid)
            {
                if (followup.FollowupId == null || followup.FollowupId <= 0)
                {
                    followup.CreatedBy = UserID;
                    followup.UpdatedBy = UserID;
                    followup.CreatedAt = DateTime.Now;
                    followup.UpdatedAt = DateTime.Now;

                    _context.Add(followup);
                    await _context.SaveChangesAsync();
                    activityLog.LogText = $"FollowUp has been Created at {DateTime.Now} by user id {UserID}";
                    _context.Add(activityLog);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var followupPrevious = await _context.Followups.FindAsync(followup.FollowupId);
                    _context.Entry(followupPrevious).State = EntityState.Detached;
                    followup.UpdatedBy = UserID;
                    followup.CreatedBy = followupPrevious.CreatedBy;
                    followup.UpdatedAt = DateTime.Now;
                    followup.CreatedAt = DateTime.Now;
                    _context.Update(followup);
                    await _context.SaveChangesAsync();
                    activityLog.LogText = $"Followup has been Updated at {DateTime.Now} by user id {UserID}";
                    _context.Add(activityLog);
                    await _context.SaveChangesAsync();
                }
                return Json(new { success = true });


            }
            else {
            ViewData["TaskId"] = new SelectList(_context.Tasks.Where(x => x.MDelete == false || x.MDelete == null), "TaskId", "Title", followup.TaskId);
                return PartialView("_Create", followup);
            }
        } 

        // GET: Followups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var followup = await _context.Followups.FindAsync(id);
            if (followup == null)
            {
                return NotFound();
            }
            ViewData["TaskId"] = new SelectList(_context.Tasks.Where(x => x.MDelete == false || x.MDelete == null), "TaskId", "Title", followup.TaskId);
            return PartialView("_Create",followup);
        }

        // POST: Followups/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FollowupId,DueDate,Remarks,Status,TaskId,FollowupTitle")] Followup followup)
        {
            if (id != followup.FollowupId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
             
                try
                {
					var followupPrevious = await _context.Followups.FindAsync(id);
					_context.Entry(followupPrevious).State = EntityState.Detached;
					followup.CreatedBy = followupPrevious.CreatedBy;
					followup.UpdatedBy = UserID;
					_context.Update(followup);
                    await _context.SaveChangesAsync();
                   
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FollowupExists(followup.FollowupId))
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
            ViewData["TaskId"] = new SelectList(_context.Tasks.Where(x => x.MDelete == false || x.MDelete == null), "TaskId", "TaskId", followup.TaskId);
            return View(followup);
        }

        // GET: Followups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var followup = await _context.Followups
                .Include(f => f.Task)
                .FirstOrDefaultAsync(m => m.FollowupId == id);
            if (followup == null)
            {
                return NotFound();
            }

            return View(followup);
        }

        // POST: Followups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ActivityLog activityLog = new ActivityLog();
            activityLog.ProjectId = null;
            activityLog.TaskId = null;
            activityLog.UserId = UserID;
            var followup = await _context.Followups.FindAsync(id);
            if (followup != null)
            {
                _context.Followups.Remove(followup);
            }

            await _context.SaveChangesAsync();
            activityLog.LogText = $"FollowUp has been Deleted at {DateTime.Now} by user id {UserID}";
            _context.Add(activityLog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FollowupExists(int id)
        {
            return _context.Followups.Any(e => e.FollowupId == id);
        }
    }
}
