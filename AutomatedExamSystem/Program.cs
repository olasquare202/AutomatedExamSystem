using AutomatedExamSystem.Data;
using AutomatedExamSystem.Repository;
using AutomatedExamSystem.Services;
using AutomatedExamSystem.Services.AutomatedExamSystem.Services;
using Microsoft.EntityFrameworkCore;

namespace AutomatedExamSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ==========================================
            // 1?? Add MVC Controllers and Views
            // ==========================================
            builder.Services.AddControllersWithViews();

            // ==========================================
            // 2?? Configure Database
            // ==========================================
            var dbConnection = builder.Configuration.GetConnectionString("ExamConnection");
            builder.Services.AddDbContext<ExamAppDbContext>(options =>
                options.UseSqlServer(dbConnection));

            // ==========================================
            // 3?? Register Repositories
            // ==========================================
            builder.Services.AddScoped<ICandidateRepository, CandidateRepository>();
            builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();

            // ==========================================
            // 4?? Register Core Services
            // ==========================================
            builder.Services.AddScoped<IExamService, ExamService>();

            // ==========================================
            // 5?? Configure and Register Email (SMTP)
            // ==========================================
            builder.Services.Configure<SmtpSettings>(
                builder.Configuration.GetSection("Smtp"));
            builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

            // ==========================================
            // 6?? Session and Cookies
            // ==========================================
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(120);
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            // ==========================================
            // 7?? Build Application
            // ==========================================
            var app = builder.Build();

            // ==========================================
            // 8?? Middleware Pipeline
            // ==========================================
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}

