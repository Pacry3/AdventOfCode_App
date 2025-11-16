using System.Diagnostics;

namespace AOC;

// != or is not for null checks?

// mistakes:
// no index out of range handling in userInput splitting >> Program.RunBuild
// no index out of range handling in File.ReadLine return >> ProgramData.GetInput.UseInput
// compare result and time length with part 1 even if only part 2 is requested >> Day.PrintDay
// print result of part 1 if part 2 is requested >> Day.PrintDay

// feature dynamic method reload 

// todo: Dont exit program when invalid input in build

sealed class Program
{
    private static ExecutionState State { get; set; }
    private static Day[] Days { get; set; } = null!;
    
    private static void Main(string[] args)
    {
        // this or init in static constructor and split State init and days init?
        (State, Days) = ProgramData.GetProgramData(args);

        Day day;
        if (State == ExecutionState.Build)
            RunBuild();
        else if (State == ExecutionState.DebugDay)
        {
            day = Days.Last();
            RunDays(day);
            PrintResults(day);
        }
        else
        {
            day = Days.Last();
            var dayPart = day.HasSecondPart ? DayPartInfo.PartTwo : DayPartInfo.PartOne;
            RunDays(day, dayPart);
            PrintResults(day, dayPart);
        }
    }

    private static void RunBuild()
    {
        while (true)
        {
            string userInput = Console.ReadLine() ?? string.Empty;
            userInput = userInput.Trim().ToLowerInvariant();
            bool parallel = !userInput.Contains('b');

            if (ProgramData.InputContainsExitCode(userInput))
                Environment.Exit(0);
            /*else if (userInput == "reset" || userInput == "reload" || userInput == "r")
            {
                SetProgramData();
                continue;
            }*/

            if (userInput == string.Empty || userInput.Contains("all") || userInput.Contains('b'))
            {
                RunDays(parallel: parallel);
                PrintResults();
            }
            else
            {
                var userInputs = userInput.Split([',', '.', ' ']);
                bool validDay = TryGetValidDay(userInputs, out Day? day);
                if (!validDay)
                    continue;
                bool validPart = TryGetValidPart(userInputs, out int partNumber);
                
                if (!validPart)
                {
                    RunDays(day!, parallel: parallel);
                    PrintResults(day);
                }
                else
                {
                    DayPartInfo dayPart = partNumber == 1 ? DayPartInfo.PartOne : DayPartInfo.PartTwo;
                    RunDays(day!, dayPart);
                    PrintResults(day, dayPart);
                }
            }
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private static bool TryGetValidDay(string[] userInputs, out Day? day)
    {
        Debug.Assert(userInputs.Length > 0);
        bool validDay = int.TryParse(userInputs[0], out int dayNumber) && dayNumber <= Days.Length && dayNumber > 0;

        if (validDay)
        {
            day = Days[dayNumber - 1];
            return true;
        }
        
        Console.Write($"Invalid day number {userInputs[0]}");
        day = null;
        return false;
    }

    private static bool TryGetValidPart(string[] userInputs, out int partNumber)
    {
        string secondInput = "";
        if (userInputs.Length > 1)
            secondInput = userInputs[1];
        partNumber = 0;
        
        bool validPart = int.TryParse(secondInput, out int tempPartNumber) && (tempPartNumber == 1 || tempPartNumber == 2);
        partNumber = tempPartNumber;
        
        if (!validPart && partNumber != 0)
        {
            Console.WriteLine($"Part {tempPartNumber} is not valid");
            return false;
        }

        return true;
    }

    private static void RunDays(Day? day = null, DayPartInfo? dayPart = null, bool parallel = true)
    {
        Debug.Assert(dayPart is null || day is not null && dayPart is not null);

        if (day is null)
        {
            if (parallel)
                Parallel.ForEach(Days, tempDay => tempDay.Run(true));
            else
                foreach (var tempDay in Days)
                    tempDay.Run(false);
        }
        else if (dayPart is null)
            day.Run(parallel);
        else
            day.Run(dayPart.Value);
    }

    private static void PrintResults(Day? day = null, DayPartInfo? dayPart = null)
    {
        Debug.Assert(dayPart is null || day is not null && dayPart is not null);
        
        Console.WriteLine();
        Console.WriteLine("Results:");
        Console.WriteLine("---------------------");
        if (day is null)
            foreach (var tempDay in Days)
                tempDay.PrintDay();
        else
            day.PrintDay(dayPart);
        Console.WriteLine();
    }
}