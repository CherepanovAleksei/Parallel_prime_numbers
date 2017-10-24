using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;


class FindPrimeInRange
{
    public static bool IsPrime(int number)
    {
        if (number <= 1) return false;
        for(int i=2; i * i <= number; i++)
        {
            if (number % i == 0) return false;
        }
        //Console.Write("{0}, ", number);
        return true;
    }

    public static List<int> SimpleFindInRange(int left, int right)
    {
        List<int> primeNumbers = new List<int>();
        for(int number = left; number<= right; number++)
        {
            if (IsPrime(number)) primeNumbers.Add(number);
        }
        return primeNumbers;
    }

    public static List<int> ParallelFindInRange(int left, int right, int range)
    {
        List<int> primeNumbers = new List<int>();
        if (right - left > range)
        {
            Task<List<int>> one = Task<List<int>>.Run(() => ParallelFindInRange(left, (right + left) / 2, range));
            Task<List<int>> two = Task<List<int>>.Run(() => ParallelFindInRange((right + left) / 2 + 1, right, range));

            Task.WaitAll(one, two);

            primeNumbers.AddRange(one.Result);
            primeNumbers.AddRange(two.Result);
        }
        else
        {
            primeNumbers.AddRange(SimpleFindInRange(left, right));
        }
        return primeNumbers;
    }

    static void Main(string[] args)
    {
        Input:
        Console.Write("Range from:\n>>");
        if (!Int32.TryParse(Console.ReadLine(), out int begin))
        {
            Console.WriteLine("String could not be parsed.");
            goto Input;
            return;
        }

        Input2:
        Console.Write("Range to:\n>>");
        if (!Int32.TryParse(Console.ReadLine(), out int end))
        {
            Console.WriteLine("String could not be parsed.");
            goto Input2;
            return;
        }
        if (end < begin)
        {
            Console.WriteLine("Wrong range");
            goto Input;
            return;
        }

        Stopwatch stopWatch = new Stopwatch();
        //stopWatch.Start();
        ////Simple
        //SimpleFindInRange(begin, end);
        //stopWatch.Stop();
        //Console.WriteLine("Без распараллеливания:{0}", stopWatch.Elapsed);

        //определяем оптимальный интервал распараллеливания
        TimeSpan[] timeArray = new TimeSpan[50];
        int[] rangeArray = new int[50];
        //Parallel
        for (int range = 1; range <= 50; range++)
        {
            stopWatch.Restart();
            ParallelFindInRange(begin, end, range * 200);
            stopWatch.Stop();
            TimeSpan newtime = stopWatch.Elapsed;
            timeArray[range - 1] = stopWatch.Elapsed;
            rangeArray[range - 1] = range * 200;
        }

        TimeSpan minTime = timeArray[0];
        int optimalRange=0;
        for (int range = 0; range < 50; range++)
        {
            if(timeArray[range]<minTime)
            {
                minTime = timeArray[range];
                optimalRange = rangeArray[range];
            }
        }
        Console.WriteLine("Optimal range is={0}\nTime: {1}\n", optimalRange, minTime);

        Console.WriteLine("Press any key to quit...");
        Console.ReadKey();
    }   
    
    public static void PrintList(List<int> list)
    {
        for (int j = 0; j < list.Count; j++)
        {
            Console.Write("{0}, ", list[j]);
        }
        Console.WriteLine();
        return;
    }
}
