using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedExamSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCandidateAttemptTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SectionBreakdown",
                table: "CandidateAttempts",
                newName: "SectionBreakdownJson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SectionBreakdownJson",
                table: "CandidateAttempts",
                newName: "SectionBreakdown");
        }
    }
}
