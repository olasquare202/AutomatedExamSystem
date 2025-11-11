using System.ComponentModel.DataAnnotations;

namespace AutomatedExamSystem.Models
{
    public class Candidate
    {
        public int Id { get; set; }
        public string CandidateCode { get; set; }  // e.g. PVM-2025-00
        public string FullName { get; set; } 
        public string Email { get; set; } 
        public string Institution { get; set; } 
        public string Level { get; set; } 
        public string CourseOfStudy { get; set; } 
        public string PhoneNumber { get; set; } 

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public bool HasTakenExam { get; set; } = false;

        public CandidateAttempt Attempts { get; set; }
    
    }   
}
