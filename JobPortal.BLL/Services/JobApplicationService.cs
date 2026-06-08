using System;
using System.Collections.Generic;
using System.Linq;
using JobPortal.BLL.DTOs;
using JobPortal.BLL.Infrastructure;
using JobPortal.BLL.Interfaces;
using JobPortal.DAL.Entities;
using JobPortal.DAL.Interfaces;

namespace JobPortal.BLL.Services
{
    public class JobApplicationService : IJobApplicationService
    {
        private readonly IUnitOfWork _database;
        public JobApplicationService(IUnitOfWork uow) { _database = uow; }

        public void ApplyForVacancy(int resumeId, int vacancyId, string initiatorRole)
        {
            var resume = _database.Resumes.Get(resumeId);
            var vacancy = _database.Vacancies.Get(vacancyId);
            if (resume == null || vacancy == null) throw new NotFoundException("Резюме або вакансію не знайдено");

            string status = initiatorRole == "Employee" ? "Applied" : "Offered";
            _database.JobApplications.Create(new JobApplication
            {
                ResumeId = resumeId,
                VacancyId = vacancyId,
                Status = status,
                ActionDate = DateTime.UtcNow
            });
            _database.Save();
        }

        public IEnumerable<JobApplicationDto> GetApplicationsForVacancy(int vacancyId)
        {
            return _database.JobApplications.FindWithIncludes(a => a.VacancyId == vacancyId, a => a.Vacancy, a => a.Resume)
                .Select(a => new JobApplicationDto { Id = a.Id, VacancyId = a.VacancyId, VacancyTitle = a.Vacancy.Title, ResumeId = a.ResumeId, ResumeTitle = a.Resume.Title, Status = a.Status, ActionDate = a.ActionDate }).ToList();
        }

        public IEnumerable<JobApplicationDto> GetApplicationsForResume(int resumeId)
        {
            return _database.JobApplications.FindWithIncludes(a => a.ResumeId == resumeId, a => a.Vacancy, a => a.Resume)
                .Select(a => new JobApplicationDto { Id = a.Id, VacancyId = a.VacancyId, VacancyTitle = a.Vacancy.Title, ResumeId = a.ResumeId, ResumeTitle = a.Resume.Title, Status = a.Status, ActionDate = a.ActionDate }).ToList();
        }
    }
}