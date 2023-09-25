using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using elearningapp.Data;
using elearningapp.Models;
using LearningApp.Models;
using Microsoft.AspNetCore.Authorization;
using PagedList;
using PagedList.Mvc;
using NuGet.Protocol.Core.Types;
using System.Drawing.Printing;
using Microsoft.AspNetCore.Hosting;

namespace LearningApp.Controllers
{
    public class CoursesController : Controller
    {
        private readonly LearningAppIdentityDbContext _context;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public CoursesController(LearningAppIdentityDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
			_webHostEnvironment = webHostEnvironment;
		}

        // GET: Courses
        [HttpGet]
		public IActionResult Index(int p = 1)
		{
		    var headings = _context.Courses.ToPagedList(p, 6);
			return _context.Courses != null ?
			View(headings) :
              Problem("Entity set 'LearningAppIdentityDbContext.Courses'  is null.");
        }
        [HttpPost]
        public IActionResult Index(int p = 1, string searchString="")
        {
            var sortedCourses = _context.Courses
                                .Where(course => course.Title.Contains(searchString))
                                .ToList();
            var headings = sortedCourses.ToPagedList(p, 6);
            return View(headings);
        }
        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Courses == null)
            {
                return NotFound();
            }

            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (courses == null)
            {
                return NotFound();
            }

            return View(courses);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
			var instructors = (from user in _context.Users
								join userRole in _context.UserRoles on user.Id equals userRole.UserId
								join role in _context.Roles on userRole.RoleId equals role.Id
								where role.Name == "Instructor"
							   select new SelectListItem
							   {
								   Value = user.Id,
								   Text = user.UserName,

							   }).ToList();
			if (User.IsInRole("Admin"))
			{
				ViewData["InstructorId"] = instructors;
			}
            else
            {
                var user_id = HttpContext.User.Claims.First().Value;
				ViewData["InstructorName"] = (from x in _context.Users where x.Id == user_id select x.UserName).FirstOrDefault();
				ViewData["InstructorId"] = user_id;

			}
				
			return View(new CourseCreateDto());
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create( CourseCreateDto courses)
        {
            if (ModelState.IsValid)
            {
				if (courses.CourseImage != null && courses.CourseImage.Length > 0)
				{
					// Dosya yükleme courses
					string fileName = Guid.NewGuid().ToString() + Path.GetExtension(courses.CourseImage.FileName);
					string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);

					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						courses.CourseImage.CopyTo(stream);
					}

					// ImageUrl'i ayarla
					courses.ImageUrl = fileName;
					courses.ImageUrl = courses.ImageUrl;
				}

				_context.Add(new Courses
                {

                    Title = courses.Title,
                    ImageUrl = courses.ImageUrl,
                    Description = courses.Description,
                    CourseDuration = courses.CourseDuration,
                    EnrollmentCount = courses.EnrollmentCount,
                    InstructorId = courses.InstructorId,
                    Category = courses.Category
                });
                await _context.SaveChangesAsync();
                return RedirectToAction("CourseList", "Users");
            }
            ViewData["InstructorId"] = new SelectList(_context.Users, "Id", "Id", courses.InstructorId);
            return View(courses);
        }
        [Authorize(Roles = "Admin, Instructor")]
        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var instructors = (from user in _context.Users
                               join userRole in _context.UserRoles on user.Id equals userRole.UserId
                               join role in _context.Roles on userRole.RoleId equals role.Id
                               where role.Name == "Instructor"
                               select new SelectListItem
                               {
                                   Value = user.Id,
                                   Text = user.UserName,

                               }).ToList();

            if (id == null || _context.Courses == null)
            {
                return NotFound();
            }

			var courses = await _context.Courses.FindAsync(id);
            if (courses == null)
            {
                return NotFound();
            }
            CourseEditDto cto = new CourseEditDto();

			cto.Title = courses.Title;
			cto.Description = courses.Description;
			cto.CourseDuration = courses.CourseDuration;
			cto.EnrollmentCount = courses.EnrollmentCount;
			cto.InstructorId = courses.InstructorId;
            cto.ImageUrl = courses.ImageUrl;
			cto.Category = courses.Category;
            
			ViewData["InstructorData"] = instructors;
            return View(cto);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CourseEditDto courses)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingCourse = await _context.Courses.FindAsync(courses.Id);
					if (courses.CourseImage != null && courses.CourseImage.Length > 0 && courses.ImageUrl != existingCourse.ImageUrl)
					{
						// Dosya yükleme işlemi
						string fileName = Guid.NewGuid().ToString() + Path.GetExtension(courses.CourseImage.FileName);
						string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);

						using (var stream = new FileStream(filePath, FileMode.Create))
						{
							courses.CourseImage.CopyTo(stream);
						}

						// ImageUrl'i ayarla
						courses.ImageUrl =  fileName;
						existingCourse.ImageUrl = courses.ImageUrl;
					}
					if (existingCourse == null)
                    {
                        return NotFound();
                    }

                    // Map data from CourseDTO to existingCourse
                    existingCourse.Title = courses.Title;
                    existingCourse.Description = courses.Description;
                    existingCourse.CourseDuration = courses.CourseDuration;
                    existingCourse.EnrollmentCount = courses.EnrollmentCount;
					existingCourse.Category = courses.Category;
                    if (User.IsInRole("Admin"))
                    {
                        existingCourse.InstructorId = courses.InstructorId;
                    }
                    _context.Update(existingCourse);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CoursesExists(courses.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("CourseList","Users");
            }
            ViewData["InstructorId"] = new SelectList(_context.Users, "Id", "Id", courses.InstructorId);
            return View(courses);
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Courses == null)
            {
                return NotFound();
            }

            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (courses == null)
            {
                return NotFound();
            }

            return View(courses);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Courses == null)
            {
                return Problem("Entity set 'LearningAppIdentityDbContext.Courses'  is null.");
            }
            var courses = await _context.Courses.FindAsync(id);
            if (courses != null)
            {
                _context.Courses.Remove(courses);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction("CourseList", "Users");
        }

        private bool CoursesExists(int id)
        {
          return (_context.Courses?.Any(e => e.Id == id)).GetValueOrDefault();
        }
		[HttpPost]
		public async Task<IActionResult> Enroll(int courseId)
		{
			var course = await _context.Courses.FindAsync(courseId);
			var userId = _context.Users.SingleOrDefault(u => u.UserName == User.Identity.Name).Id;
			var courseID = _context.Courses.SingleOrDefault(c => c.Id == courseId).Id;

			var isEnrolled = await _context.Enrollments
			.AnyAsync(e => e.CourseId == courseId && e.UserId == userId);

			if (isEnrolled)
			{
                // Handle the case where the user is already enrolled
                // You can display a message or redirect to a specific page
                return Redirect("~/Identity/Account/Manage/MyCourses");
			}

			var enrollment = new Enrollments
			{
				CourseId = courseID,
				UserId = userId,
				EnrollmentDate = DateTime.Now
			};


			_context.Enrollments.Add(enrollment);
			course.EnrollmentCount++;
			await _context.SaveChangesAsync();
			return Redirect("~/Identity/Account/Manage/MyCourses");

		}
	}
}
