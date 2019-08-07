using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Features_Tasks
{
    class Program
    {
        enum PiComplexity { Low = 10000, Medium = 20000, High = 40000 }

        static CancellationToken _ct;

        /// <summary>
        /// Defines the entry point of the application.
        /// 
        /// Async entry point supported since C# 7.1.
        /// 
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static async Task Main(string[] args)
        {
            // Parameters
            var taskCount = 6;
            var complexity = PiComplexity.Medium;
            var runConcurent = true; // Parallelism
            var msTimeOut = 30000;

            // Cancellation token initialization
            var tokenSource = new CancellationTokenSource(msTimeOut);
            _ct = tokenSource.Token;

            try
            {
                Console.WriteLine("Task Created (Press 'C' to cancel).");

                var completed = false;
                var watch = Stopwatch.StartNew();

                // Asyn task
                var piTask = ComputePiAsync(taskCount, complexity, runConcurent).ContinueWith(c => completed = true);

                Console.Write("Computing PI decimals ");
                while (!completed)
                {
                    if (Console.KeyAvailable)
                    {
                        var c = Console.ReadKey(true);
                        if (c.KeyChar == 'c' || c.KeyChar == 'C')
                            tokenSource.Cancel();
                    }

                    Console.Write(".");

                    await Task.Delay(200);
                }

                watch.Stop();

                Console.WriteLine();
                Console.WriteLine($"Task Completed in {watch.ElapsedMilliseconds} ms.");
            }
            catch (Exception e)
            {
                if (e.InnerException is TaskCanceledException)
                {
                    Console.WriteLine();
                    Console.WriteLine("Task Cancelled !");
                }      
            }


            // Exit
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");

            Console.ReadKey();
        }

        /// <summary>
        /// Computes  PI async caller.
        /// </summary>
        private static async Task ComputePiAsync(int taskCount, PiComplexity complexity, bool runConcurent)
        {
            // Cancellation callback
            _ct.Register(() => throw new TaskCanceledException());

            if (runConcurent)
            {
                var taskList = new List<Task<string>>();

                for (int i = 0; i < taskCount; i++)
                    taskList.Add(Task.Run(() => ComputePi((int)complexity)));

                // Start all concurent then await all
                await Task.WhenAll(taskList.ToArray());
            }
            else
            {
                // Start and await one by one
                for (int i = 0; i < taskCount; i++)
                    await Task.Run(() => ComputePi((int)complexity));
            }
        }

        /// <summary>
        /// Computes PI.
        /// </summary>
        /// <param name="decimalPrecision">The decimal precision.</param>
        /// <returns></returns>
        private static string ComputePi(int decimalPrecision)
        {
            var x = new SuperDecimal(0, decimalPrecision);
            var y = new SuperDecimal(0, decimalPrecision);

            x = x.ArcTan(16, 5);
            y = y.ArcTan(4, 239);
            x = x - y;

            return x.ToString();
        } 
    }
}
