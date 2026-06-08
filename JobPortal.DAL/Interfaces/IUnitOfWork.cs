using System;
using JobPortal.DAL.Entities;

namespace JobPortal.DAL.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Role> Roles { get; }
        IRepository<Vacancy> Vacancies { get; }
        IRepository<Resume> Resumes { get; }
        IRepository<JobApplication> JobApplications { get; }
        void Save();
    }
}