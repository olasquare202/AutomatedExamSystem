# AutomatedExamSystem
The Automated Exam System is a web-based assessment platform designed to streamline the process of conducting online examinations, Computer Based Test( C B T ) for students at different academic levels. It allows administrators to create and manage questions, assign tests, monitor candidates, and automatically evaluate results — all within a secure and user-friendly environment.

This system ensures fairness, accuracy, and efficiency by automating every stage of the examination process — from question generation to result computation and report generation.

🚀 Key Features:

User Roles: Admin and Candidate dashboards with separate authentication.

Automated Test Scheduling: Timed assessments (e.g., 50 minutes) that auto-submit when time elapses.

Question Management: Admins can create, edit, and categorize questions by Level (100L, 200L, etc.) and Section (Use of English, Logical Reasoning, etc.).

Candidate Management: Registration and login system with unique candidate codes.

Real-time Exam Engine: Displays one question at a time with automatic navigation and time tracking.

Automatic Scoring: Calculates candidate scores instantly after test completion.

Downloadable Reports: Admins can filter and download candidate scores by Level and Date Range as a PDF file.

Email Notifications: Sends exam credentials or result summaries via SMTP.

Responsive Interface: Built with modern web design for both desktop and mobile users.

🏗️ Tech Stack:

Frontend: ASP.NET Core MVC (Razor Views, Bootstrap 5)

Backend: ASP.NET Core 9, C#, Entity Framework Core, LINQ

Database: Microsoft SQL Server

Architecture: Clean Architecture with Repository Pattern

PDF Generation: QuestPDF

Authentication: ASP.NET Identity with role-based access

Email Service: SMTP configuration for secure notifications

🧮 Example Use Case:

Admin logs in and uploads questions for different levels.

Candidates log in using a code sent via email and take the timed test.

Once completed, the system auto-scores responses and stores results.

Admin can filter results by date or level and download the report in PDF format.

📈 Impact:

This system reduces administrative workload, minimizes human error in marking, ensures consistency across exams, and provides instant performance insights for both administrators and candidates.
