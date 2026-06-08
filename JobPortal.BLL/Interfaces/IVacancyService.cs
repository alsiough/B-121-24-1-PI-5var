using System.Collections.Generic;
using JobPortal.BLL.DTOs;

namespace JobPortal.BLL.Interfaces
{
    public interface IVacancyService
    {
        void CreateVacancy(VacancyDto vacancyDto);
        IEnumerable<VacancyDto> GetVacancies(string search, string sortBy, decimal? minSalary);
        IEnumerable<VacancyDto> GetVacanciesByResumeSkills(int resumeId);
        void UpdateVacancy(VacancyDto vacancyDto);
        void DeleteVacancy(int id, int requesterId);
    }
}
