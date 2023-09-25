using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using elearningapp.Data;
using elearningapp.Models;
using System.Web;
using LearningApp.Models;
using System.Web.Helpers;

namespace elearningapp.Controllers
{
    public class AssignmentsController : Controller
    {
        private readonly LearningAppIdentityDbContext _context;

        public AssignmentsController(LearningAppIdentityDbContext context)
        {
            _context = context;
        }

        // GET: Assignments
        public IActionResult Index(int id)
        {
            var con = (from x in _context.Courses
                     where x.Id == id
                     select x).FirstOrDefault();
            CourseDetailsViewModel concon = new CourseDetailsViewModel();

            concon.CourseTitle = con.Title;
            concon.CourseImageUrl = con.ImageUrl;
            concon.CourseDescription = con.Description;
            concon.CourseCategory = con.Category;
			string[] parts = (from x in _context.Users
							  where x.Id == con.InstructorId
							  select x.UserName).FirstOrDefault().Split('@');
			concon.InstructorName = parts[0];
            concon.LastUpdate = (from x in _context.Assignments
                                 where x.CourseId == con.Id
                                 orderby x.DueDate ascending
                                 select x.DueDate
                                 ).Take(1).FirstOrDefault();

			concon.CourseEnrollmentCount = con.EnrollmentCount;
            concon.CourseDuration = con.CourseDuration;
            concon.CourseId = con.Id;

            List<Assignments> assignments = new List<Assignments>();
            var assign = (from x in _context.Assignments
                          where x.CourseId == id
                          select x).ToList();
			assignments = assign;
            concon.Assignments = assignments;
            return View(concon);
           
        }
        // GET: Assignments/Create
        public IActionResult Create([FromQuery] int CourseId)
        {
            ViewData["CourseId"] = CourseId;
            ViewData["CourseName"] = (from x in _context.Courses
                                     where x.Id == CourseId
                                     select x.Title).FirstOrDefault();

			return View();
        }

        // POST: Assignments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseId,Title,Description,DueDate")] Assignments assignments)
        {
            /*var course_id = (from x in _context.Courses
                            where ViewData["CourseName"] == x.Title
                            select x.Id).FirstOrDefault();*/
            if (ModelState.IsValid)
            {
                _context.Add(assignments);
                await _context.SaveChangesAsync();
				return RedirectToAction("AssignmentList", "Users", new { assignments.CourseId });
			}
            return View(assignments);
        }

        // GET: Assignments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var course_variables = (from course in _context.Courses
                                        join assignment in _context.Assignments on course.Id equals assignment.CourseId
                                        where assignment.Id == id
                                        select course).FirstOrDefault();

            ViewData["CourseId"] = course_variables.Id;
            ViewData["CourseName"] = course_variables.Title;

            if (id == null || _context.Assignments == null)
            {
                return NotFound();
            }

            var assignments = await _context.Assignments.FindAsync(id);
            if (assignments == null)
            {
                return NotFound();
            }
            return View(assignments);
        }

        // POST: Assignments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CourseId,Title,Description,DueDate")] Assignments assignments)
        {
            var course_id = assignments.CourseId;
            if (id != assignments.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(assignments);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssignmentsExists(assignments.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("AssignmentList","Users", new { CourseId = course_id });
            }
            return RedirectToAction("AssignmentList", "Users", assignments.CourseId);
        }

        // GET: Assignments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Assignments == null)
            {
                return NotFound();
            }

            var assignments = await _context.Assignments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (assignments == null)
            {
                return NotFound();
            }

            return View(assignments);
        }

        // POST: Assignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Assignments == null)
            {
                return Problem("Entity set 'LearningAppDbContext.Assignments'  is null.");
            }
            var assignments = await _context.Assignments.FindAsync(id);
            var courseid = (from x in _context.Assignments
						   where x.Id == id
						   select x.CourseId).FirstOrDefault();

			if (assignments != null)
            {
                _context.Assignments.Remove(assignments);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction("AssignmentList", "Users", new {courseid });
        }

        private bool AssignmentsExists(int id)
        {
          return (_context.Assignments?.Any(e => e.Id == id)).GetValueOrDefault();
        }

		[HttpPost]
		public ActionResult CallToList()
		{
			// Get the URL of the previous page
			string previousPageUrl = Request.Headers["Referer"].ToString();

			if (!string.IsNullOrEmpty(previousPageUrl))
			{
				// Redirect back to the previous page
				return Redirect(previousPageUrl);
			}
			else
			{
				// Handle the case where there is no previous page URL
				// You can redirect to a default page or display a message to the user
				return RedirectToAction("CourseList", "UsersController"); // Example: Redirect to a default page
			}
		}
	}
}
