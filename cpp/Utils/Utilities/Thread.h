
#ifdef WIN32
#include <windows.h>
#include <process.h>
#elif __linux__
#include <pthread.h>
#include <signal.h>
#include <unistd.h>
#include <errno.h>
#endif

#include "dllMain.h"
#include "String.h"

// http://www.codeproject.com/Articles/16134/Calling-a-non-static-member-function-as-a-thread-f
#pragma once
namespace utilities
{
	class UtilAPI Thread
	{
	public:
		Thread(void*( UtilAPI *func )( void ));
		Thread(void*( UtilAPI *func )( void * ), void* classPtr);
		~Thread(void);
		
		static void sleep(long ms);
		int setName(String* name);
		int lock();
		int unlock();
		int stop();
		int suspend();
		int resume();
		bool isAlive();
		void* m_ExeFunc;
	protected:
		static void* UtilAPI run(void * param);

		/////////////////////////////////////////////////////////
		/// Variable: m_ThreadID
		///
		/// <summary>The id to manage this thread.</summary>
		/////////////////////////////////////////////////////////
		unsigned long m_ThreadID;
                String m_Name;
		/////////////////////////////////////////////////////////
		/// Variable: m_ThreadHandle
		///
		/// <summary>The handle to the thread.</summary>
		/////////////////////////////////////////////////////////
#ifdef WIN32
		void* m_ThreadHandle;
		void* m_MutexHandle;
#elif __linux__
                pthread_t m_ThreadHandle;
                pthread_mutex_t m_MutexHandle;
                bool m_Suspend;
                pthread_cond_t m_ResumeCond;
#endif
	};
}
