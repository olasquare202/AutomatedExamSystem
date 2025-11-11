namespace AutomatedExamSystem.Models
{
    public class ExamSession
    {
        public int Id { get; set; }
        
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double Score { get; set; }
        public int CandidateIdRef { get; set; }
        public Candidate Candidate { get; set; }
    }
}
