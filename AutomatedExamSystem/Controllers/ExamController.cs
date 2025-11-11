using AutomatedExamSystem.Data;
using AutomatedExamSystem.Models;
using AutomatedExamSystem.Repository;
using AutomatedExamSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutomatedExamSystem.Controllers
{
    public class ExamController : Controller
    {
        private readonly IQuestionRepository _qRepo;
        private readonly ICandidateRepository _cRepo;
        private readonly IExamService _examService;
        private readonly IConfiguration _config;

        public ExamController(
            IQuestionRepository qRepo,
            ICandidateRepository cRepo,
            IExamService examService,
            IConfiguration config)
        {
            _qRepo = qRepo;
            _cRepo = cRepo;
            _examService = examService;
            _config = config;
        }

        // ✅ Initial entry point for candidate after login
        public async Task<IActionResult> Index()
        {
            var id = HttpContext.Session.GetInt32("CandidateId");
            if (id == null)
                return RedirectToAction("Login", "Account");

            var candidate = await _cRepo.GetAsync(id.Value);
            if (candidate == null)
                return RedirectToAction("Login", "Account");

            // ✅ Ensure candidate has level information
            if (string.IsNullOrWhiteSpace(candidate.Level))
            {
                ViewBag.Error = "Your academic level could not be determined. Please contact support.";
                return View("Error");
            }

            // ✅ Load 10 questions per section based on level
            var questions = new List<Question>();
            foreach (var section in Enum.GetValues(typeof(SectionType)).Cast<SectionType>())
            {
                var qs = await _qRepo.GetBySectionAndLevelAsync(section, candidate.Level, 10);
                questions.AddRange(qs);
            }

            // Log for debugging
            Console.WriteLine($"[ExamController] Loaded {questions.Count} questions for {candidate.FullName} ({candidate.Level})");

            // Render Start view
            return View("StartAgain", questions);
        }

        // ✅ Called when user tries again (e.g., no answers selected)
        public async Task<IActionResult> StartAgain()
        {
            var id = HttpContext.Session.GetInt32("CandidateId");
            if (id == null)
                return RedirectToAction("Login", "Account");

            var candidate = await _cRepo.GetAsync(id.Value);
            if (candidate == null)
                return RedirectToAction("Login", "Account");

            // Fetch 10 questions per section again
            var questions = new List<Question>();
            foreach (var section in Enum.GetValues(typeof(SectionType)).Cast<SectionType>())
            {
                var qs = await _qRepo.GetBySectionAndLevelAsync(section, candidate.Level, 10);
                questions.AddRange(qs);
            }

            return View("StartAgain", questions);
        }

        // ✅ Handles form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(IFormCollection form)
        {
            var id = HttpContext.Session.GetInt32("CandidateId");
            if (id == null)
                return RedirectToAction("Login", "Account");

            // ✅ Extract selected answers from form
            var answers = new Dictionary<int, char>();
            foreach (var key in form.Keys)
            {
                if (key.StartsWith("answers[") && key.EndsWith("]"))
                {
                    var idStr = key.Substring(8, key.Length - 9); // extract number inside [ ]
                    if (int.TryParse(idStr, out int qId))
                    {
                        var val = form[key].ToString();
                        if (!string.IsNullOrEmpty(val))
                            answers[qId] = val[0];
                    }
                }
            }

            // ✅ If no answers selected
            if (answers.Count == 0)
            {
                TempData["ErrorMessage"] = "Please select at least one answer before submitting.";
                return RedirectToAction("StartAgain");
            }

            try
            {
                // ✅ Submit and compute score
                var attempt = await _examService.SubmitAttemptAsync(id.Value, answers);

                if (attempt == null)
                {
                    ViewBag.Error = "Submission failed. Please try again.";
                    return View("Error");
                }

                // ✅ Success page
                return View("SubmitSuccess", attempt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ExamController.Submit] Error: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while submitting your answers. Please try again.";
                return RedirectToAction("StartAgain");
            }
        }
    }
}








