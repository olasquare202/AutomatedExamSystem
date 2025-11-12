using AutomatedExamSystem.Data;
using AutomatedExamSystem.Models;
using AutomatedExamSystem.Repository;
using AutomatedExamSystem.Services.AutomatedExamSystem.Services;
using Microsoft.EntityFrameworkCore;

namespace AutomatedExamSystem.Services
{
    public class ExamService : IExamService
    {
        private readonly ExamAppDbContext _db;
        private readonly ILogger<SmtpEmailSender> _logger;
        private readonly IQuestionRepository _questionRepo;

        public ExamService(ExamAppDbContext db, ILogger<SmtpEmailSender> logger, IQuestionRepository questionRepo)
        {
            _db = db;
            _logger = logger;
            _questionRepo = questionRepo;
        }

        public async Task<bool> StartAttemptAsync(Candidate candidate)
        {
            try
            {
                if (candidate.HasTakenExam)
                    throw new InvalidOperationException("Candidate already attempted the test.");

                bool hasAttempt = await _db.CandidateAttempts.AnyAsync(a => a.CandidateId == candidate.Id);
                if (hasAttempt)
                {
                    _logger.LogInformation($"Candidate {candidate.Email} has already taken the exam.");
                    return false;
                }

                // ✅ Create new attempt
                var attempt = new CandidateAttempt
                {
                    CandidateId = candidate.Id,
                    StartTimeUtc = DateTime.UtcNow
                };

                await _db.CandidateAttempts.AddAsync(attempt);

                // ✅ Mark candidate as started
                candidate.HasTakenExam = true;
                _db.Candidates.Update(candidate);

                // ✅ Fetch questions based on candidate’s level
                var sections = Enum.GetValues(typeof(SectionType)).Cast<SectionType>();
                var allQuestions = new List<Question>();

                foreach (var section in sections)
                {
                    var sectionQuestions = await _questionRepo.GetBySectionAndLevelAsync(section, candidate.Level, 10);
                    allQuestions.AddRange(sectionQuestions);
                }

                await _db.SaveChangesAsync();
                _logger.LogInformation($"Exam attempt started for {candidate.Email} (Level: {candidate.Level})");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while starting exam attempt");
                return false;
            }
        }
        public async Task<CandidateAttempt> SubmitAttemptAsync(int candidateId, Dictionary<int, char> answers)
        {
            var attempt = await _db.CandidateAttempts
                .Where(a => a.CandidateId == candidateId && a.EndTimeUtc == null)
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync();

            if (attempt == null)
                throw new InvalidOperationException("No active attempt found.");

            int totalScore = 0;
            var sectionScores = new Dictionary<SectionType, int>();
            var sectionQuestionCounts = new Dictionary<SectionType, int>();

            foreach (var kv in answers)
            {
                var q = await _db.Questions.FindAsync(kv.Key);
                if (q == null)
                    continue;

                // ✅ Section is required — no need for null checks
                var section = q.Section;

                // ✅ Initialize dictionary entries if missing
                if (!sectionQuestionCounts.ContainsKey(section))
                    sectionQuestionCounts[section] = 0;
                if (!sectionScores.ContainsKey(section))
                    sectionScores[section] = 0;

                sectionQuestionCounts[section]++;

                bool isCorrect = kv.Value
                    .ToString()
                    .Equals(q.CorrectOption, StringComparison.OrdinalIgnoreCase);

                if (isCorrect)
                {
                    totalScore += 2;
                    sectionScores[section] += 2;
                }

                var candidateAnswer = new CandidateAnswer
                {
                    CandidateAttemptId = attempt.Id,
                    QuestionId = q.Id,
                    SelectedOption = kv.Value.ToString(),
                    CorrectOption = q.CorrectOption.ToString(),
                    IsCorrect = isCorrect
                };

                await _db.CandidateAnswers.AddAsync(candidateAnswer);
            }

            // ✅ Build a readable section score breakdown
            var breakdown = sectionQuestionCounts.ToDictionary(
                s => s.Key.ToString(),
                s =>
                {
                    var score = sectionScores.ContainsKey(s.Key) ? sectionScores[s.Key] : 0;
                    var total = s.Value * 2;
                    return $"{score}/{total}";
                });

            attempt.Score = totalScore;
            attempt.SectionBreakdownJson = System.Text.Json.JsonSerializer.Serialize(breakdown);

            // ✅ Store submission time (UTC + WAT)
            attempt.EndTimeUtc = DateTime.UtcNow;
            var watZone = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time");
            attempt.SubmittedAtWAT = TimeZoneInfo.ConvertTimeFromUtc(attempt.EndTimeUtc.Value, watZone);

            _db.CandidateAttempts.Update(attempt);
            await _db.SaveChangesAsync();

            return attempt;
        }
        public Task<bool> IsRegistrationOpenAsync(IConfiguration config)
        {
            var testDate = DateTime.Parse(config["TestSettings:TestDate"]);
            var openFrom = testDate.AddDays(-int.Parse(config["TestSettings:RegistrationOpenDaysBefore"]));
            var now = DateTime.UtcNow.Date;
            return Task.FromResult(now >= openFrom.Date && now <= testDate.Date);
        }

        public Task<bool> IsTestWindowOpenAsync(IConfiguration config)
        {
            var testDate = DateTime.Parse(config["TestSettings:TestDate"]);
            var startTime = TimeSpan.Parse(config["TestSettings:TestStartTime"]);
            var endTime = TimeSpan.Parse(config["TestSettings:TestEndTime"]);

            var nowUtc = DateTime.UtcNow;
            var testStart = DateTime.SpecifyKind(new DateTime(testDate.Year, testDate.Month, testDate.Day).Add(startTime), DateTimeKind.Local).ToUniversalTime();
            var testEnd = DateTime.SpecifyKind(new DateTime(testDate.Year, testDate.Month, testDate.Day).Add(endTime), DateTimeKind.Local).ToUniversalTime();

            return Task.FromResult(nowUtc >= testStart && nowUtc <= testEnd);
        }





    }
}