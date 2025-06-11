using System.ComponentModel.DataAnnotations;

namespace DynamicTimetableGenerator.Models
{
    public class SubjectHourModel
    {
        [Required(ErrorMessage = "Subject name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Hours are required.")]
        [Range(1, 100, ErrorMessage = "Hours must be a positive number.")]
        public int Hours { get; set; }
    }
}
