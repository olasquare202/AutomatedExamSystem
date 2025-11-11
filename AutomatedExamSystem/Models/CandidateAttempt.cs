using AutomatedExamSystem.Models;

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
        public string? SectionBreakdownJson { get; set; } // ✅ “English: 8, Logic: 7”
        public Candidate Candidate { get; set; }
    }
}


