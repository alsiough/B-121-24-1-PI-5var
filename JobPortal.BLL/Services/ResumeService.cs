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
    public class ResumeService : IResumeService
    {
        private readonly IUnitOfWork _database;
        public ResumeService(IUnitOfWork uow) { _database = uow; }

        public void CreateResume(ResumeDto dto)
        {
            if (string.IsNullOrEmpty(dto.Title)) throw new ValidationException("Заголовок обов'язковий", "Title");
            var user = _database.Users.Get(dto.EmployeeId);
            if (user == null || user.RoleId != RoleIds.Employee) throw new AccessDeniedException("Тільки здобувачі створюють резюме");

            _database.Resumes.Create(new Resume
            {
                Title = dto.Title,
                Skills = dto.Skills,
                Experience = dto.Experience,
                Description = dto.Description,
                EmployeeId = dto.EmployeeId,
                UpdatedAt = DateTime.UtcNow
            });
            _database.Save();
        }

        public IEnumerable<ResumeDto> GetResumes(string search, string sortBy)
        {
            var items = _database.Resumes.GetAll();
            if (!string.IsNullOrEmpty(search))
                items = items.Where(r => r.Title.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || r.Skills.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);

            switch (sortBy)
            {
                case "title": items = items.OrderBy(r => r.Title); break;
                default: items = items.OrderByDescending(r => r.UpdatedAt); break;
            }
            return items.Select(r => new ResumeDto { Id = r.Id, Title = r.Title, Skills = r.Skills, Experience = r.Experience, Description = r.Description, EmployeeId = r.EmployeeId, UpdatedAt = r.UpdatedAt }).ToList();
        }

        public IEnumerable<ResumeDto> GetResumesByVacancyRequirements(int vacancyId)
        {
            var vacancy = _database.Vacancies.Get(vacancyId);
            if (vacancy == null) throw new NotFoundException("Вакансію не знайдено");
            var tags = vacancy.Requirements.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return _database.Resumes.GetAll()
                .Where(r => tags.Any(t => r.Skills.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0))
                .Select(r => new ResumeDto { Id = r.Id, Title = r.Title, Skills = r.Skills }).ToList();
        }

        public void UpdateResume(ResumeDto dto)
        {
            if (dto.Id <= 0) throw new ValidationException("Невірний ідентифікатор", "Id");
            if (string.IsNullOrEmpty(dto.Title)) throw new ValidationException("Заголовок обов'язковий", "Title");

            var resume = _database.Resumes.Get(dto.Id);
            if (resume == null) throw new NotFoundException("Резюме не знайдено");

            var user = _database.Users.Get(dto.EmployeeId);
            if (user == null || user.RoleId != RoleIds.Employee)
                throw new AccessDeniedException("Тільки здобувачі редагують резюме");
            if (resume.EmployeeId != dto.EmployeeId)
                throw new AccessDeniedException("Можна редагувати лише власне резюме");

            resume.Title = dto.Title;
            resume.Skills = dto.Skills;
            resume.Experience = dto.Experience;
            resume.Description = dto.Description;
            resume.UpdatedAt = DateTime.UtcNow;

            _database.Resumes.Update(resume);
            _database.Save();
        }

        public void DeleteResume(int id, int requesterId)
        {
            var resume = _database.Resumes.Get(id);
            if (resume == null) throw new NotFoundException("Резюме не знайдено");

            var user = _database.Users.Get(requesterId);
            if (user == null || (user.RoleId != RoleIds.Employee && user.RoleId != RoleIds.Admin))
                throw new AccessDeniedException("Недостатньо прав для видалення");
            if (user.RoleId == RoleIds.Employee && resume.EmployeeId != requesterId)
                throw new AccessDeniedException("Можна видаляти лише власне резюме");

            _database.Resumes.Delete(id);
            _database.Save();
        }
    }
}