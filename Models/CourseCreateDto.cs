namespace LearningApp.Models
{
	public class CourseCreateDto
	{
		public string Title { get; set; }
		public string? Description { get; set; }
		public string InstructorId { get; set; }
		public string Category { get; set; }
		public int EnrollmentCount { get; set; }
		public string? ImageUrl { get; set; }
		public int CourseDuration { get; set; }

		public IFormFile CourseImage { get; set; }
	}
}
