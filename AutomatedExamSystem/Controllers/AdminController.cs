using AutomatedExamSystem.Models;
using AutomatedExamSystem.Repository;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AutomatedExamSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly IQuestionRepository _qRepo;
        private readonly ICandidateRepository _candidateRepo;
        private readonly IConfiguration _config;

        public AdminController(IQuestionRepository qRepo, ICandidateRepository candidateRepo, IConfiguration config)
        {
            _qRepo = qRepo;
            _candidateRepo = candidateRepo;
            _config = config;
        }

        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            var userInfo = _config["Admin:Username"];
            var userPass = _config["Admin:Password"];
            if (username == userInfo && password == userPass)
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Invalid admin credentials");
            return View();
        }

        private bool IsAdmin() => HttpContext.Session.GetString("IsAdmin") == "true";

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login");
            var qs = await _qRepo.GetAllAsync();
            return View(qs);
        }

        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login");
            return View(new Question());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question model)
        {
            if (!IsAdmin()) return RedirectToAction("Login");
            if (!ModelState.IsValid) return View(model);
            await _qRepo.AddAsync(model);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login");
            var q = await _qRepo.GetAsync(id);
            if (q == null) return NotFound();
            return View(q);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Question model)
        {
            if (!IsAdmin()) return RedirectToAction("Login");
            if (!ModelState.IsValid) return View(model);
            _qRepo.Update(model);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login");
            var q = _qRepo.GetAsync(id).Result;
            if (q != null) _qRepo.Remove(q);
            return RedirectToAction("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("IsAdmin");
            return RedirectToAction("Login");
        }

        //// ✅ NEW: Download Candidate Scores in PDF (Descending Order + Summary)

        [HttpGet]
        public async Task<IActionResult> DownloadCandidateScores()
        {
            if (!IsAdmin())
                return RedirectToAction("Login");

            // ✅ Get all candidates with their attempts (preloaded via Include)
            var candidates = await _candidateRepo.GetAllWithAttemptsAsync();

            // ✅ Handle cases where Attempts is null
            double averageScore = candidates
                .Select(c => c.Attempts?.Score ?? 0)
                .DefaultIfEmpty(0)
                .Average();

            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                var pdf = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(40);

                        // ===== HEADER =====
                        page.Header()
                            .AlignCenter()
                            .Text("Automated Exam System - Candidate Scores")
                            .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                        // ===== TABLE =====
                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(100); // Candidate ID
                                columns.RelativeColumn(2);   // Full Name
                                columns.RelativeColumn(2);   // Candidate Level
                                columns.RelativeColumn(2);   // Candidate Email
                                columns.RelativeColumn(2);   // Candidate Phone Number
                                columns.ConstantColumn(80);  // Score
                            });

                            // ---- Table Header ----
                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                    .Text("Candidate ID").SemiBold();
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                    .Text("Full Name").SemiBold();
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                    .Text("Candidate Level").SemiBold();
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                    .Text("Candidate Email").SemiBold();
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                    .Text("Candidate Phone Number").SemiBold();
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium)
                                    .Text("Score").SemiBold();
                            });

                            // ---- Table Rows ----
                            foreach (var c in candidates)
                            {
                                var score = c.Attempts?.Score ?? 0;

                                table.Cell().Text(c.Id.ToString());
                                table.Cell().Text($"{c.FullName} ({c.CandidateCode})");
                                table.Cell().Text($"{c.Level}") ;
                                table.Cell().Text($"{c.Email}");
                                table.Cell().Text($"{c.PhoneNumber}");
                                table.Cell().Text(score.ToString("F2"));
                            }

                            // ---- Summary Row ----
                            table.Cell().ColumnSpan(3).PaddingTop(20)
                                .Text($"Total Candidates: {candidates.Count()}     |     Average Score: {averageScore:F2}")
                                .SemiBold().FontColor(Colors.Blue.Medium);
                        });

                        // ===== FOOTER =====
                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Generated on: " + DateTime.Now.ToString("f"));
                            });
                    });
                });

                var pdfBytes = pdf.GeneratePdf();
                return File(pdfBytes, "application/pdf", "CandidateScores.pdf");
            }
            catch (Exception ex)
            {
                // Optional: log exception or return a view-friendly error message
                return StatusCode(500, $"Error generating PDF: {ex.Message}");
            }
        }

    }
}






//[HttpGet]
//public async Task<IActionResult> DownloadCandidateScoresFiltered(string? level, DateTime? fromDate, DateTime? toDate)
//{
//    if (!IsAdmin())
//        return RedirectToAction("Login");

//    // ✅ Get all candidates with their attempts
//    var candidates = await _candidateRepo.GetAllWithAttemptsAsync();

//    // ✅ Apply Level filter if provided
//    if (!string.IsNullOrEmpty(level))
//        candidates = candidates.Where(c => c.Level == level).ToList();

//    // ✅ Apply Date filter (single day or range)
//    if (fromDate.HasValue && toDate.HasValue)
//    {
//        candidates = candidates
//            .Where(c => c.RegistrationDate.Date >= fromDate.Value.Date &&
//                        c.RegistrationDate.Date <= toDate.Value.Date)
//            .ToList();
//    }
//    else if (fromDate.HasValue) // same-day or single date filter
//    {
//        candidates = candidates
//            .Where(c => c.RegistrationDate.Date == fromDate.Value.Date)
//            .ToList();
//    }

//    // ✅ Calculate average safely
//    double averageScore = candidates
//        .Select(c => c.Attempts?.Score ?? 0)
//        .DefaultIfEmpty(0)
//        .Average();

//    // ✅ Build dynamic filter message
//    string filterMessage = "📋 Showing all results";
//    if (!string.IsNullOrEmpty(level) || fromDate.HasValue)
//    {
//        string datePart = fromDate.HasValue && toDate.HasValue
//            ? $"📅 From {fromDate.Value:MMM d} to {toDate.Value:MMM d}"
//            : fromDate.HasValue ? $"📅 On {fromDate.Value:MMM d}" : "";

//        string levelPart = !string.IsNullOrEmpty(level) ? $" | 🎓 Level: {level}" : "";

//        filterMessage = $"{datePart}{levelPart}";
//    }

//    try
//    {
//        QuestPDF.Settings.License = LicenseType.Community;

//        var pdf = Document.Create(container =>
//        {
//            container.Page(page =>
//            {
//                page.Margin(40);

//                // ===== HEADER =====
//                page.Header()
//                    .AlignCenter()
//                    .Column(column =>
//                    {
//                        column.Item().Text("Automated Exam System - Candidate Scores")
//                            .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);
//                        column.Item().Text(filterMessage)
//                            .FontSize(11).FontColor(Colors.Grey.Medium);
//                    });

//                // ===== TABLE =====
//                page.Content().Table(table =>
//                {
//                    table.ColumnsDefinition(columns =>
//                    {
//                        columns.ConstantColumn(100); // Candidate ID
//                        columns.RelativeColumn(2);   // Full Name
//                        columns.ConstantColumn(80);  // Score
//                    });

//                    // ---- Table Header ----
//                    table.Header(header =>
//                    {
//                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium)
//                            .Text("Candidate ID").SemiBold();
//                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium)
//                            .Text("Full Name").SemiBold();
//                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Medium)
//                            .Text("Score").SemiBold();
//                    });

//                    // ---- Table Rows ----
//                    foreach (var c in candidates)
//                    {
//                        var score = c.Attempts?.Score ?? 0;

//                        table.Cell().Text(c.Id.ToString());
//                        table.Cell().Text($"{c.FullName} ({c.CandidateCode})");
//                        table.Cell().Text(score.ToString("F2"));
//                    }

//                    // ---- Summary Row ----
//                    table.Cell().ColumnSpan(3).PaddingTop(20)
//                        .Text($"Total Candidates: {candidates.Count}     |     Average Score: {averageScore:F2}")
//                        .SemiBold().FontColor(Colors.Blue.Medium);
//                });

//                // ===== FOOTER =====
//                page.Footer()
//                    .AlignCenter()
//                    .Text($"Generated on: {DateTime.Now:f}");
//            });
//        });

//        var pdfBytes = pdf.GeneratePdf();
//        return File(pdfBytes, "application/pdf", "CandidateScores.pdf");
//    }
//    catch (Exception ex)
//    {
//        return StatusCode(500, $"Error generating PDF: {ex.Message}");
//    }
//}




