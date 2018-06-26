using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//A Producer-consumer scenario: 2 producers use bi-directional signalling to combine their individual contributions into a finalized product.
//A set of consumer threads wait to consume the output from the producers. Each output from the producers is consumed only once 

namespace Threading1
{
    class Program
    {
        static readonly object Lock=new object();
        static EventWaitHandle handle = new AutoResetEvent(false);
        static EventWaitHandle endwritehandle = new ManualResetEvent(false);

        static EventWaitHandle writehandle = new AutoResetEvent(false);
        static EventWaitHandle handle2 = new AutoResetEvent(true);
        static string line = "";
        List<string> linelist = new List<string>();
        static int index = 0;
        static void Main(string[] args)
        {
            Program p = new Program();
            //var f = File.Open("D:\\sampletext.txt",FileMode.Open);
            StreamReader r = new StreamReader("D:\\Documents\\sampletext.txt");
            StreamReader r2= new StreamReader("D:\\Documents\\sampletext2.txt");
            Thread t = new Thread(p.Reader);
            Thread t2 = new Thread(p.Reader2);
            StreamWriter writer = new StreamWriter("D:\\multiwriter.txt");
            Thread t3 = new Thread(()=> { p.write(writer, 0); });
            Thread t4 = new Thread(() => { p.write(writer, 1); });
            Thread t5 = new Thread(() => { p.write(writer, 2); });
            Thread t6 = new Thread(() => { p.write(writer, 3); });
            t3.Name = "Bot1";
            t4.Name = "Bot2";
            t5.Name = "Bot3";
            t6.Name = "Bot4";
            t3.Start();
            t4.Start();
            t5.Start();
            t6.Start();
            t.Start(r);
            t2.Start(r2);
            t.Join();
            t2.Join();
            lock (Lock)
            {
                writehandle.Set();
                p.linelist.Add("");
                writehandle.Set();
                p.linelist.Add("");
                writehandle.Set();
                p.linelist.Add("");
                writehandle.Set();
                p.linelist.Add("");
            }
            t3.Join();
            t4.Join();
            t5.Join();
            t6.Join();
            //StreamWriter write = new StreamWriter("D:\\combinewrite.txt");
            //write.Write(Program.line);
            writer.Close();
            r.Close();
            r2.Close();
            
            //Console.Write(Program.line);
            Console.ReadKey();

        }
        void Reader(object f)
        {
            StreamReader file = (StreamReader)f;
            string firstp = "";
            int c = 0;

            while((firstp=file.ReadLine())!=null)
            {
                handle2.WaitOne();
                lock (Lock)
                {
                    Console.WriteLine("Inside lock of reader ");
                    Program.line += firstp;
                    firstp = "";
                    handle.Set();
                    Console.WriteLine("Leaving lock of reader ");
                }
                //c = file.Read();
                //if (c != -1)
                //{
                //    if (c != '\n')
                //        firstp += (char)c;
                //    else
                //    {
                //        handle2.WaitOne();
                //        lock (Lock)
                //        {
                //            Console.WriteLine("Inside lock of reader ");
                //            Program.line += firstp;
                //            firstp = "";
                //            handle.Set();
                //            Console.WriteLine("Leaving lock of reader ");
                //        }
                //    }
                //}
        }//while (c != -1);
        }
        void Reader2(object f)
        {
            StreamReader file = (StreamReader)f;
            string secondp = "";
            int c = 0;
            while ((secondp = file.ReadLine()) != null)
            {
                handle.WaitOne();
                lock (Lock)
                {
                    Console.WriteLine("Inside lock of reader2 ");
                    Program.line += secondp;
                    linelist.Add(line);
                    line = "";
                    secondp = "";
                    handle2.Set();
                    //for(int i=0;i<line.Length;i++)
                    //Console.Write(line[i]);
                    writehandle.Set();
                    Console.WriteLine("leaving lock of reader2 ");
                }
                //c = file.Read();
                //if (c != -1)
                //{
                //    if (c != '\n')
                //        secondp += (char)c;
                //    else
                //    {
                //        handle.WaitOne();
                //        lock (Lock)
                //        {
                //            Console.WriteLine("Inside lock of reader2 ");                                                       
                //            Program.line += secondp;
                //            linelist.Add(line);
                //            line = "";
                //            secondp = "";
                //            handle2.Set();
                //            //for(int i=0;i<line.Length;i++)
                //            //Console.Write(line[i]);
                //            writehandle.Set();
                //            Console.WriteLine("leaving lock of reader2 ");
                //        }
                //    }
                //}
            }//while (c != -1);
             //lock(Lock)
             //    {
             //    Console.WriteLine(line);
             //}

        } 
        void write(object o,int p)
        {
            StreamWriter write = (StreamWriter)o;
            int f = 0;
            while(true)
            {
                writehandle.WaitOne();
                lock(Lock)
                {
                    if(linelist[index]!="")
                    {
                        Console.WriteLine("Line " + index + " written by worker number " + p + " named " + Thread.CurrentThread.Name);
                        write.WriteLine(linelist[index] + Environment.NewLine);
                        index++;
                    }
                    else
                    {
                        Console.WriteLine(Thread.CurrentThread.Name + " is quiting");
                        f = 1;
                        index++;
                    }
                }
                if (f == 1)
                    break;                
            }
        }
    }
}
