using elearningapp.Data;
using elearningapp.Models;
using LearningApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace elearningapp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly LearningAppIdentityDbContext _context;

		public HomeController(ILogger<HomeController> logger, LearningAppIdentityDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(int id)
        {
            var courses = (from x in _context.Courses
                           orderby x.EnrollmentCount descending
                           select x).Take(3).ToList();

            var courseCount = (from x in _context.Courses
							   orderby x.EnrollmentCount descending
							   select x).Count();

            var instructorCount = (from user in _context.Users
                                   join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                   join role in _context.Roles on userRole.RoleId equals role.Id
                                   where role.Name == "Instructor"
                                   select user).ToList().Count();

            var studentCount = (from user in _context.Users
                                join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                join role in _context.Roles on userRole.RoleId equals role.Id
                                where role.Name == "Student"
                                select user).ToList().Count();

            var assignmentCount = (from assign in _context.Assignments
                                   select assign).ToList().Count();
            

            var Dashboardmodel = new DashboardModel
            {
                AssignmentCount = assignmentCount,
                StudentCount = studentCount,
                CourseCount = courseCount,
                InstructorCount = instructorCount,
                courses = courses,

            };
            return View(Dashboardmodel);

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}