using AutomatedExamSystem.Data;
using AutomatedExamSystem.IRepository;
using AutomatedExamSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AutomatedExamSystem.Repository
{
    public interface IQuestionRepository : IGenericRepository<Question>
    {
        Task<IEnumerable<Question>> GetBySectionAsync(SectionType section, int take);

        // ✅ New method to fetch questions by Section + Level
        Task<IEnumerable<Question>> GetBySectionAndLevelAsync(SectionType section, string level, int take);

        // ✅ Optional: fetch all questions for a level
        Task<IEnumerable<Question>> GetByLevelAsync(string level);
    }

    public class QuestionRepository : IQuestionRepository
    {
        private readonly ExamAppDbContext _db;
        public QuestionRepository(ExamAppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Question entity)
        {
            await _db.Questions.AddAsync(entity);
            await _db.SaveChangesAsync();
        }

        public void Update(Question entity)
        {
            _db.Questions.Update(entity);
            _db.SaveChanges();
        }

        public void Remove(Question entity)
        {
            _db.Questions.Remove(entity);
            _db.SaveChanges();
        }

        public async Task<Question> GetAsync(int id) => await _db.Questions.FindAsync(id);

        public async Task<IEnumerable<Question>> GetAllAsync() => await _db.Questions.ToListAsync();

        public async Task<IEnumerable<Question>> GetBySectionAsync(SectionType section, int take)
        {
            return await _db.Questions
                .Where(q => q.Section == section)
                .Take(take)
                .ToListAsync();
        }

        // ✅ New: Get questions by Section and Level
        public async Task<IEnumerable<Question>> GetBySectionAndLevelAsync(SectionType section, string level, int take)
        {
            return await _db.Questions
                .Where(q => q.Section == section && q.Level == level)
                .Take(take)
                .ToListAsync();
        }

        // ✅ New: Get all questions by Level (optional, useful for exam setup)
        public async Task<IEnumerable<Question>> GetByLevelAsync(string level)
        {
            return await _db.Questions
                .Where(q => q.Level == level)
                .ToListAsync();
        }

        public async Task Save()
        {
            await _db.SaveChangesAsync();
        }
    }
}
