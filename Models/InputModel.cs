using System.ComponentModel.DataAnnotations;

namespace DynamicTimetableGenerator.Models
{
    public class InputModel
    {
        [Required(ErrorMessage = "Working Days is required")]
        [Range(1, 7, ErrorMessage = "Working Days must be between 1 and 7")]
        public int WorkingDays { get; set; }

        [Required(ErrorMessage = "Subjects Per Day is required")]
        [Range(1, 8, ErrorMessage = "Subjects Per Day must be between 1 and 8")]
        public int SubjectsPerDay { get; set; }

        [Required(ErrorMessage = "Total Subjects is required")]
        [Range(1, 100, ErrorMessage = "Total Subjects must be at least 1")]
        public int TotalSubjects { get; set; }

        public int TotalHours => WorkingDays * SubjectsPerDay;
    }
}
