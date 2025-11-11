using AutomatedExamSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AutomatedExamSystem.Data
{
    public class ExamAppDbContext : DbContext
    {
        public ExamAppDbContext(DbContextOptions<ExamAppDbContext> options) : base(options) 
        {
            
        }
        public DbSet<Candidate> Candidates {  get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<CandidateAnswer> CandidateAnswers { get; set; }
        public DbSet<ExamSession> ExamSessions { get; set; }
        public DbSet<CandidateAttempt> CandidateAttempts { get; set; }
        
    }
}
