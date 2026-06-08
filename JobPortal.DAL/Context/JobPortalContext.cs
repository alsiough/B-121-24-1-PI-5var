using System.Data.Entity;
using JobPortal.DAL.Entities;

namespace JobPortal.DAL.Context
{
    public class JobPortalContext : DbContext
    {
        public JobPortalContext() : base("name=JobPortalDb") { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Vacancy> Vacancies { get; set; }
        public DbSet<Resume> Resumes { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
    }
}