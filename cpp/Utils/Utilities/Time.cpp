#include "Time.h"

namespace utilities
{
	const int Time::SECOND = 1000;
	const int Time::MINUTE = 60 * SECOND;
	const int Time::HOUR = 60 * MINUTE;
	const int Time::DAY = 24 * HOUR;

	Time::Time(void)
	{
	}

	Time::~Time(void)
	{
	}

	void Time::sleep(int ms)
	{
#ifdef __linux__
		usleep(ms * 1000);
#else
		Sleep(ms);
#endif
	}

	INT64 Time::getTimeMs64()
	{
	#ifdef WIN32
		/* Windows */
		FILETIME ft;
		LARGE_INTEGER li;

		/* Get the amount of 100 nano seconds intervals elapsed since January 1, 1601 (UTC) and copy it
		* to a LARGE_INTEGER structure. */
		GetSystemTimeAsFileTime(&ft);
		li.LowPart = ft.dwLowDateTime;
		li.HighPart = ft.dwHighDateTime;

		UINT64 ret = li.QuadPart;
		ret -= 116444736000000000LL; /* Convert from file time to UNIX epoch time. */
		ret /= 10000; /* From 100 nano seconds (10^-7) to 1 millisecond (10^-3) intervals */

		return ret;
	#else
		/* Linux */
		struct timeval tv;

		gettimeofday(&tv, NULL);

		uint64 ret = tv.tv_usec;
		/* Convert from micro seconds (10^-6) to milliseconds (10^-3) */
		ret /= 1000;

		/* Adds the seconds (10^0) after converting them to milliseconds (10^-3) */
		ret += (tv.tv_sec * 1000);

		return ret;
	#endif
	}

	String Time::toString(INT64 ms)
	{
		char buff[255];
		INT64 tmp = 0;
		buff[0] = '\0';

		String text = newString("");
		if (ms > DAY)
		{
			tmp = ms / DAY;
			_itoa((int)tmp, buff, 10);
			text.append(newString(buff));
			text.append(newString("d "));
			ms %= DAY;
		}
		if (ms > HOUR)
		{
			tmp = (ms / HOUR);
			_itoa((int)tmp, buff, 10);
			text.append(newString(buff));
			text.append(newString("h "));
			ms %= HOUR;
		}
		if (ms > MINUTE)
		{
			tmp = (ms / MINUTE);
			_itoa((int)tmp, buff, 10);
			text.append(newString(buff));
			text.append(newString("m "));
			ms %= MINUTE;
		}
		if (ms > SECOND)
		{
			tmp = (ms / SECOND);
			_itoa((int)tmp, buff, 10);
			text.append(newString(buff));
			text.append(newString("s "));
			ms %= SECOND;
		}
		_itoa((int)ms, buff, 10);
		text.append(newString(buff));
		text.append(newString("ms"));
		return text;
	}
}