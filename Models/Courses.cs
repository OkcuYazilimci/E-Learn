using Microsoft.AspNetCore.Identity;

namespace elearningapp.Models
{
    public class Courses
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string InstructorId { get; set; }
        public string Category { get; set; }
        public int EnrollmentCount { get; set; }
        public string? ImageUrl { get; set; }
        public int CourseDuration { get; set; }

        public IdentityUser Instructor { get; set; }

		public ICollection<Assignments> Assignments { get; set; } = new List<Assignments>();

	}
}
