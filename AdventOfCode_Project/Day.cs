using System.Diagnostics;
using System.Reflection;

namespace AOC;

sealed class Day
{
    private static int _nextId = 1;
    internal bool HasSecondPart { get; }
    private (DayPart, DayPart?) DayParts { get; }
    
    internal void Run(bool parallel)
    {
        if (parallel && DayParts.Item2 is not null)
        {
            DayPart[] dayParts = [DayParts.Item1, DayParts.Item2];
            Parallel.ForEach(dayParts, dayPart => dayPart.Run());
        }
        else
        {
            DayParts.Item1.Run();
            DayParts.Item2?.Run();
        }
    }

    // do Run(bool parallel, DayPartInfo? specificDay) instead??
    // use dayPartInfo accross project or use day naming when possible
    internal void Run(DayPartInfo day)
    {
        if (day == DayPartInfo.PartOne)
            DayParts.Item1.Run();
        else
            DayParts.Item2?.Run();
    }

    internal void PrintDay(DayPartInfo? specificDayPart = null)
    {
        if (DayParts.Item2 is null && specificDayPart is null)
        {
            specificDayPart = DayPartInfo.PartOne;
        }
        // should be handled by main
        Debug.Assert(DayParts.Item2 is not null || specificDayPart == DayPartInfo.PartOne);
        
        bool runBoth = specificDayPart is null;
        
        (int result, int time) longestLength = 
                (
                    DayParts.Item1.Result.ToString().Length, 
                    DayParts.Item1.ElapsedMilliseconds.ToString().Length
                );
        
        if (runBoth)
        {
            (int result, int time) partTwoLength = 
                    (
                        DayParts.Item2!.Result.ToString().Length,
                        DayParts.Item2.ElapsedMilliseconds.ToString().Length
                    );
            longestLength.result = Math.Max(longestLength.result, partTwoLength.result);
            longestLength.time = Math.Max(longestLength.time, partTwoLength.time);
        }

        if (specificDayPart == DayPartInfo.PartOne || runBoth)
            DayParts.Item1.PrintResult(longestLength.result, longestLength.time);
        if (specificDayPart == DayPartInfo.PartTwo || runBoth)
            DayParts.Item2!.PrintResult(longestLength.result, longestLength.time);
        Console.WriteLine("---------------------");
    }
    
    #region Instantiation
    private Day((DayPart, DayPart?) dayParts)
    {
        HasSecondPart = dayParts.Item2 is not null;
        DayParts = dayParts;
    }

    internal static bool TryGetNewInstance(out Day? newDay)
    {
        var id = _nextId++;
        if (!TryGetDayParts(id, out (DayPart, DayPart?)? dayParts))
        {
            newDay = null;
            return false;
        }

        // why does dayParts! not work and dayParts.Value gives warning but then dayParts!.Value works???
        // also why does it give warning if the .Value does the exact same as the !
        newDay = new Day(dayParts!.Value);
        return true;
    }
        
    private static bool TryGetDayParts(int id, out (DayPart, DayPart?)? dayParts)
    {
        if (!DayPart.TryGetNewInstance(id, DayPartInfo.PartOne, out var dayPartOne))
        {
            dayParts = null;
            return false;
        }

        DayPart.TryGetNewInstance(id, DayPartInfo.PartTwo, out var dayPartTwo);
        dayParts = new (dayPartOne!, dayPartTwo);
        return true;
    }
    #endregion Instantiation
    
    private sealed class DayPart
    {
        private readonly int _dayId;
        private readonly int _id;
        private readonly DayPartInfo _dayPartInfo;
        private readonly MethodInfo _executionMethod;
        private string[] _input = null!;
        public bool UsedExample { get; private set; }
        public int Result { get; private set; }
        public long ElapsedMilliseconds { get; private set; }
    
        internal void Run()
        {
            UpdateInput();
            
            try
            {
                // only include what might throw?
                var sw = Stopwatch.StartNew();
                Result = (int)_executionMethod.Invoke(null, [_input])!;
                sw.Stop();
                ElapsedMilliseconds = sw.ElapsedMilliseconds;
            }
            catch
            {
                throw new Exception("Wrong Method Return Implementation");
            }

            var inputInfo = UsedExample ? "(exmpl)" : "(final)";
            Console.WriteLine($"..d{_dayId}p{_id} {inputInfo} exits with: {Result} ({(double)ElapsedMilliseconds / 1000:F3}s)");
        }

        internal void PrintResult(int longestResultLength, int longestTimeLength)
        {
            string result = Result.ToString();
            string time = $"{(double)ElapsedMilliseconds / 1000:F3}";
            result = result.PadRight(longestResultLength);
            time = time.PadRight(longestTimeLength);
            
            var inputInfo = UsedExample ? "(Exmpl)" : "(Final)";
            Console.WriteLine($"Day {_dayId} Part {_id} {inputInfo} :  " + result + "  (" + time + "s)");
        }

        void UpdateInput(bool forceNormalInput  = false)
        {
            (_input, UsedExample) = ProgramData.GetInput(_dayId, _dayPartInfo, forceNormalInput);
        }
    
        #region Instantiation
        private DayPart(int dayId, DayPartInfo dayPartInfo,  MethodInfo methodInfo)
        {
            _dayId = dayId;
            _id = dayPartInfo == DayPartInfo.PartOne ? 1 : 2;
            _dayPartInfo = dayPartInfo;
            _executionMethod = methodInfo;
            UpdateInput();
        }

        // name "new" or not?
        internal static bool TryGetNewInstance(int dayId, DayPartInfo dayPartInfo, out DayPart? newDayPart)
        {
            if (!TryGetMethodInfo(dayId, dayPartInfo, out var methodInfo))
            {
                newDayPart = null;
                return false;
            }
        
            newDayPart = new DayPart(dayId, dayPartInfo, methodInfo!);
            return true;
        }
    
        // Handle by ProgramData ???
        private static bool TryGetMethodInfo(int dayId, DayPartInfo dayPartInfo, out MethodInfo? newMethodInfo)
        {
            var (dayRef, dayPartName) = ProgramData.GetDayRefAndPartNameStr(dayId, dayPartInfo);
        
            Type? type = Type.GetType(dayRef);
            newMethodInfo = type?.GetMethod(dayPartName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        
            return newMethodInfo is not null;
        }
        #endregion Instantiation
    }
}