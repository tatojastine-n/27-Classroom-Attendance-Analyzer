using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum Presence { Present = 1, Absent = 0 }

public class StudentAttendance
{
    public string Name { get; }
    public List<Presence> AttendanceRecords { get; }
    public int MaxStreak { get; private set; }
    public int CurrentStreak { get; private set; }
    public decimal AbsenceRate { get; private set; }

    public StudentAttendance(string name, string attendanceData)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Student name cannot be empty");

        Name = name;
        AttendanceRecords = ParseAttendance(attendanceData);
        CalculateStats();
    }

    private List<Presence> ParseAttendance(string data)
    {
        var records = new List<Presence>();

        foreach (char c in data)
        {
            if (c == '1' || char.ToUpper(c) == 'Y')
                records.Add(Presence.Present);
            else if (c == '0' || char.ToUpper(c) == 'N')
                records.Add(Presence.Absent);
            else
                throw new ArgumentException($"Invalid attendance character: '{c}'");
        }

        if (records.Count != 30)
            throw new ArgumentException("Attendance data must contain exactly 30 records");

        return records;
    }

    private void CalculateStats()
    {
        int absentDays = AttendanceRecords.Count(r => r == Presence.Absent);
        AbsenceRate = (decimal)absentDays / AttendanceRecords.Count;

        int currentStreak = 0;
        MaxStreak = 0;
        CurrentStreak = 0;

        foreach (var record in AttendanceRecords)
        {
            if (record == Presence.Present)
            {
                currentStreak++;
                CurrentStreak = currentStreak;

                if (currentStreak > MaxStreak)
                    MaxStreak = currentStreak;
            }
            else
            {
                currentStreak = 0;
                CurrentStreak = 0;
            }
        }
    }

    public bool IsDefaulter(decimal absenceThreshold, int minStreak)
    {
        return AbsenceRate > absenceThreshold || MaxStreak < minStreak;
    }

    public override string ToString()
    {
        string attendanceString = string.Concat(AttendanceRecords
            .Select(r => r == Presence.Present ? "Y" : "N"));

        return $"{Name,-15} {attendanceString} | " +
               $"Max: {MaxStreak,2} days | " +
               $"Current: {CurrentStreak,2} days | " +
               $"Absent: {AbsenceRate:P0}";
    }
}

public class AttendanceAnalyzer
{
    private readonly List<StudentAttendance> _students;

    public AttendanceAnalyzer()
    {
        _students = new List<StudentAttendance>();
    }

    public void AddStudent(string name, string attendanceData)
    {
        _students.Add(new StudentAttendance(name, attendanceData));
    }

    public void AnalyzeAttendance(decimal absenceThreshold, int minStreak)
    {
        Console.WriteLine("\nAttendance Analysis Results");
        Console.WriteLine(new string('=', 80));
        Console.WriteLine("Legend: Y = Present, N = Absent");
        Console.WriteLine(new string('-', 80));

        int defaulterCount = 0;

        foreach (var student in _students.OrderBy(s => s.Name))
        {
            bool isDefaulter = student.IsDefaulter(absenceThreshold, minStreak);

            Console.ForegroundColor = isDefaulter ? ConsoleColor.Red : ConsoleColor.Gray;
            Console.Write(student);

            if (isDefaulter)
            {
                Console.Write(" [DEFaulTER]");
                defaulterCount++;
            }

            Console.ResetColor();
            Console.WriteLine();
        }

        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"Total Students: {_students.Count}");
        Console.WriteLine($"Defaulters: {defaulterCount} ({defaulterCount * 100m / _students.Count:P0})");

        var overallAbsenceRate = _students.Average(s => s.AbsenceRate);
        var avgMaxStreak = _students.Average(s => s.MaxStreak);
        Console.WriteLine($"\nOverall Absence Rate: {overallAbsenceRate:P0}");
        Console.WriteLine($"Average Maximum Streak: {avgMaxStreak:F1} days");
    }
}

namespace Classroom_Attendance_Analyzer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var analyzer = new AttendanceAnalyzer();

            Console.WriteLine("Enter students and their 30-day attendance records (Y/N or 1/0 format)");
            Console.WriteLine("Format: StudentName YNNYNY... (30 characters)");
            Console.WriteLine("Enter 'done' when finished\n");

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine().Trim();
                if (input.Equals("done", StringComparison.OrdinalIgnoreCase))
                    break;

                try
                {
                    var spacePos = input.IndexOf(' ');
                    if (spacePos == -1)
                    {
                        Console.WriteLine("Invalid format. Use: Name YNNY...");
                        continue;
                    }

                    var name = input.Substring(0, spacePos);
                    var attendanceData = input.Substring(spacePos + 1).Replace(" ", "");
                    analyzer.AddStudent(name, attendanceData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            Console.WriteLine("\nSet defaulter criteria:");
            Console.Write("Maximum acceptable absence rate (0-100%): ");
            decimal threshold = decimal.Parse(Console.ReadLine()) / 100m;

            Console.Write("Minimum acceptable attendance streak (days): ");
            int minStreak = int.Parse(Console.ReadLine());

            analyzer.AnalyzeAttendance(threshold, minStreak);
        }
    }
}
