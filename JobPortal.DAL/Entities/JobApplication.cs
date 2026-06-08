using System;

namespace JobPortal.DAL.Entities
{
    public class JobApplication
    {
        public int Id { get; set; }
        public int ResumeId { get; set; }
        public virtual Resume Resume { get; set; }
        public int VacancyId { get; set; }
        public virtual Vacancy Vacancy { get; set; }
        public string Status { get; set; } // "Applied", "Offered", "Rejected", "Accepted"
        public DateTime ActionDate { get; set; }
    }
}