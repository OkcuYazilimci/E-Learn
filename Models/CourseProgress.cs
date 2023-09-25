using elearningapp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Operations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningApp.Models
{
    public class CourseProgress
    {
        public int Id { get; set; }
        [ForeignKey(nameof(Id))]
        public int AssignmentsId { get; set; }
        [ForeignKey(nameof(Id))]
        public string UsersId { get; set; }

        public IdentityUser Users { get; set; }
        public bool IsCompleted { get; set; }
        public Assignments Assignments { get; set; }

    }
}
