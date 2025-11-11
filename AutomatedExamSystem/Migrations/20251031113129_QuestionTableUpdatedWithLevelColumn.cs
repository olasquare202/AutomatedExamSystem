using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedExamSystem.Migrations
{
    /// <inheritdoc />
    public partial class QuestionTableUpdatedWithLevelColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CandidateAttempts_CandidateId",
                table: "CandidateAttempts");

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateAttempts_CandidateId",
                table: "CandidateAttempts",
                column: "CandidateId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CandidateAttempts_CandidateId",
                table: "CandidateAttempts");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Questions");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateAttempts_CandidateId",
                table: "CandidateAttempts",
                column: "CandidateId");
        }
    }
}
