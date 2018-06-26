using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//An example of the Reader-Writer scenario using reader-writer locks. Multiple writers are continually appending Key-Value pairs to a file.
//Multiple reader threads are simultaneously scanning the file. each reader is tasked with searching for a specific key & at the end must return
//the latest value associated with the key.

    /* Note- that there seems to be a glitch in the reader sections. the values returned are not always the latest.Need to investigate */

namespace Threading2
{
    class Program
    {
        static readonly object Lock = new object();
        static string[] keys = { "Alpha", "Beta", "Gama", "Delta", "Capa", "Epsilon", "Phi", "Rho" };
        static EventWaitHandle handle = new ManualResetEvent(false);
        static ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim();
        static EventWaitHandle endwritehandle = new ManualResetEvent(false);
        static int readercount = 0;
        static int writercount = 0;
        static int linecount = 0;
        static int flag=0;
        static EventWaitHandle writehandle = new AutoResetEvent(false);
        static EventWaitHandle handle2 = new AutoResetEvent(true);
        static string line = "";
        List<string> linelist = new List<string>();
        static int index = 0;
        static void Main(string[] args)
        {
            Program p = new Program();
            //var f = File.Open("D:\\sampletext.txt",FileMode.Open);
            FileStream wstream= new FileStream("D:\\Documents\\reader-writer12.txt", FileMode.Append,
                               FileAccess.Write, FileShare.ReadWrite);
            //FileStream rstream = new FileStream("D:\\Documents\\reader-writer7.txt", FileMode.Open,
            //                   FileAccess.Read, FileShare.ReadWrite);
            StreamWriter writer = new StreamWriter(wstream);
            //StreamReader r = new StreamReader(rstream);
            string writepath = "D:\\Documents\\reader-writer12.txt";
            string readpath = "D:\\Documents\\reader-writer12.txt";
            CancellationTokenSource tokensource1 = new CancellationTokenSource();
            CancellationTokenSource tokensource2 = new CancellationTokenSource();
            Thread t = new Thread(() => { p.Reader(readpath, "Alpha"); });
            Thread t2 = new Thread(() => { p.Reader(readpath, "Phi"); });
            Thread t3 = new Thread(() => { p.Reader(readpath, "Delta"); });
            Thread t4 = new Thread(() => { p.Reader(readpath, "Capa"); });
            Thread t5 = new Thread(() => { p.write(writer, 5000000, tokensource1.Token); });
            Thread t6 = new Thread(() => { p.write(writer, 3000000, tokensource2.Token); });
            t.Name = "reader1";
            t2.Name = "reader2";
            t3.Name = "reader3";
            t4.Name = "reader4";
            t5.Name = "writer1";
            t6.Name = "writer2";

            t.Start();
            t2.Start();
            t3.Start();
            t4.Start();
            t5.Start();
            t6.Start();
            Thread.Sleep(300000);
            rwlock.EnterWriteLock();
            if (tokensource1.Token.CanBeCanceled)
                tokensource1.Cancel();
            if (tokensource2.Token.CanBeCanceled)
                tokensource2.Cancel();
            rwlock.ExitWriteLock();
            t.Join();
            t2.Join();
            
            lock (Lock)
            {
                flag = 1;
                writehandle.Set();
                p.linelist.Add("TERMINATE");
                writehandle.Set();
                p.linelist.Add("TERMINATE");
            }
            t3.Join();
            t4.Join();
            t5.Join();
            t6.Join();
            
            //writer.Close();
            //r.Close();

            //Console.Write(Program.line);
            Console.WriteLine("Total number of Key:Value pairs written to file= " + linecount);
            Console.ReadKey();

        }
        void readerregister()
        {
            readercount++;
            Console.WriteLine("reader registering. current reader count= " + readercount);
        }
        void readerderegister(int Count)
        {
            readercount--;
            Console.WriteLine("This reader has read " + Count + " lines so far");
            Console.WriteLine("reader deregistering. current reader count= " + readercount);
        }
        void Reader(object f,string search)
        {

            FileStream rstream = new FileStream((string)f, FileMode.OpenOrCreate,
                               FileAccess.Read, FileShare.ReadWrite);
            StreamReader file = new StreamReader(rstream);
            string firstp = "";
            int c = 0;
            string output = "";
            Thread t = Thread.CurrentThread;
            int Count = 0;
            while (true)//(firstp = file.ReadLine()) != null)
            {                
                rwlock.EnterReadLock();
                Console.Write("Entered read region in " + t.Name + ". Lines read in this thread= " + Count+" .Current number of active writers= "+(2-writercount)+". Total number of lines written= "+linecount);// rwlock.CurrentReadCount);
                firstp = file.ReadLine();
                if(Count>=linecount&&writercount>=2)
                {
                    Console.WriteLine(t.Name + " exiting having read= "+Count+" lines.");
                    rwlock.ExitReadLock();
                    break;
                }
                else if(writercount>=2)
                {
                    //Console.WriteLine("Lines read in " + t.Name + " till now= " + Count);
                }
                if (firstp != ""&&firstp!=null)
                {
                    string[] pair = firstp.Split(':');
                    if (pair.Length == 3 && pair[0] == search)
                        output = "From worker " + t.Name + " :- Latest value of key " + search + ": " + pair[1];
                    Count++;                  
                }
                rwlock.ExitReadLock();
            }
            lock(Lock)
                Console.WriteLine("Output from "+t.Name+" for key "+search+" = "+output);
            //file.Close();
        }
   
        void write(object o, int p,CancellationToken token)
        {
            //FileStream wstream = new FileStream((string)o, FileMode.Append,
            //                   FileAccess.Write, FileShare.ReadWrite);
            StreamWriter write = (StreamWriter)o;
            int f = 0;
            Random r = new Random(System.DateTime.UtcNow.Millisecond);
            int zc = 0;
            int lc = 0;
            Thread t = Thread.CurrentThread;
            while (true)
            {                
                rwlock.EnterWriteLock();
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine(t.Name + " is quiting because cancellation was requested. It has written "+lc+" key values till date");
                    //write.WriteLine("TERMINATE" + Environment.NewLine);
                    writercount++;
                    Console.WriteLine("Leaving writer lock on " + t.Name);
                    if (writercount == 2)
                        write.Close();
                    rwlock.ExitWriteLock();
                    return;
                }
                Console.WriteLine("\n Entered writer lock in "+p+"th writer");
                int key_pos = r.Next() % keys.Length;
                int value = r.Next();
                if (lc < p)
                {
                    //Console.WriteLine("Line " + index + " written by worker number " + p + " named " + Thread.CurrentThread.Name);
                    string output = "" + keys[key_pos] + ":" + value.ToString() + ":Written by worker " + t.Name;
                    lc++;
                    linecount++;
                    write.WriteLine(output);
                    //Thread.Sleep(50);
                }
                else
                {
                    Console.WriteLine(t.Name + " is quiting");
                    //write.WriteLine("TERMINATE" + Environment.NewLine);
                    writercount++;
                    Console.WriteLine("Leaving writer lock on " + t.Name);
                    if(writercount==2)
                    write.Close();
                    rwlock.ExitWriteLock();
                    return;
                }
                handle.Set();
                //writercount--;
                Console.WriteLine("Leaving writer lock on " + p + "th writer");
                rwlock.ExitWriteLock();
                Thread.Sleep(5);
            }
        }
    }
}
