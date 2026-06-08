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
    public class VacancyService : IVacancyService
    {
        private readonly IUnitOfWork _database;
        public VacancyService(IUnitOfWork uow) { _database = uow; }

        public void CreateVacancy(VacancyDto dto)
        {
            if (string.IsNullOrEmpty(dto.Title)) throw new ValidationException("Назва обов'язкова", "Title");
            var user = _database.Users.Get(dto.EmployerId);
            if (user == null || user.RoleId != RoleIds.Employer) throw new AccessDeniedException("Тільки роботодавці створюють вакансії");

            _database.Vacancies.Create(new Vacancy
            {
                Title = dto.Title,
                Description = dto.Description,
                Requirements = dto.Requirements,
                Salary = dto.Salary,
                EmployerId = dto.EmployerId,
                CreatedAt = DateTime.UtcNow
            });
            _database.Save();
        }

        public IEnumerable<VacancyDto> GetVacancies(string search, string sortBy, decimal? minSalary)
        {
            var items = _database.Vacancies.GetAll();
            if (!string.IsNullOrEmpty(search))
                items = items.Where(v => v.Title.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || v.Description.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            if (minSalary.HasValue)
                items = items.Where(v => v.Salary >= minSalary.Value);

            switch (sortBy)
            {
                case "salary_desc": items = items.OrderByDescending(v => v.Salary); break;
                case "salary_asc": items = items.OrderBy(v => v.Salary); break;
                default: items = items.OrderByDescending(v => v.CreatedAt); break;
            }
            return items.Select(v => new VacancyDto { Id = v.Id, Title = v.Title, Description = v.Description, Requirements = v.Requirements, Salary = v.Salary, EmployerId = v.EmployerId, CreatedAt = v.CreatedAt }).ToList();
        }

        public IEnumerable<VacancyDto> GetVacanciesByResumeSkills(int resumeId)
        {
            var resume = _database.Resumes.Get(resumeId);
            if (resume == null) throw new NotFoundException("Резюме не знайдено");
            var tags = resume.Skills.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return _database.Vacancies.Find(v => tags.Any(t => v.Requirements.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0))
                .Select(v => new VacancyDto { Id = v.Id, Title = v.Title, Requirements = v.Requirements, Salary = v.Salary }).ToList();
        }

        public void UpdateVacancy(VacancyDto dto)
        {
            if (dto.Id <= 0) throw new ValidationException("Невірний ідентифікатор", "Id");
            if (string.IsNullOrEmpty(dto.Title)) throw new ValidationException("Назва обов'язкова", "Title");

            var vacancy = _database.Vacancies.Get(dto.Id);
            if (vacancy == null) throw new NotFoundException("Вакансію не знайдено");

            var user = _database.Users.Get(dto.EmployerId);
            if (user == null || user.RoleId != RoleIds.Employer)
                throw new AccessDeniedException("Тільки роботодавці редагують вакансії");
            if (vacancy.EmployerId != dto.EmployerId)
                throw new AccessDeniedException("Можна редагувати лише власні вакансії");

            vacancy.Title = dto.Title;
            vacancy.Description = dto.Description;
            vacancy.Requirements = dto.Requirements;
            vacancy.Salary = dto.Salary;

            _database.Vacancies.Update(vacancy);
            _database.Save();
        }

        public void DeleteVacancy(int id, int requesterId)
        {
            var vacancy = _database.Vacancies.Get(id);
            if (vacancy == null) throw new NotFoundException("Вакансію не знайдено");

            var user = _database.Users.Get(requesterId);
            if (user == null || (user.RoleId != RoleIds.Employer && user.RoleId != RoleIds.Admin))
                throw new AccessDeniedException("Недостатньо прав для видалення");
            if (user.RoleId == RoleIds.Employer && vacancy.EmployerId != requesterId)
                throw new AccessDeniedException("Можна видаляти лише власні вакансії");

            _database.Vacancies.Delete(id);
            _database.Save();
        }
    }
}