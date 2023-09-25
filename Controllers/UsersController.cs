using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using elearningapp.Data;
using LearningApp.Controllers;
using Microsoft.AspNetCore.Mvc.Rendering;
using LearningApp.Models;
using elearningapp.Models;
using System.Security.Claims;

namespace elearningapp.Controllers
{

    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly LearningAppIdentityDbContext _idcontext;

        public UsersController(UserManager<IdentityUser> userManager, LearningAppIdentityDbContext contextid, RoleManager<IdentityRole> roleManager)
        {
            _idcontext = contextid;
            _userManager = userManager;
            _roleManager = roleManager;
           
        }
        //GET : Dashboard
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Dashboard()
        {
            var courses = (from x in _idcontext.Courses
                          select x).ToList();

            var courseCount = courses.Count();

            var instructorCount = (from user in _idcontext.Users
                                   join userRole in _idcontext.UserRoles on user.Id equals userRole.UserId
                                   join role in _idcontext.Roles on userRole.RoleId equals role.Id
                                   where role.Name == "Instructor"
                                   select user).ToList().Count();

            var studentCount = (from user in _idcontext.Users
                                 join userRole in _idcontext.UserRoles on user.Id equals userRole.UserId
                                 join role in _idcontext.Roles on userRole.RoleId equals role.Id
                                 where role.Name == "Student"
                                 select user).ToList().Count();

            var assignmentCount = (from assign in _idcontext.Assignments
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
        [Authorize(Roles = "Admin")]
        // GET: Users
        public async Task<IActionResult> Index()
        {

            var users = await _userManager.Users.ToListAsync();
            List<UserWasRole> list = new List<UserWasRole>();


            /*var roles = await _userManager.GetRolesAsync(user.Identity.GetUserId());  */
            foreach (IdentityUser user in users)
            {
                UserWasRole UserwithRole = new UserWasRole();
                UserwithRole.Roles = new List<string>();
                UserwithRole.Username = user.UserName;
                UserwithRole.Id = user.Id;
                //UserwithRole.Roles = _userManager.GetRolesAsync(user.Id.ToString());     
                var sqlRoles = (from x in _idcontext.UserRoles
                                where x.UserId == user.Id
                                select x).ToList();
                //UserwithRole.Roles = sqlRoles.ToList<string>();
                foreach (var role in sqlRoles)
                {

                    var roleNames = (from x in _idcontext.Roles
                                     where x.Id == role.RoleId
                                     select x).ToList();
                    foreach (var name in roleNames)
                    {
                        UserwithRole.Roles.Add(name.Name);
                    }
                }
                list.Add(UserwithRole);
            }

            return View(list);

        }
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            UserCreateModel model = new UserCreateModel();
			ViewBag.RoleNames = (from x in _idcontext.Roles select x.Name).ToList();
			return View(model);
        }
        [Authorize(Roles = "Admin")]
        // POST: Users/Create
        [HttpPost]
        public async Task<IActionResult> Create(UserCreateModel user , [Bind("RoleName")] string RoleName)
        {
            if (ModelState.IsValid)
            {
			
				var userasd = new IdentityUser();
				userasd.UserName = user._IdentityUser.UserName;
                userasd.Email = user._IdentityUser.Email;

				string userPWD = user._IdentityUser.PasswordHash;

				IdentityResult chkUser = await _userManager.CreateAsync(userasd, userPWD);

				//Add default User to Role Admin
				if (chkUser.Succeeded)
				{
					var result1 = await _userManager.AddToRoleAsync(userasd, RoleName);
					if (result1.Succeeded)
					{
						return RedirectToAction("", "Users");
					}

					foreach (var error in result1.Errors)
					{
						ModelState.AddModelError(string.Empty, error.Description);
					}
				}

				
            }

            return View(user);
        }
        [Authorize(Roles = "Admin")]
        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        [Authorize(Roles = "Admin")]
        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            UserWasRole UserwithRole = new UserWasRole();

            UserwithRole.Username = user.UserName;
            UserwithRole.Id = user.Id;

            var roleNames = (from x in _idcontext.UserRoles
                             join y in _idcontext.Roles on x.RoleId equals y.Id
                             where x.UserId == user.Id
                             select y.Name);

            UserwithRole.RoleName = roleNames.FirstOrDefault();
            UserwithRole.Email = user.Email;


			var allroles = (from x in _idcontext.Roles
                            select x).
                            Select(y => new SelectListItem
							{
								Value = y.Id,
								Text = y.Name
							})
			                .ToList();
            

			ViewData["allroles"] = allroles;
			if (user == null)
            {
                return NotFound();
            }

            return View(UserwithRole);
        }
        [Authorize(Roles = "Admin")]
        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,UserName,Email")] IdentityUser user, [Bind("RoleName")] string RoleName)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound();
                }
               
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;

                var userRole = await _userManager.GetRolesAsync(existingUser);
                await _userManager.RemoveFromRoleAsync(existingUser, userRole[0]);
                await _userManager.AddToRoleAsync(existingUser, RoleName);

                var result = await _userManager.UpdateAsync(existingUser);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(user);
        }
        [Authorize(Roles = "Admin")]
        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        [Authorize(Roles = "Admin")]
        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("", "Users");
                }
            }

            return NotFound();
        }
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> CourseList()
        {

            List<CourseDTO> courses = new List<CourseDTO>();
			string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user_info = await _userManager.FindByIdAsync(currentUserId);
            var user_role = await _userManager.GetRolesAsync(user_info);
            var my_courses = _idcontext.Courses.ToList();
			if (user_role.Contains("Instructor"))
            {
                my_courses = (from course in _idcontext.Courses
                             where course.InstructorId == user_info.Id
                             select course).ToList();
            }
			foreach (var item in my_courses)
			{
				var kapali = (from user in _idcontext.Users
							  where user.Id == item.InstructorId
							  select user.UserName
					).ToList();

				var courseDTO = new CourseDTO
				{
					Id = item.Id,
					// Populate other properties as needed
					InstructorName = kapali.FirstOrDefault(),
					Title = item.Title,
					Description = item.Description,
					Category = item.Category,
					InstructorId = item.InstructorId,
					EnrollmentCount = item.EnrollmentCount,
					CourseDuration = item.CourseDuration,
					ImageUrl = item.ImageUrl,

				};
				courses.Add(courseDTO);


			}
			return _idcontext.Courses != null ?
                          View(courses) :
                          Problem("Entity set 'LearningAppDbContext.Courses'  is null.");
        }

        [HttpGet]
        public async Task<IActionResult> AssignmentList([FromQuery] int CourseId)
        {
            List<Assignments> assignments = new List<Assignments>((from x in _idcontext.Assignments
                                                                   where x.CourseId == CourseId
																   select x
                        ).ToList());

            ViewData["CourseName"] = (from x in _idcontext.Courses
                                     where x.Id == CourseId
									  select x.Title).FirstOrDefault();
            ViewData["CourseId"] = CourseId;
            return View(assignments);
        }

    }
}

