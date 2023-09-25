using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace elearningapp.Models
{
    public class Enrollments
    {
        public int Id { get; set; }
        public string UserId { get; set; }
		public int CourseId { get; set; }
        public DateTime EnrollmentDate { get; set; }

		public IdentityUser User { get; set; }
	}
}
