using AutomatedExamSystem.Models;
using AutomatedExamSystem.Models.DTOs;
using AutomatedExamSystem.Repository;
using AutomatedExamSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedExamSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ICandidateRepository _candidateRepo;
        private readonly IEmailSender _emailSender;
        private readonly IExamService _examService;
        private readonly IConfiguration _config;

        public AccountController(ICandidateRepository candidateRepo, IEmailSender emailSender, IExamService examService, IConfiguration config)
        {
            _candidateRepo = candidateRepo;
            _emailSender = emailSender;
            _examService = examService;
            _config = config;
        }
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            // If registration hasn't started yet, return a view displaying an error message.
            // Otherwise, show the registration view.
            if (!await _examService.IsRegistrationOpenAsync(_config))
                return View("ErrorView");
            return View("Register");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(CandidateDto model)
        {
            if (!ModelState.IsValid) return View(model);

            if (!await _examService.IsRegistrationOpenAsync(_config)) 
                return View("Register");


            var existing = await _candidateRepo.GetByEmailAsync(model.Email);
            if (existing != null) 
            { ModelState.AddModelError("", "Email already registered."); 
                return View(model);
            }

            var candidateCode = await _candidateRepo.GenerateNextCandidateCodeAsync();
            var candidate = new Candidate
            {
                FullName = model.FullName,
                Email = model.Email,
                Institution = model.Institution,
                Level = model.Level,
                CourseOfStudy = model.CourseOfStudy,
                PhoneNumber = model.PhoneNumber,
                CandidateCode = candidateCode
            };


            await _candidateRepo.AddAsync(candidate);
            await _candidateRepo.Save();

            //// send confirmation email (stub)
            //var subject = "Registration Confirmation - Automated Exam";
            //var body = $"Hello {candidate.FullName},\nYour Candidate ID: {candidate.CandidateCode}\nTest Date: {_config["TestSettings:TestDate"]} {_config["TestSettings:TestStartTime"]} - {_config["TestSettings:TestEndTime"]}\nLogin at: /Account/Login";
            //await _emailSender.SendEmailAsync(candidate.Email, subject, body);


            // ✅ Send confirmation email using SMTP
            var subject = "Registration Confirmation - Automated Exam System";

            var testDate = _config["TestSettings:TestDate"];
            var startTime = _config["TestSettings:TestStartTime"];
            var endTime = _config["TestSettings:TestEndTime"];
            var portalUrl = _config["PortalSettings:LoginUrl"]; // ✅ pulled from appsettings.json

            var body = $@"
                    <html>
                    <body style='font-family:Arial, sans-serif;'>
                        <p>Hello <b>{candidate.FullName}</b>,</p>
                        <p>Your registration was successful for the <b>Automated Exam System</b>.</p>
                        <p>
                            <b>Candidate ID:</b> {candidate.CandidateCode}<br/>
                            <b>Test Date:</b> {testDate}<br/>
                            <b>Test Time:</b> {startTime} - {endTime} (WAT)
                        </p>
                        <p>You can log in to take your test using the link below:</p>
                        <p>
                            <a href='{portalUrl}' 
                               style='display:inline-block;padding:8px 14px;background-color:#2a7ae2;color:#ffffff;
                                      text-decoration:none;border-radius:5px;'>Access Test Portal</a>
                        </p>
                        <p>Or manually visit: <b>{portalUrl}</b></p>
                        <p>Best regards,<br/>Automated Exam System Team</p>
                    </body>
                    </html>
                    ";

            await _emailSender.SendEmailAsync(candidate.Email, subject, body);



            return View("RegistrationSuccess", candidate);
        }
        [HttpGet]
        public IActionResult Login() => View();



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string candidateCode, string email)
        {
            var candidate = await _candidateRepo.GetByEmailAsync(email);
            if (candidate == null || candidate.CandidateCode != candidateCode)
            {
                ModelState.AddModelError("", "Invalid credentials");
                return View();
            }

            if (!await _examService.IsTestWindowOpenAsync(_config))
            {
                ModelState.AddModelError("", "Test is not available at this time.");
                return View("ExamNotAvailable");
            }

            // ✅ Check if candidate has already taken exam
            if (candidate.HasTakenExam)
            {
                // Go to a custom page instead of reloading the login form
                return View("AlreadyAttempted", candidate);
            }

            // Try to start attempt
            bool started = await _examService.StartAttemptAsync(candidate);
            if (!started)
            {
                //
                ModelState.AddModelError("", "Attempt failed, try again later.");
                return View();
            }

            // Create session
            HttpContext.Session.SetInt32("CandidateId", candidate.Id);
            return RedirectToAction("Index", "Exam");
        }

        
        // ===================================
        // 🔐 Logout Action
        // ===================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Clear session data and redirect to login
            HttpContext.Session.Clear();
            // Redirect to Login page
            return RedirectToAction("Login","Account");
        }
    }
}

