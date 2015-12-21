#ifdef WIN32
#include <Windows.h>
#else
#include <sys/time.h>
#include <ctime>
#endif

#include "dllMain.h"
#include "String.h"

#pragma once
namespace utilities
{
	class UtilAPI Time
	{
	protected:
		static const int SECOND;
		static const int MINUTE;
		static const int HOUR;
		static const int DAY;
	public:
		Time(void);
		~Time(void);

		static INT64 getTimeMs64();
		static String toString(INT64 inTime);
	};
}