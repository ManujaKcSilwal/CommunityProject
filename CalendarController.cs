using cmty_prjt.Data.Scaffold;
using cmty_prjt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace cmty_prjt.Controllers
{
    public class CalendarController : Controller
    {
        private readonly ClgeventmgmtContext _context;

        public CalendarController(ClgeventmgmtContext context)
        {
            _context = context;
        }

        // Action to display the calendar page
        public IActionResult Index()
        {
            return View();
        }

        // Action to display the Create Event page
        public IActionResult Create()
        {
            // Get events for display
            var events = _context.Events
                .OrderByDescending(e => e.EventDate)
                .ToList();
                
            return View(events);
        }

        // Action to return events in JSON format
        public async Task<JsonResult> GetEvents()
        {
            var events = await _context.Events
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.Description,
                    e.Location,
                    Start = e.EventDate.ToString("yyyy-MM-ddTHH:mm:ss"),  // Use EventDate for calendar display
                    End = e.EventDate.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ss"),  // Default to 1-hour events
                    e.Status
                })
                .ToListAsync();

            return new JsonResult(events);
        }

        // Action to add a new event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEvent([FromForm] Event eventDetails, [FromForm] DateTime EndDate)
        {
            if (ModelState.IsValid)
            {
                eventDetails.CreatedDate = DateTime.UtcNow;
                eventDetails.CreatedById = User.Identity.Name ?? "System"; // Set the creator
                eventDetails.Status = eventDetails.Status ?? "Active";
                
                // If the event duration is needed, it can be stored in the Description
                // Format: "Event ends at: [EndDate]"
                if (EndDate > eventDetails.EventDate)
                {
                    eventDetails.Description += $" (Ends: {EndDate:yyyy-MM-dd HH:mm})";
                }
                
                _context.Events.Add(eventDetails);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Event added successfully", eventDetails });
            }
            return BadRequest(new { success = false, message = "Event details are invalid." });
        }

        // Action to update an existing event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEvent([FromForm] Event eventDetails, [FromForm] DateTime EndDate)
        {
            var eventToUpdate = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventDetails.Id);
            if (eventToUpdate != null)
            {
                eventToUpdate.Title = eventDetails.Title;
                eventToUpdate.EventDate = eventDetails.EventDate;
                eventToUpdate.Location = eventDetails.Location;
                eventToUpdate.Status = eventDetails.Status;
                eventToUpdate.UpdatedDate = DateTime.UtcNow;
                
                // Update description including end date information
                var baseDescription = eventDetails.Description;
                if (baseDescription.Contains("(Ends:"))
                {
                    baseDescription = baseDescription.Substring(0, baseDescription.IndexOf("(Ends:")).Trim();
                }
                
                if (EndDate > eventDetails.EventDate)
                {
                    eventToUpdate.Description = $"{baseDescription} (Ends: {EndDate:yyyy-MM-dd HH:mm})";
                }
                else
                {
                    eventToUpdate.Description = baseDescription;
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Event updated successfully", eventDetails });
            }
            return NotFound(new { success = false, message = "Event not found." });
        }

        // Action to delete an existing event
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Calendar/DeleteEvent/{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventToDelete = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (eventToDelete != null)
            {
                _context.Events.Remove(eventToDelete);
                await _context.SaveChangesAsync();
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Ok(new { success = true, message = "Event deleted successfully" });
                }
                
                // For regular form submission, redirect back to the dashboard
                TempData["SuccessMessage"] = "Event deleted successfully";
                return RedirectToAction("Index", "Home");
            }
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return NotFound(new { success = false, message = "Event not found." });
            }
            
            TempData["ErrorMessage"] = "Event not found";
            return RedirectToAction("Index", "Home");
        }

        // Action to display the Edit Event page
        public async Task<IActionResult> Edit(int id)
        {
            var eventToEdit = await _context.Events.FindAsync(id);
            if (eventToEdit == null)
            {
                return NotFound();
            }
            
            return View(eventToEdit);
        }
    }
}
