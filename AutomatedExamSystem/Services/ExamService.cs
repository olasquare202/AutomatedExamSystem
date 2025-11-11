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

                // Store question IDs for tracking (optional if you randomize)
                //foreach (var q in allQuestions)
                //{
                //    await _db.CandidateAnswers.AddAsync(new CandidateAnswer
                //    {
                //        CandidateAttempt = attempt,
                //        QuestionId = q.Id
                //    });
                //}

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

                // ✅ Safely handle nullable Section
                var section = q.Section ?? SectionType.UseOfEnglish;

                // ✅ Ensure dictionary entries exist before incrementing
                if (!sectionQuestionCounts.ContainsKey(section))
                    sectionQuestionCounts[section] = 0;
                if (!sectionScores.ContainsKey(section))
                    sectionScores[section] = 0;

                sectionQuestionCounts[section]++;

                bool isCorrect = kv.Value.ToString()
                    .Equals(q.CorrectOption, StringComparison.OrdinalIgnoreCase);

                if (isCorrect)
                {
                    totalScore += 2;
                    sectionScores[section] += 2;
                }

                var ca = new CandidateAnswer
                {
                    CandidateAttemptId = attempt.Id,
                    QuestionId = q.Id,
                    SelectedOption = kv.Value.ToString(),
                    CorrectOption = q.CorrectOption.ToString(),
                    IsCorrect = isCorrect
                };
                await _db.CandidateAnswers.AddAsync(ca);
            }

            // ✅ Build a readable breakdown safely
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
            attempt.EndTimeUtc = DateTime.UtcNow;

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








//using AutomatedExamSystem.Data;
//using AutomatedExamSystem.Models;
//using Microsoft.EntityFrameworkCore;

//namespace AutomatedExamSystem.Services
//{
//    public class ExamService : IExamService
//    {
//        private readonly ExamAppDbContext _db;
//        private readonly ILogger<SendGridEmailSender> _logger;

//        public ExamService(ExamAppDbContext db, ILogger<SendGridEmailSender> logger)
//        {
//            _db = db;
//            _logger = logger;
//        }


//        public async Task<bool> StartAttemptAsync(Candidate candidate)
//        {
//            // Check if the candidate has an active or completed attempt

//            try
//            {

//                if (candidate.HasTakenExam)
//                    throw new InvalidOperationException("Candidate already attempted the test.");

//                bool hasAttempt = await _db.CandidateAttempts
//                    .AnyAsync(a => a.CandidateId == candidate.Id);

//                if (hasAttempt || candidate.HasTakenExam)
//                {
//                    _logger.LogInformation($"Candidate {candidate.Email} has already taken the exam.");
//                    return false; // ❌ already taken
//                }
//                var attempt = new CandidateAttempt
//                {
//                    CandidateId = candidate.Id,
//                    StartTimeUtc = DateTime.UtcNow
//                };

//                await _db.CandidateAttempts.AddAsync(attempt);
//                candidate.HasTakenExam = true;

//                _db.Candidates.Update(candidate);
//                await _db.SaveChangesAsync();

//                _logger.LogInformation($"Exam attempt started for {candidate.Email}");
//                return true; // ✅ success
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error while starting exam attempt");
//                return false;
//            }



//        }


//        public async Task<CandidateAttempt> SubmitAttemptAsync(int candidateId, Dictionary<int, char> answers)
//        {
//            var attempt = await _db.CandidateAttempts
//                .Where(a => a.CandidateId == candidateId && a.EndTimeUtc == null)
//                .OrderByDescending(a => a.Id)
//                .FirstOrDefaultAsync();

//            if (attempt == null)
//                throw new InvalidOperationException("No active attempt found.");

//            int totalScore = 0;
//            var sectionScores = new Dictionary<SectionType, int>();
//            var sectionQuestionCounts = new Dictionary<SectionType, int>();

//            foreach (var kv in answers)
//            {
//                var q = await _db.Questions.FindAsync(kv.Key);
//                if (q != null)
//                {
//                    //bool isCorrect = char.ToUpperInvariant(kv.Value) == char.ToUpperInvariant(q.CorrectOption);
//                    bool isCorrect = kv.Value.ToString().Equals(q.CorrectOption, StringComparison.OrdinalIgnoreCase);
//                    if (isCorrect)
//                    {
//                        totalScore += 2; // ✅ 2 marks each
//                        if (!sectionScores.ContainsKey(q.Section)) sectionScores[q.Section] = 0;
//                        sectionScores[q.Section] += 2;
//                    }

//                    // Track how many questions per section for breakdown
//                    if (!sectionQuestionCounts.ContainsKey(q.Section))
//                        sectionQuestionCounts[q.Section] = 0;
//                    sectionQuestionCounts[q.Section]++;

//                    var ca = new CandidateAnswer
//                    {
//                        CandidateAttemptId = attempt.Id,
//                        QuestionId = q.Id,
//                        SelectedOption = kv.Value.ToString(), // ✅ store as string
//                        CorrectOption = q.CorrectOption.ToString(),
//                        IsCorrect = isCorrect
//                    };
//                    await _db.CandidateAnswers.AddAsync(ca);
//                }
//            }

//            // ✅ Build readable breakdown JSON
//            var breakdown = sectionScores
//                .Select(s => new
//                {
//                    Section = s.Key.ToString(),
//                    Score = s.Value,
//                    Total = sectionQuestionCounts[s.Key] * 2
//                })
//                .ToDictionary(x => x.Section, x => $"{x.Score}/{x.Total}");

//            attempt.Score = totalScore;
//            attempt.SectionBreakdownJson = System.Text.Json.JsonSerializer.Serialize(breakdown);
//            attempt.EndTimeUtc = DateTime.UtcNow;

//            _db.CandidateAttempts.Update(attempt);
//            await _db.SaveChangesAsync();

//            return attempt;
//        }





//        public Task<bool> IsRegistrationOpenAsync(IConfiguration config)
//        {
//            var testDate = DateTime.Parse(config["TestSettings:TestDate"]);
//            var openFrom = testDate.AddDays(-int.Parse(config["TestSettings:RegistrationOpenDaysBefore"]));
//            var now = DateTime.UtcNow.Date; // compare dates in UTC
//            return Task.FromResult(now >= openFrom.Date && now <= testDate.Date);
//        }


//        public Task<bool> IsTestWindowOpenAsync(IConfiguration config)
//        {
//            var testDate = DateTime.Parse(config["TestSettings:TestDate"]);
//            var startTime = TimeSpan.Parse(config["TestSettings:TestStartTime"]);
//            var endTime = TimeSpan.Parse(config["TestSettings:TestEndTime"]);


//            var nowUtc = DateTime.UtcNow;
//            // Convert test date + time (assumed local server time) to UTC by using server timezone if needed.
//            var testStart = DateTime.SpecifyKind(new DateTime(testDate.Year, testDate.Month, testDate.Day).Add(startTime), DateTimeKind.Local).ToUniversalTime();
//            var testEnd = DateTime.SpecifyKind(new DateTime(testDate.Year, testDate.Month, testDate.Day).Add(endTime), DateTimeKind.Local).ToUniversalTime();


//            return Task.FromResult(nowUtc >= testStart && nowUtc <= testEnd);
//        }
//    }
//}








//using AutomatedExamSystem.Data;
//using AutomatedExamSystem.Models;
//using Microsoft.EntityFrameworkCore;

//namespace AutomatedExamSystem.Services
//{
//    public class ExamService : IExamService
//    {
//        private readonly ExamAppDbContext _db;
//        private readonly ILogger<ExamService> _logger;

//        public ExamService(ExamAppDbContext db, ILogger<ExamService> logger)
//        {
//            _db = db;
//            _logger = logger;
//        }

//        // ===============================
//        // 🚀 Start Attempt
//        // ===============================
//        public async Task<bool> StartAttemptAsync(Candidate candidate)
//        {
//            try
//            {
//                if (candidate.HasTakenExam)
//                    throw new InvalidOperationException("Candidate already attempted the test.");

//                bool hasAttempt = await _db.CandidateAttempts
//                    .AnyAsync(a => a.CandidateId == candidate.Id);

//                if (hasAttempt)
//                {
//                    _logger.LogInformation($"Candidate {candidate.Email} has already taken the exam.");
//                    return false;
//                }

//                var attempt = new CandidateAttempt
//                {
//                    CandidateId = candidate.Id,
//                    StartTimeUtc = DateTime.UtcNow
//                };

//                await _db.CandidateAttempts.AddAsync(attempt);
//                candidate.HasTakenExam = true;

//                _db.Candidates.Update(candidate);
//                await _db.SaveChangesAsync();

//                _logger.LogInformation($"Exam attempt started for {candidate.Email}");
//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error while starting exam attempt");
//                return false;
//            }
//        }

//        // ===============================
//        // 🧮 Submit Attempt (with Scoring)
//        // ===============================
//        public async Task<CandidateAttempt> SubmitAttemptAsync(int candidateId, Dictionary<int, char> answers)
//        {
//            var attempt = await _db.CandidateAttempts
//                .Where(a => a.CandidateId == candidateId && a.EndTimeUtc == null)
//                .OrderByDescending(a => a.Id)
//                .FirstOrDefaultAsync();

//            if (attempt == null)
//                throw new InvalidOperationException("No active attempt found.");

//            int totalQuestions = 0;
//            int correctAnswers = 0;

//            var sectionSummary = new Dictionary<SectionType, int>();

//            foreach (var kv in answers)
//            {
//                var question = await _db.Questions.FindAsync(kv.Key);
//                if (question != null)
//                {
//                    totalQuestions++;

//                    bool isCorrect = char.ToUpperInvariant(kv.Value) == char.ToUpperInvariant(question.CorrectOption);
//                    if (isCorrect)
//                    {
//                        correctAnswers++;
//                        if (!sectionSummary.ContainsKey(question.Section))
//                            sectionSummary[question.Section] = 0;
//                        sectionSummary[question.Section]++;
//                    }

//                    // ✅ Save user's answer + correct answer
//                    var answer = new CandidateAnswer
//                    {
//                        CandidateAttemptId = attempt.Id,
//                        QuestionId = question.Id,
//                        SelectedOption = kv.Value,
//                        CorrectOption = question.CorrectOption,
//                        IsCorrect = isCorrect
//                    };
//                    await _db.CandidateAnswers.AddAsync(answer);
//                }
//            }

//            // Compute final score (2 marks per correct answer or 100% scale)
//            int score = correctAnswers * 2;

//            attempt.Score = score;
//            attempt.TotalQuestions = totalQuestions;
//            attempt.CorrectAnswers = correctAnswers;
//            attempt.SectionBreakdown = string.Join(", ", sectionSummary.Select(s => $"{s.Key}: {s.Value}"));
//            attempt.EndTimeUtc = DateTime.UtcNow;

//            _db.CandidateAttempts.Update(attempt);
//            await _db.SaveChangesAsync();

//            return attempt; // ✅ Return attempt summary for result page
//        }

//        // ===============================
//        // 📅 Registration Check
//        // ===============================
//        public Task<bool> IsRegistrationOpenAsync(IConfiguration config)
//        {
//            var testDate = DateTime.Parse(config["TestSettings:TestDate"]);
//            var openFrom = testDate.AddDays(-int.Parse(config["TestSettings:RegistrationOpenDaysBefore"]));
//            var now = DateTime.UtcNow.Date;
//            return Task.FromResult(now >= openFrom && now <= testDate.Date);
//        }

//        // ===============================
//        // ⏰ Exam Window Check
//        // ===============================
//        public Task<bool> IsTestWindowOpenAsync(IConfiguration config)
//        {
//            var testDate = DateTime.Parse(config["TestSettings:TestDate"]);
//            var startTime = TimeSpan.Parse(config["TestSettings:TestStartTime"]);
//            var endTime = TimeSpan.Parse(config["TestSettings:TestEndTime"]);

//            var nowUtc = DateTime.UtcNow;
//            var testStart = DateTime.SpecifyKind(
//                new DateTime(testDate.Year, testDate.Month, testDate.Day).Add(startTime),
//                DateTimeKind.Local).ToUniversalTime();

//            var testEnd = DateTime.SpecifyKind(
//                new DateTime(testDate.Year, testDate.Month, testDate.Day).Add(endTime),
//                DateTimeKind.Local).ToUniversalTime();

//            return Task.FromResult(nowUtc >= testStart && nowUtc <= testEnd);
//        }
//    }
//}
