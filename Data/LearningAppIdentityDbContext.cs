using elearningapp.Models;
using LearningApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace elearningapp.Data
{
    public class LearningAppIdentityDbContext : IdentityDbContext
    {
        public LearningAppIdentityDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
			base.OnModelCreating(builder);

        }
		public DbSet<Courses> Courses { get; set; }
		public DbSet<Enrollments> Enrollments { get; set; }
		public DbSet<Assignments> Assignments { get; set; }

        public DbSet<CourseProgress> CourseProgresses { get; set; }
	}
}
