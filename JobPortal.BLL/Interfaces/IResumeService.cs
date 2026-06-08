using System.Collections.Generic;
using JobPortal.BLL.DTOs;

namespace JobPortal.BLL.Interfaces
{
    public interface IResumeService
    {
        void CreateResume(ResumeDto resumeDto);
        IEnumerable<ResumeDto> GetResumes(string search, string sortBy);
        IEnumerable<ResumeDto> GetResumesByVacancyRequirements(int vacancyId);
        void UpdateResume(ResumeDto resumeDto);
        void DeleteResume(int id, int requesterId);
    }
}
