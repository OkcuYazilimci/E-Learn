// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Web.Helpers;
using elearningapp.Data;
using elearningapp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace elearningapp.Areas.Identity.Pages.Account.Manage
{
    public class MyCoursesData : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly LearningAppIdentityDbContext _context;
        private readonly ILogger<MyCoursesData> _logger;
        public InputModel Input { get; set; }
        public MyCoursesData(
            UserManager<IdentityUser> userManager,
            ILogger<MyCoursesData> logger, LearningAppIdentityDbContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public List<Courses> CourseList { get; set; }
        }
        private async Task LoadAsync(IdentityUser user)
        {
            //course title -> enroll olunan
            var courselist = (from course in _context.Courses
                         where (from enrollment in _context.Enrollments
                                where enrollment.UserId == user.Id
                                select enrollment.CourseId)
                                .Contains(course.Id)
                         select course).ToList();

            Input = new InputModel
            {
                CourseList = courselist,
            };

        }
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }
    }
}
