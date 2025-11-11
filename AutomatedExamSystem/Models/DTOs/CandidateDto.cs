using System.ComponentModel.DataAnnotations;

namespace AutomatedExamSystem.Models.DTOs
{
    public class CandidateDto
    {
       
        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public string Institution { get; set; }
        public string Level { get; set; }
        public string CourseOfStudy { get; set; }
        public string PhoneNumber { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public bool HasTakenExam { get; set; } = false;
    }
}
