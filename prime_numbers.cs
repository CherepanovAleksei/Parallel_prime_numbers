using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;


class FindPrimeInRange
{
    static object locker = new object();

    private static bool IsPrime(int number)
    {
        if (number <= 1) return false;
        for(int i=2; i * i <= number; i++)
        {
            if (number % i == 0) return false;
        }
        return true;
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

    public static List<int> SimpleFindInRange(int left, int right)
    {
        List<int> primeNumbers = new List<int>();
        for(int number = left; number<= right; number++)
        {
            if (IsPrime(number)) primeNumbers.Add(number);
        }
        return primeNumbers;
    }

    public static List<int> ThreadParallelFindInRange(int left, int right, int range)
    {
        List<int> primeNumbers = new List<int>();
        if (right - left > range)
        {
            List<int> res_one = new List<int>();
            List<int> res_two = new List<int>();
            Thread one = new Thread(() =>
            {
                res_one = ThreadParallelFindInRange(left, (right + left) / 2, range);
            });

            Thread two = new Thread(() =>
            {
                res_two = ThreadParallelFindInRange((right + left) / 2 + 1, right, range);
            });
            one.Start();
            two.Start();

            one.Join();
            two.Join();
            primeNumbers.AddRange(res_one);
            primeNumbers.AddRange(res_two);
        }
        else
        {
            primeNumbers.AddRange(SimpleFindInRange(left, right));
        }
        return primeNumbers;
    }

    public static List<int> ThreadPullParallelFindInRange(int left, int right)
    {
        List<int> primeNumbers = new List<int>();

        var myEvent = new ManualResetEvent(false);
        ThreadPool.QueueUserWorkItem(delegate
        {
            SimpleFindInRange(left, right);
            myEvent.Set();
        });
        myEvent.WaitOne();
        return primeNumbers;
    }

    
    public static List<int> TaskParallelFindInRange(int left, int right, int range)
    {
        List<int> primeNumbers = new List<int>();
        if (right - left > range)
        {
            Task<List<int>> one = Task<List<int>>.Run(() => TaskParallelFindInRange(left, (right + left) / 2, range));
            Task<List<int>> two = Task<List<int>>.Run(() => TaskParallelFindInRange((right + left) / 2 + 1, right, range));

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
        }

        Input2:
        Console.Write("Range to:\n>>");
        if (!Int32.TryParse(Console.ReadLine(), out int end))
        {
            Console.WriteLine("String could not be parsed.");
            goto Input2;
        }
        if (end < begin)
        {
            Console.WriteLine("Wrong range");
            goto Input;
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
            TaskParallelFindInRange(begin, end, range * 200);
            //ThreadParallelFindInRange(begin, end, range * 200);
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

        //thread pull
        //stopWatch.Start();
        //ThreadPullParallelFindInRange(begin, end);
        //stopWatch.Stop();
        //TimeSpan newtime = stopWatch.Elapsed;
        //Console.WriteLine(newtime);

        Console.WriteLine("Press any key to quit...");
        Console.ReadKey();
    }
}
