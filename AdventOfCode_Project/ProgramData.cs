namespace AOC;

enum ExecutionState
{
    Build,
    DebugDay,
    DebugDayPart
}

enum DayPartInfo
{
    PartOne,
    PartTwo
}

static class ProgramData
{
    //internal static ExecutionState State { get; private set; } // Handle by Main ??
    //internal static Day[] Days { get; private set; } // Handle by Main ??

    private const int CalenderDays = 26;

    private const string ReplaceString = "X";
    private const string DaysNamespaceStr = "AOC23.Days";
    private const string DayClassName = "DayX";
    private const string DayPart2ClassName = "";
    private const string DayPartMethodNaming = "PX"; // naming of the method which runs a part

    private const string InputDirPathStr = "/Users/pc/RiderProjects/AOC23/AOC23/Inputs/InputX.txt";
    private const string ExampleInputFilePathStr = "/Users/pc/RiderProjects/AOC23/AOC23/Inputs/Example.txt";
    private const bool SmartInputSwitching = true; // Whether to always prefer the example input for the current Part

    private static readonly string[] ExitCodes = ["exit", "close", "stop"];

    internal static (ExecutionState, Day[]) GetProgramData(string[] args)
    {
        //Day.Reset();
        var state = GetState(args);
        var days = GetDays();

        return (state, days);
    }

    #region External Getters

    public static (string, string) GetDayRefAndPartNameStr(int dayId, DayPartInfo dayPartInfo)
    {
        int partId = dayPartInfo == DayPartInfo.PartOne ? 1 : 2;
        string replace = ReplaceString;
        string daysDir = DaysNamespaceStr.Replace(replace, dayId.ToString());
        string dayRef = daysDir + '.' + DayClassName.Replace(replace, dayId.ToString());
        if (DayPart2ClassName.Length != 0)
            dayRef = daysDir + DayPart2ClassName.Replace(replace, dayId.ToString());
        string dayPartName = DayPartMethodNaming.Replace(replace, partId.ToString());

        return (dayRef, dayPartName);
    }

    // Handle by DayPart?
    internal static (string[] input, bool usedExample) GetInput(int dayId, DayPartInfo dayPartInfo, bool getNormalInput)
    {
        // name inputExample or exampleInput ?
        string inputPath = InputDirPathStr.Replace(ReplaceString, dayId.ToString());
        string exampleInputPath = ExampleInputFilePathStr.Replace(ReplaceString, dayId.ToString());

        bool validInput = TryGetInput(false, out var input);
        bool validExampleInput = TryGetInput(true, out var exampleInput);

        bool getInput = getNormalInput || UseInput();

        if (getInput)
            return (input!, false);
        else
            return (exampleInput!, true);

        #region Helpers

        bool UseInput()
        {
            // do ternary anyways?
            bool validFirstInputLine = (input ?? []).Length != 0;
            bool validFirstExampleInputLine = (exampleInput ?? []).Length != 0;
            if (validFirstInputLine)
                validFirstInputLine = (input?[0] ?? "").Length != 0;
            if (validFirstExampleInputLine)
                validFirstExampleInputLine = (exampleInput?[0] ?? "").Length != 0;

            bool realValidInput = validInput && validFirstInputLine;
            bool realValidExampleInput = validExampleInput && validFirstExampleInputLine;

            if (!realValidInput && !realValidExampleInput)
            {
                Console.WriteLine($"Day {dayId} has no valid Input file");
                Environment.Exit(1);
            }

            if (SmartInputSwitching is false && realValidInput)
                return true;
            else if (SmartInputSwitching is false)
                return false;

            if (dayPartInfo == DayPartInfo.PartOne)
            {
                if (realValidInput)
                    return true;
                return false;
            }

// do else here?
            if (realValidExampleInput)
                return false;
            return true;
        }

        bool TryGetInput(bool getExampleInput, out string[]? tempInput)
        {
            try
            {
                if (!getExampleInput)
                {
                    tempInput = File.ReadAllLines(inputPath);
                }
                else
                {
                    tempInput = File.ReadAllLines(exampleInputPath);
                }

                return true;
            }
            catch
            {
            }

            tempInput = null;
            return false;
        }

        #endregion Helpers
    }

    internal static bool InputContainsExitCode(string userInput)
    {
        if (ExitCodes.Any(exitCode => userInput.Contains(exitCode)))
            return true;
        return false;
    }

    #endregion External Getters

    #region Internal Getters

    private static ExecutionState GetState(string[] args)
    {
        var state = ExecutionState.Build;
        if (args.Length == 1)
        {
            if (args[0] == "--debug-day")
                state = ExecutionState.DebugDay;
            else if (args[0] == "--debug-day-part")
                state = ExecutionState.DebugDayPart;
        }
        else
        {
            if (args.Length > 1)
            {
                Console.Write("Invalid program arguments!");
                Environment.Exit(2);
            }
        }

        return state;
    }

    private static Day[] GetDays()
    {
        var days = new List<Day>();
        for (int i = 0; i < CalenderDays; i++)
        {
            if (Day.TryGetNewInstance(out var day))
                days.Add(day!);
            else
                break;
        }

        if (days.Count == 0)
        {
            Console.WriteLine("Implement a Day to run");
            Environment.Exit(1);
        }

        return days.ToArray();
    }

    #endregion Internal Getters
}