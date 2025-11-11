

using System.ComponentModel.DataAnnotations;

namespace AutomatedExamSystem.Models
{
    public enum SectionType
    {
        UseOfEnglish, LogicalReasoning, NumericalReasoning, CurrentAffairs, SituationalJudgment
    }

    public enum AcademicLevel
    {
        Level100 = 100,
        Level200 = 200
    }

    public class Question
    {
        public int Id { get; set; }

        // ✅ Nullable enum
        public SectionType? Section { get; set; }

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }

        // ✅ Optional level (nullable string)
        [Display(Name = "Level")]
        [RegularExpression("100L|200L", ErrorMessage = "Level must be either 100L or 200L.")]
        public string? Level { get; set; }

        // ✅ Optional correct answer
        [StringLength(1, ErrorMessage = "Enter only one character (A–D).")]
        [RegularExpression("[A-Da-d]", ErrorMessage = "Valid options are A, B, C, or D.")]
        public string? CorrectOption { get; set; }
    }
}








//using System.ComponentModel.DataAnnotations;

//namespace AutomatedExamSystem.Models
//{
//    public enum SectionType { UseOfEnglish, LogicalReasoning, NumericalReasoning, CurrentAffairs, SituationalJudgment }
//    public enum AcademicLevel
//    {
//        Level100 = 100,
//        Level200 = 200
//    }
//    public class Question
//    {

//        public int Id { get; set; }

//        [Required]
//        public SectionType Section { get; set; }  // UseOfEnglish, LogicalReasoning, etc.

//        [Required]
//        public string QuestionText { get; set; }

//        [Required]
//        public string OptionA { get; set; }
//        [Required]
//        public string OptionB { get; set; }
//        [Required]
//        public string OptionC { get; set; }
//        [Required]
//        public string OptionD { get; set; }
//        [Required]
//        [Display(Name = "Level")]
//        [RegularExpression("100L|200L", ErrorMessage = "Level must be either 100L or 200L.")]
//        public string Level { get; set; }


//        // store correct option as single char 'A','B','C','D'
//        [Required]
//        [StringLength(1, ErrorMessage = "Enter only one character (A–D).")]
//        [RegularExpression("[A-Da-d]", ErrorMessage = "Valid options are A, B, C, or D.")]
//        public string CorrectOption { get; set; }
//    }
//}








//using System;
//using System.ComponentModel.DataAnnotations;

//namespace AutomatedExamSystem.Models
//{
//    // ✅ Enum for exam sections
//    public enum SectionType
//    {
//        UseOfEnglish,
//        LogicalReasoning,
//        NumericalReasoning,
//        CurrentAffairs,
//        SituationalJudgment
//    }

//    // ✅ Enum for academic levels
//    public enum AcademicLevel
//    {
//        Level100 = 100,
//        Level200 = 200,
//        //Level300 = 300,
//        //Level400 = 400
//    }

//    public class Question
//    {
//        public int Id { get; set; }

//        // ✅ Section of the question
//        [Required]
//        [Display(Name = "Section")]
//        public SectionType Section { get; set; }

//        // ✅ Main question text
//        [Required]
//        [Display(Name = "Question Text")]
//        public string QuestionText { get; set; }

//        // ✅ Multiple choice options
//        [Required]
//        [Display(Name = "Option A")]
//        public string OptionA { get; set; }

//        [Required]
//        [Display(Name = "Option B")]
//        public string OptionB { get; set; }

//        [Required]
//        [Display(Name = "Option C")]
//        public string OptionC { get; set; }

//        [Required]
//        [Display(Name = "Option D")]
//        public string OptionD { get; set; }

//        // ✅ Academic Level (optional, e.g., 100L–400L)
//        [Display(Name = "Level")]
//        [RegularExpression("100L|200L", ErrorMessage = "Level must be 100L or 200L.")]
//        public string? Level { get; set; }

//        // ✅ Correct Answer
//        [Required(ErrorMessage = "Please specify the correct option (A–D).")]
//        [StringLength(1, ErrorMessage = "Enter only one character (A–D).")]
//        [RegularExpression("[A-Da-d]", ErrorMessage = "Valid options are A, B, C, or D.")]
//        [Display(Name = "Correct Option")]
//        public string CorrectOption { get; set; }

//        // ✅ Timestamps (useful for filtering, e.g., by creation date)
//        [Display(Name = "Date Created")]
//        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//    }
//}

