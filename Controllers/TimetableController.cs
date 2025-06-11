using Microsoft.AspNetCore.Mvc;
using DynamicTimetableGenerator.Models;
using System.Reflection.Metadata;
using ClosedXML.Excel;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

public class TimetableController : Controller
{
    private static InputModel inputModel;
    private static List<SubjectHourModel> subjectHours;

    public IActionResult Step1()
    {
        return View(new InputModel());
    }

    [HttpPost]
    public IActionResult Step1(InputModel model)
    {
        if (model.WorkingDays < 1 || model.WorkingDays > 7 || model.SubjectsPerDay < 1 || model.SubjectsPerDay > 8 || model.TotalSubjects < 1)
        {
            ModelState.AddModelError("", "Invalid input values");
            return View(model);
        }

        inputModel = model;
        return RedirectToAction("Step2");
    }

    public IActionResult Step2()
    {
        ViewBag.TotalHours = inputModel.TotalHours;
        var subjects = new List<SubjectHourModel>();
        for (int i = 0; i < inputModel.TotalSubjects; i++)
        {
            subjects.Add(new SubjectHourModel());
        }
        return View(subjects);
    }

    [HttpPost]
    public IActionResult Step2(List<SubjectHourModel> subjects)
    {
        ViewBag.TotalHours = inputModel.TotalHours;


        if (!ModelState.IsValid)
        {
            return View(subjects);
        }

        int totalHoursEntered = subjects.Sum(s => s.Hours);
        if (totalHoursEntered != inputModel.TotalHours)
        {

            ModelState.AddModelError(string.Empty, $"Total subject hours must equal {inputModel.TotalHours}. You entered {totalHoursEntered}.");
            return View(subjects);
        }

        subjectHours = subjects;
        return RedirectToAction("Generate");
    }



    public IActionResult Generate()
    {
        var timetable = GenerateTimetable(inputModel.WorkingDays, inputModel.SubjectsPerDay, subjectHours);
        return View(timetable);
    }

    private TimetableModel GenerateTimetable(int days, int perDay, List<SubjectHourModel> subjects)
    {
        string[,] table = new string[perDay, days];
        var subjectPool = new List<string>();

        foreach (var s in subjects)
        {
            for (int i = 0; i < s.Hours; i++)
                subjectPool.Add(s.Name);
        }

        var random = new Random();
        for (int row = 0; row < perDay; row++)
        {
            for (int col = 0; col < days; col++)
            {
                if (subjectPool.Count == 0) break;
                int idx = random.Next(subjectPool.Count);
                table[row, col] = subjectPool[idx];
                subjectPool.RemoveAt(idx);
            }
        }

        return new TimetableModel
        {
            Days = Enumerable.Range(1, days).Select(i => $"Day {i}").ToList(),
            Timetable = table
        };
    }



    public IActionResult ExportToExcel()
    {
        var timetable = GenerateTimetable(inputModel.WorkingDays, inputModel.SubjectsPerDay, subjectHours);
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Timetable");

        // Header
        for (int i = 0; i < timetable.Days.Count; i++)
            worksheet.Cell(1, i + 1).Value = timetable.Days[i];

        // Rows
        for (int r = 0; r < timetable.Timetable.GetLength(0); r++)
        {
            for (int c = 0; c < timetable.Timetable.GetLength(1); c++)
            {
                worksheet.Cell(r + 2, c + 1).Value = timetable.Timetable[r, c];
            }
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Timetable.xlsx");
    }

    public IActionResult ExportToPdf()
    {
        var timetable = GenerateTimetable(inputModel.WorkingDays, inputModel.SubjectsPerDay, subjectHours);

        if (timetable == null || timetable.Days.Count == 0)
            return Content("Timetable is empty");

        byte[] pdfBytes;
        using (var ms = new MemoryStream())
        {
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var doc = new iText.Layout.Document(pdf);

            var table = new Table(timetable.Days.Count);

            // Header
            foreach (var day in timetable.Days)
            {
                table.AddHeaderCell(day);
            }

            // Body
            for (int i = 0; i < timetable.Timetable.GetLength(0); i++)
            {
                for (int j = 0; j < timetable.Timetable.GetLength(1); j++)
                {
                    table.AddCell(timetable.Timetable[i, j] ?? "-");
                }
            }

            doc.Add(table);
            doc.Close();

            pdfBytes = ms.ToArray();
        }

        return File(pdfBytes, "application/pdf", "Timetable.pdf");
    }


}
