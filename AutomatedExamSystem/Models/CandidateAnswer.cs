using AutomatedExamSystem.Models;

namespace AutomatedExamSystem.Models
{
    public class CandidateAnswer
    {
        public int Id { get; set; }

        public int CandidateAttemptId { get; set; }
        public CandidateAttempt CandidateAttempt { get; set; } 

        public int QuestionId { get; set; }
        public Question Question { get; set; } 

        public string SelectedOption { get; set; }
        public string CorrectOption { get; set; } // ✅ store correct answer
        public bool IsCorrect { get; set; }     // ✅ easy summary flag
    }
}


