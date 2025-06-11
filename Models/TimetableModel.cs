namespace DynamicTimetableGenerator.Models
{
    public class TimetableModel
    {
        public List<string> Days { get; set; }
        public string[,] Timetable { get; set; }
    }

}
