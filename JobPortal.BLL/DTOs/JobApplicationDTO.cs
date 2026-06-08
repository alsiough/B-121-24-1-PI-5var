using System;

namespace JobPortal.BLL.DTOs
{
    public class JobApplicationDto
    {
        public int Id { get; set; }
        public int VacancyId { get; set; }
        public string VacancyTitle { get; set; }
        public int ResumeId { get; set; }
        public string ResumeTitle { get; set; }
        public string Status { get; set; }
        public DateTime ActionDate { get; set; }
    }
}