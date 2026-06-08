using System;

namespace JobPortal.BLL.DTOs
{
    public class ResumeDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Skills { get; set; }
        public string Experience { get; set; }
        public string Description { get; set; }
        public int EmployeeId { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}