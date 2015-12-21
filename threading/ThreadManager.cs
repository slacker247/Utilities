using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections;


namespace Utilities.threading
{
	/// <summary>
	/// This class handles all thread creation, scheduling, and callbacks for the 
	/// collection process. In Addition, this class causes updates to the user's 
	/// history table in the user_db with progress/status of the collection.
    /// 
    /// debugging:
    /// http://msdn.microsoft.com/en-us/magazine/cc163528.aspx
    /// http://stackoverflow.com/questions/4373683/unable-to-load-sos-in-windbg
	/// </summary>
	public class ThreadManager
	{
		//These class members are used for data management between multiple threads
        volatile List<BaseWorker> m_ThreadQueue;
        volatile List<BaseWorker> m_DeadThreads = new List<BaseWorker>();
        volatile Dictionary<string, BaseWorker> m_ActiveThreads;
		private System.Object m_Lock = new System.Object();
        protected Dictionary<long, long> m_CancelThreads = new Dictionary<long,long>();

		//used for progress tracking
		volatile int m_TotalThreads;
		volatile int m_ActiveSeedThreads;

        /// <summary>
        /// The max user allowed threads to run concurrently.
        /// </summary>
        public int MaxThreads { get; set; }
		/// <summary>
		/// Var used to manage the System limit on threads
		/// </summary>
		private int m_MaxThreads;

        protected bool m_Run = true;

        protected String m_InstanceName = "Manager_";
        protected static int m_InstanceCount = 0;

        public enum ThreadProcessState
        {
            ADDED,
            STARTED,
            COMPLETED
        };

		#region Constructors
		/// <summary>
		/// Default constructor.
		/// </summary>
		public ThreadManager()
		{
			//get the number of logical processors aka max threads
            m_MaxThreads = Environment.ProcessorCount + 1;
            m_ThreadQueue = new List<BaseWorker>();
            m_ActiveThreads = new Dictionary<string, BaseWorker>();
			m_TotalThreads = 0;
			m_ActiveSeedThreads = 1;
            m_InstanceCount++;
            m_InstanceName += m_InstanceCount.ToString();
		}
		#endregion

        public int getActiveThreads()
        {
            return m_ActiveThreads.Count;
        }

        public int getQueuedThreads()
        {
            return m_ThreadQueue.Count;
        }

        public void stop()
        {
            m_Run = false;
            int i = 0;
            while(m_DeadThreads.Count > 0)
            {
                if (m_DeadThreads[i].Join(150))
                {
                    m_DeadThreads.RemoveAt(i);
                    i--;
                }
                i++;
            }
        }

        // TODO : implement
        public void stopThreads(long uid, long hid)
        {
            throw new NotImplementedException("stopThreads is not impelemented.");
            m_CancelThreads[uid] = hid;
        }

		/// <summary>
		/// This function will set the numer of initial threads needed by the collector.
		/// </summary>
		/// <param name="count">The number of initial theads.</param>
		public void setSeedThreads(int count)
		{
			m_ActiveSeedThreads = count;
			m_TotalThreads += count;
		}

		/// <summary>
		/// This function serves as the master run thread loop.
		/// </summary>
		public void runThreadsLoop()
		{
            int count = 0;
            bool oneTime = true;
            DateTime idleTime = DateTime.Now;
            while (m_Run)
			{
                // Pause all requested cancel threads
                for (int i = 0; i < m_CancelThreads.Keys.Count; i++ )
                {
                    long uid = m_CancelThreads.Keys.ElementAt<long>(i);
                    long hid = m_CancelThreads[uid];
                    foreach(BaseWorker th in m_ActiveThreads.Values)
                    {
                        if (false)
                        {
                            th.Stop();
                        }
                    }
                    for (int n = 0; n < m_ThreadQueue.Count; n++ )
                    {
                        BaseWorker th = m_ThreadQueue[n];
                        if (false)
                        {
                            m_ThreadQueue.RemoveAt(n);
                            n--;
                        }
                    }
                    m_CancelThreads.Remove(uid);
                }

                Process mainProc = System.Diagnostics.Process.GetCurrentProcess();
                if ((m_ActiveThreads.Count() < m_MaxThreads) && (m_ThreadQueue.Count > 0)
                   // && mainProc.PrivateMemorySize64 < 701001000L
                   // && mainProc.HandleCount < 4000
                    )
				{
                    lock (m_Lock)
                    {
                        DateTime start = DateTime.Now;
                        DateTime past = DateTime.Now;
                        TimeSpan timeout = new TimeSpan(0, 0, 2);
                        while(m_ActiveThreads.Count() < m_MaxThreads &&
                            m_ThreadQueue.Count() > 0 &&
                            past - start < timeout)
                        {
                            //pop a thread and start, and increment active threads
                            BaseWorker popThread = m_ThreadQueue[0];
                            m_ThreadQueue.RemoveAt(0);
                            if (popThread != null &&
                                !m_ActiveThreads.ContainsKey(popThread.Name))
                            {
                                m_ActiveThreads[popThread.Name] = popThread;
                                if (popThread.ThreadState == System.Threading.ThreadState.Unstarted)
                                {
                                    popThread.Start();
                                }
                            }
                            past = DateTime.Now;
                        }
                    }
				}

				float cpuUsage = Utilities.system.SystemResources.getCPUCounter();
				if (cpuUsage < 80f)
				{
                    if (m_MaxThreads < MaxThreads)
						m_MaxThreads++;
				}
				else
					m_MaxThreads = Environment.ProcessorCount + 1;

				if (m_MaxThreads < Environment.ProcessorCount + 1)
					m_MaxThreads = Environment.ProcessorCount + 1;
                
				int totalThreadCount = (m_ThreadQueue.Count + m_ActiveThreads.Count + m_DeadThreads.Count);

                if (totalThreadCount == 0 && oneTime)
                {
                    Console.WriteLine("\nIdle...");
                    if (mainProc.HandleCount > 2000)
                        Environment.FailFast("Releasing Resources");
                    oneTime = false;
                }

                if (totalThreadCount > 0)
                {
                    oneTime = true;
                    idleTime = DateTime.Now;
                }
                else
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    //if (DateTime.Now - idleTime > new TimeSpan(0, 2, 0) &&
                    //   mainProc.HandleCount > 1000)
                    //{
                    //    m_Run = false;
                    //}
                }
                
                count++;
                if (count > 7 && (m_DeadThreads.Count > 0 || m_ActiveThreads.Count > 0))
                {
                    for (int i = 0; i < m_DeadThreads.Count; i++)
                    {
                        if (m_DeadThreads[i] == null ||
                            m_DeadThreads[i].Join(150))
                        {
                            //m_DeadThreads[i] = null;
                            m_DeadThreads.RemoveAt(i);
                            Console.Write(".");
                            i--;
                        }
                    }
                    count = 0;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
//#if DEBUG
                    Console.WriteLine(m_InstanceName + "\n" +
                                      "Total Threads: " + totalThreadCount +
                                      "\nQueued Threads: " + m_ThreadQueue.Count +
                                      "\nActive Threads: " + m_ActiveThreads.Count +
                                      "\nDead Threads: " + m_DeadThreads.Count +
                                      "\nMax Threads: " + m_MaxThreads);
//#endif
                }
                else
                {
                    if (m_ActiveThreads.Count > m_MaxThreads)
                        Thread.Sleep(120);
                    //else
                    //    Thread.Sleep(300);
                }
			}

			//update history to complete
			Console.WriteLine("All threads complete, press any key to continue.");
		}

        /// <summary>
        /// This function will take in a worker instance object, a callback handle, and 
        ///  a social media type.  Then it will create and queue a thread for running.
        /// </summary>
        /// <param name="worker">The worker instance object.</param>
        public void createQueueThread(BaseWorker worker)
        {
            //create worker thread
            m_TotalThreads++;
            worker.queueCallback += new BaseWorker.queueCallbackDelegate(queueCallback);
            worker.threadCallback += new BaseWorker.threadCallbackDelegate(threadCallback);
            worker.progressCallback += new BaseWorker.progressCallbackDelegate(progressCallback);
            worker.m_SeedThread = true;

            worker.createThread(ref worker);

            lock (m_Lock)
            {
                //queue thread
                m_ThreadQueue.Add(worker);
            }
        }

		/// <summary>
		/// This function serves as a callback method for queueing threads and
		///  when they complete.
		/// </summary>
		/// <param name="thread">A handle to the worker thread.</param>
		void queueCallback(BaseWorker thread)
		{
			lock (m_Lock)
			{
				//if worker isn't null then add thread to queue
                if (thread != null)
                {
                    m_ThreadQueue.Add(thread);
                }
			}
		}

		/// <summary>
		/// This function serves as a callback method for queueing threads and
		///  when they complete.
		/// </summary>
		/// <param name="name">The name of the thread that is finished working.</param>
		void threadCallback(string name = "")
		{
            lock (m_Lock)
            {
                if (name != null)
                {
                    if (m_ActiveThreads.ContainsKey(name))
                    {
                        //decrement active threads and cleanup
                        BaseWorker finishedThread = m_ActiveThreads[name];

                        m_ActiveThreads.Remove(name);
                        m_DeadThreads.Add(finishedThread);
                    }
                    else
                        Console.WriteLine("Error: No Active threads with the name: " + name);
                }
                else
                    Console.WriteLine("Error: Thread name is not valid.");
            }
        }

		/// <summary>
		/// This function serves as a callback method for tracking completion progress
		/// </summary>
		/// <param name="mainReturnCnt">The number of threads that will be returned by a starting
		/// thread. Only sent by an initial thread and used for progress tracking.</param>
		/// <param name="isSeedThread">Switch stating if thread is a seed thread.</param>
		void progressCallback(long id, ThreadProcessState state)
		{
			lock (m_Lock)
			{
                //if (m_UserHistoryItems.ContainsKey(id))
                //{
                //    History history = m_UserHistoryItems[id];
                //    History dbHistory = new History(history.getId(), history.getUserId());
                //    HistoryParametersBase parameters = dbHistory.getParameters();
                //    if (parameters.getErrorCode() == History.ErrorType.Canceled)
                //        this.stopThreads(dbHistory.getUserId(), dbHistory.getId());
                //    String status = "";
                //    if (state == ThreadProcessState.ADDED)
                //    {
                //        // need to increase when we add the thread.  otherwise we may complete the thread
                //        // before the spawned threads start and in that case we'll report that we're complete 
                //        // too early.
                //        long cnt = parameters.getTotalThreads() ?? 0;
                //        cnt++;
                //        parameters.setTotalThreads(cnt);
                //        status = History.STATUS_INPROGRESS;
                //    }
                //    else if (state == ThreadProcessState.STARTED)
                //    {
                //        // just update the last updated time (to show progress)
                //        parameters.setLastProgressUpdate();
                //    }
                //    else if (state == ThreadProcessState.COMPLETED)
                //    {
                //        long cnt = parameters.getCompletedThreads() ?? 0;
                //        cnt++;
                //        parameters.setCompletedThreads(cnt);

                //        // TODO - We should really be paying attention to changes to the history status here
                //        // but we're caching the history item in the dictionary.
                //        if (cnt >= parameters.getTotalThreads() && dbHistory.getStatus() != History.STATUS_ERROR)
                //        {
                //            status = History.STATUS_COMPLETE;
                //            parameters.setEndTime(DateTime.UtcNow);
                //        }
                //    }
                //    if (!String.IsNullOrEmpty(status) &&
                //        parameters.getErrorCode() != History.ErrorType.Canceled)
                //    {
                //        dbHistory.setStatus(status);
                //    }
                //    if(parameters.getErrorCode() != History.ErrorType.Canceled)
                //        parameters.setErrorCode(History.ErrorType.None);
                //    dbHistory.setParameters(parameters);
                //    dbHistory.save();
                //    String historyStatus = dbHistory.getStatus();
                //    if (historyStatus == History.STATUS_COMPLETE || historyStatus == History.STATUS_ERROR)
                //        m_UserHistoryItems.Remove(id);
                //}
			}
		}
	}
}
