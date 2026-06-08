using System;
using JobPortal.DAL.Context;
using JobPortal.DAL.Entities;
using JobPortal.DAL.Interfaces;

namespace JobPortal.DAL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly JobPortalContext _context = new JobPortalContext();
        private Repository<User> _userRepository;
        private Repository<Role> _roleRepository;
        private Repository<Vacancy> _vacancyRepository;
        private Repository<Resume> _resumeRepository;
        private Repository<JobApplication> _jobApplicationRepository;

        public IRepository<User> Users
        {
            get
            {
                if (_userRepository == null) _userRepository = new Repository<User>(_context);
                return _userRepository;
            }
        }

        public IRepository<Role> Roles
        {
            get
            {
                if (_roleRepository == null) _roleRepository = new Repository<Role>(_context);
                return _roleRepository;
            }
        }

        public IRepository<Vacancy> Vacancies
        {
            get
            {
                if (_vacancyRepository == null) _vacancyRepository = new Repository<Vacancy>(_context);
                return _vacancyRepository;
            }
        }

        public IRepository<Resume> Resumes
        {
            get
            {
                if (_resumeRepository == null) _resumeRepository = new Repository<Resume>(_context);
                return _resumeRepository;
            }
        }

        public IRepository<JobApplication> JobApplications
        {
            get
            {
                if (_jobApplicationRepository == null) _jobApplicationRepository = new Repository<JobApplication>(_context);
                return _jobApplicationRepository;
            }
        }

        public void Save() => _context.SaveChanges();

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing) _context.Dispose();
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}