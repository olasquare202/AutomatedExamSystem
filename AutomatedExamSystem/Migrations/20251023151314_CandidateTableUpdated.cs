using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedExamSystem.Migrations
{
    /// <inheritdoc />
    public partial class CandidateTableUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CorrectAnswers",
                table: "CandidateAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SectionBreakdown",
                table: "CandidateAttempts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalQuestions",
                table: "CandidateAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CorrectOption",
                table: "CandidateAnswers",
                type: "nvarchar(1)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "CandidateAnswers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrectAnswers",
                table: "CandidateAttempts");

            migrationBuilder.DropColumn(
                name: "SectionBreakdown",
                table: "CandidateAttempts");

            migrationBuilder.DropColumn(
                name: "TotalQuestions",
                table: "CandidateAttempts");

            migrationBuilder.DropColumn(
                name: "CorrectOption",
                table: "CandidateAnswers");

            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "CandidateAnswers");
        }
    }
}
