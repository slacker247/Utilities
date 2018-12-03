#include "Thread.h"

namespace utilities
{
	Thread::Thread(void*( UtilAPI *func )( void ))
	{
            m_ExeFunc = (void*)func;
#ifdef WIN32
            m_MutexHandle = NULL;
            m_ThreadHandle = (void*)_beginthread(&(Thread::run), 0, this);
            this->m_ThreadID = GetThreadId(this->m_ThreadHandle);
#elif __linux__
            m_Suspend = false;
            pthread_create(&(this->m_ThreadHandle), NULL, Thread::run, this);
            this->m_ThreadID = (ulong)m_ThreadHandle;
#endif
	}
	
	Thread::Thread(void*( UtilAPI *func )( void * ), void* classPtr)
	{
            m_ExeFunc = (void*)func;
#ifdef WIN32
            m_MutexHandle = NULL;
            m_ThreadHandle = (void*)_beginthread(func, 0, classPtr);
            this->m_ThreadID = GetThreadId(this->m_ThreadHandle);
#elif __linux__
            m_Suspend = false;
            pthread_create(&(this->m_ThreadHandle), NULL, func, classPtr);
            this->m_ThreadID = (ulong)this->m_ThreadHandle;
#endif
	}

	Thread::~Thread(void)
	{
            int status = -1;
#ifdef WIN32
            TerminateThread(this->m_ThreadHandle, status);
#elif __linux__
            pthread_cancel(this->m_ThreadHandle);
#endif
	}

	int Thread::setName(String* name)
	{
            int status = -1;
            // http://stackoverflow.com/questions/16486937/name-a-thread-created-by-beginthread-in-c
            this->m_Name = *name;
            return status;
	}

	/////////////////////////////////////////////////////////
	/// Function: run
	///
	/// <summary>is the code to be executed.</summary>
	///
	/// <param>param: pointer to the class.</param>
	///
	/// <returns>returns: nothing </returns>
	/////////////////////////////////////////////////////////
	void* UtilAPI Thread::run(void * param)
	{
            //*(((Thread)param).m_ExeFunc)();

            //while(m_Suspend)
            //  pthread_cond_wait(this->m_ResumeCond, this->m_MutexHandle);
            return NULL;
	}

	/////////////////////////////////////////////////////////
	/// Function: lock
	///
	/// <summary>Locks a resource for this thread to use.</summary>
	///
	/// <returns>returns: 0 for a success. Otherwise the
	/// value will be in the lookup table: <see></see> </returns>
	/////////////////////////////////////////////////////////
	int Thread::lock()
	{
            int status = -1;
#ifdef WIN32
            m_MutexHandle = CreateMutex(NULL, false, NULL);
#elif __linux__
            pthread_mutex_init(&(this->m_MutexHandle), NULL);
            status = pthread_mutex_lock(&(this->m_MutexHandle));
#endif
            return status;
	}

	/////////////////////////////////////////////////////////
	/// Function: unlock
	///
	/// <summary>Unlocks a resource that this thread has locked.
	/// </summary>
	///
	/// <returns>returns: 0 for a success. Otherwise the
	/// value will be in the lookup table: <see></see> </returns>
	/////////////////////////////////////////////////////////
	int Thread::unlock()
	{
            int status = -1;
#ifdef WIN32
            if(m_MutexHandle != NULL)
                if(ReleaseMutex(m_MutexHandle))
                {
                    m_MutexHandle = NULL;
                    status = 0;
                }
#elif __linux__
                if(pthread_mutex_unlock(&(this->m_MutexHandle)) == 0)
                {
                    status = pthread_mutex_destroy(&(this->m_MutexHandle));
                }
#endif
            return status;
	}
	
	/////////////////////////////////////////////////////////
	/// Function: isAlive
	///
	/// <summary>Tells the caller if the thread is still active
	/// or not.</summary>
	///
	/// <returns>returns: true if the thread is still active
	/// and false if the thread is not active.</returns>
	/////////////////////////////////////////////////////////
	bool Thread::isAlive()
	{
            bool status = true;
#ifdef WIN32
            DWORD result = WaitForSingleObject(this->m_ThreadHandle, 10);
            switch(result)
            {
            case WAIT_ABANDONED:
                    status = false;
                    break;
            case WAIT_OBJECT_0:
                    status = false;
                    break;
            case WAIT_TIMEOUT:
                    break;
            case WAIT_FAILED:
                    DWORD err = GetLastError();
                    break;
            }
#elif __linux__
            int result = pthread_kill(this->m_ThreadHandle, 0);
            switch(result)
            {
                case ESRCH:
                    status = false;
                    break;
                case EINVAL:
                    status = false;
                    // error status
                    break;
            }
#endif
            return status;
	}

	/////////////////////////////////////////////////////////
	/// Function: stop
	///
	/// <summary>Stops the execution of the thread.  This fundtion
	/// will not return until the thread has stopped.</summary>
	///
	/// <returns>returns: 0 for a success. Otherwise the
	/// value will be in the lookup table: <see></see> </returns>
	/////////////////////////////////////////////////////////
	int Thread::stop()
	{
		int status = -1;
#ifdef WIN32
		_endthreadex(status);
#elif __linux__
                status = pthread_kill(this->m_ThreadHandle, SIGKILL);
#endif
		return status;
	}

	/////////////////////////////////////////////////////////
	/// Function: suspend
	///
	/// <summary>This will suspend the thread if it is not
	/// suspended.</summary>
	///
	/// <returns>returns: 0 for a success. Otherwise the
	/// value will be in the lookup table: <see></see> </returns>
	/////////////////////////////////////////////////////////
	int Thread::suspend()
	{
		int status = -1;
#ifdef WIN32
		SuspendThread(this->m_ThreadHandle);
#elif __linux__
                lock();
                m_Suspend = true;
                unlock();
#endif
		return status;
	}

	/////////////////////////////////////////////////////////
	/// Function: resume
	///
	/// <summary>If the thread is suspended, this function will
	/// resume the thread.</summary>
	///
	/// <returns>returns: 0 for a success. Otherwise the
	/// value will be in the lookup table: <see></see> </returns>
	/////////////////////////////////////////////////////////
	int Thread::resume()
	{
		int status = -1;
#ifdef WIN32
		while(ResumeThread(this->m_ThreadHandle));
#elif __linux__
                lock();
                m_Suspend = false;
                pthread_cond_broadcast(&(this->m_ResumeCond));
                unlock();
#endif
		return status;
	}

	void Thread::sleep(long ms)
	{
#ifdef WIN32
		Sleep(ms);
#elif __linux__
                usleep(ms);
#endif
	}
}