using elearningapp.Models;

namespace LearningApp.Models
{
    public class DashboardModel
    {
        public int InstructorCount { get; set; }
        public int StudentCount { get; set; }
        public int CourseCount { get; set; }
        public int AssignmentCount { get;set;}
        
        public List<Courses> courses { get; set; }
    }
}
