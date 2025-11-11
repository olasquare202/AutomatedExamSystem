using AutomatedExamSystem.Data;
using AutomatedExamSystem.IRepository;
using AutomatedExamSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AutomatedExamSystem.Repository
{
    public interface ICandidateRepository : IGenericRepository<Candidate>
    {
        Task<Candidate> GetByEmailAsync(string email);
        Task<string> GenerateNextCandidateCodeAsync();
        Task<IEnumerable<(Candidate Candidate, int Score)>> GetAllWithScoresAsync();
        Task<IEnumerable<Candidate>> GetAllWithAttemptsAsync();
        Task<IEnumerable<Candidate>> GetByLevelAsync(string level); // ✅ new
        Task Save();
    }

    public class CandidateRepository : ICandidateRepository
    {
        private readonly ExamAppDbContext _db;

        public CandidateRepository(ExamAppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Candidate entity)
        {
            await _db.Candidates.AddAsync(entity);
        }

        public void Update(Candidate entity)
        {
            _db.Candidates.Update(entity);
        }

        public void Remove(Candidate entity)
        {
            _db.Candidates.Remove(entity);
        }

        public async Task<Candidate> GetAsync(int id)
        {
            return await _db.Candidates.FindAsync(id);
        }

        public async Task<IEnumerable<Candidate>> GetAllWithAttemptsAsync()
        {
            return await _db.Candidates
                .Include(c => c.Attempts)
                .OrderByDescending(c => c.Attempts.Score)
                .ToListAsync();
        }

        public async Task<IEnumerable<Candidate>> GetAllAsync()
        {
            return await _db.Candidates.ToListAsync();
        }

        public async Task<IEnumerable<(Candidate Candidate, int Score)>> GetAllWithScoresAsync()
        {
            var data = await _db.Candidates
                .Select(c => new
                {
                    Candidate = c,
                    Score = _db.CandidateAttempts
                        .Where(a => a.CandidateId == c.Id)
                        .OrderByDescending(a => a.Score)
                        .Select(a => a.Score)
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.Score)
                .ToListAsync();

            return data.Select(x => (x.Candidate, x.Score));
        }

        public async Task<Candidate> GetByEmailAsync(string email)
        {
            return await _db.Candidates.FirstOrDefaultAsync(c => c.Email == email);
        }

        // ✅ NEW: Get all candidates by Level (e.g., "100L" or "200L")
        public async Task<IEnumerable<Candidate>> GetByLevelAsync(string level)
        {
            return await _db.Candidates
                .Where(c => c.Level == level)
                .Include(c => c.Attempts)
                .OrderByDescending(c => c.Attempts.Score)
                .ToListAsync();
        }

        public async Task<string> GenerateNextCandidateCodeAsync()
        {
            var year = DateTime.UtcNow.Year;
            var last = await _db.Candidates
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();

            var next = (last == null) ? 1 : last.Id + 1;
            return $"PVM-{year}-{next:000}";
        }

        public async Task Save()
        {
            await _db.SaveChangesAsync();
        }
    }
}
