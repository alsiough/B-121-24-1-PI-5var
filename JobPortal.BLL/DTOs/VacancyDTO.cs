using System;

namespace JobPortal.BLL.DTOs
{
    public class VacancyDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
        public decimal Salary { get; set; }
        public int EmployerId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}