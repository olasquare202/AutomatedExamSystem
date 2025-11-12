using System;
using System.ComponentModel.DataAnnotations;

namespace AutomatedExamSystem.Models
{
    public class CandidateAttempt
    {
        public int Id { get; set; }

        public int CandidateId { get; set; }

        public DateTime StartTimeUtc { get; set; }

        public DateTime? EndTimeUtc { get; set; }

        public int Score { get; set; }

        public int TotalQuestions { get; set; }

        public int CorrectAnswers { get; set; }

        public string? SectionBreakdownJson { get; set; }

        // ✅ Local (WAT) submission timestamp
        [Display(Name = "Submitted At (WAT)")]
        public DateTime? SubmittedAtWAT { get; set; }

        // Navigation property
        public Candidate Candidate { get; set; }
    }
}