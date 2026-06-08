using System.Collections.Generic;
using JobPortal.BLL.DTOs;

namespace JobPortal.BLL.Interfaces
{
    public interface IJobApplicationService
    {
        void ApplyForVacancy(int resumeId, int vacancyId, string initiatorRole);
        IEnumerable<JobApplicationDto> GetApplicationsForVacancy(int vacancyId);
        IEnumerable<JobApplicationDto> GetApplicationsForResume(int resumeId);
    }
}