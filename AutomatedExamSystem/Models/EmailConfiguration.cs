namespace AutomatedExamSystem.Models
{
    public class EmailConfiguration
    {
        public string From { get; set; } = null!;
        public string SmtpServer { get; set; } = null!;
        public int PortNumber { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
