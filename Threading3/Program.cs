using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//Solves the dining philosophers problem with a set of monitors (one for each chopstick)

namespace Threading2
{
    class Program
    {
        static readonly object Lock = new object();
        static object[] chopstick;
        static void Main(string[] args)
        {
            Program p = new Program();
            CancellationTokenSource tokensource1 = new CancellationTokenSource();
            CancellationTokenSource tokensource2 = new CancellationTokenSource();
            Console.WriteLine("Enter the number of starving philosophers at the table: ");
            int N = Int32.Parse(Console.ReadLine());
            chopstick = new object[N];
            for (int i = 0; i < N; i++)
                chopstick[i] = new object();
            List<Thread> threadset = new List<Thread>();
            for(int i=0;i<N;i++)
            {
                threadset.Add(new Thread(p.GrabChopSticks));
                threadset[i].Name = "Philosopher-" + i.ToString();
            }
            Thread[] threads = threadset.ToArray<Thread>();
            for (int i = 0; i < threads.Length; i++)
                threads[i].Start();
            for (int i = 0; i < threads.Length; i++)
                threads[i].Join();
            //Console.Write(Program.line);
            Console.WriteLine("All philosophers have finished eating");
            Console.ReadKey();
        }

        void GrabChopSticks()
        {
            Thread t = Thread.CurrentThread;
            int id = Convert.ToInt32(t.Name.Split('-')[1]);
            int flag = 0;
            while(true)
            {
                lock(chopstick[id])
                {
                    Console.WriteLine(t.Name + " has obtained chopstick " + id + " & will now attempt to get the other chopstick ");
                    if (Monitor.TryEnter(chopstick[(id + 1) % chopstick.Length]))
                    {
                        try
                        {
                            Console.WriteLine("Philosopher " + id + " has successfully grabbed chopsticks " + id + " & " + ((id + 1) % chopstick.Length) + " and can finish the meal");
                            Thread.Sleep(500);
                            flag = 1;
                            Console.WriteLine(t.Name + " has finished eating & will leave ");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Philosopher " + id + " encountered the following technical problem when eating= " + e);
                        }
                        finally
                        {
                            Monitor.Exit(chopstick[(id + 1) % chopstick.Length]);
                        }
                    }
                    else
                        Console.WriteLine(t.Name + " couldn't get both chopsticks, so he/she put down the " + id + " stick");
                }
                if (flag == 1)
                    break;
            }
        }

    }
}
