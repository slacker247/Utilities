using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities.threading
{
    class TaskManager
    {
        static List<Task> Tasks = new List<Task>();
        static int TotalTasks = 0;

        public static void WaitAll()
        {
            while(Tasks.Count > 0)
                Thread.Sleep(120);
        }

        public static void AddTask(Task t)
        {
            TotalTasks++;
            lock(Tasks)
                Tasks.Add(t);
        }

        public static void run(CancellationToken token)
        {
            int completed = 0;
            int faulted = 0;
            DateTime lastUpdate = DateTime.Now;
            while(!token.IsCancellationRequested)
            {
                int queued = 0;
                int running = 0;
                DateTime timeout = DateTime.Now;
                for(int i = 0;
                    i < Tasks.Count &&
                    (DateTime.Now - timeout < TimeSpan.FromSeconds(10));
                    i++)
                {
                    if (Tasks[i] != null)
                    {
                        if (Tasks[i].Status == TaskStatus.WaitingToRun ||
                            Tasks[i].Status == TaskStatus.Created ||
                            Tasks[i].Status == TaskStatus.WaitingForActivation ||
                            Tasks[i].Status == TaskStatus.WaitingForChildrenToComplete)
                            queued++;
                        else if (Tasks[i].Status == TaskStatus.Running)
                            running++;
                        else if(Tasks[i].Status == TaskStatus.Faulted)
                        {
                            faulted++;
                            Utilities.Logger.log(Tasks[i].Exception.Message, Utilities.MessageSeverity.ERROR);
                            Utilities.Logger.log(Tasks[i].Exception.StackTrace, Utilities.MessageSeverity.ERROR);
                            if (Tasks[i].Exception.InnerException != null)
                            {
                                Utilities.Logger.log(Tasks[i].Exception.InnerException.Message, Utilities.MessageSeverity.ERROR);
                                Utilities.Logger.log(Tasks[i].Exception.InnerException.StackTrace, Utilities.MessageSeverity.ERROR);
                            }
                            lock(Tasks)
                                Tasks.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            completed++;
                            lock(Tasks)
                                Tasks.RemoveAt(i);
                            i--;
                        }
                    }
                    else
                    {
                        completed++;
                        lock(Tasks)
                            Tasks.RemoveAt(i);
                        i--;
                    }
                }

                if(DateTime.Now - lastUpdate > TimeSpan.FromSeconds(3))
                {
                    Console.WriteLine("Added:     " + TotalTasks);
                    Console.WriteLine("Queued:    " + queued);
                    Console.WriteLine("Running:   " + running);
                    Console.WriteLine("Completed: " + completed);
                    Console.WriteLine("Faulted:   " + faulted);
                    TotalTasks = 0;
                    completed = 0;
                    faulted = 0;
                    lastUpdate = DateTime.Now;
                }
                Thread.Sleep(12);
            }
        }
    }
}
