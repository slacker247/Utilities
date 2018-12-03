
#include "String.h"

namespace utilities
{
	// NOT THREAD SAFE!!!
	String newString(const char* s)
	{
		String ws;
#ifdef WIN32
		int length = std::strlen(s);
		if(length > 0)
		{
			wchar_t* buf = new wchar_t[ length ];
			size_t num_chars = mbstowcs( buf, s, length );
			ws.assign( buf, num_chars );
			delete buf;
		}
		else
			ws.assign(L"");
#else
                ws.append(s);
#endif
		return ws;
	}

	int cmpStrI(const String& stringA , const String& stringB)
	{
		int status = 0;
#ifdef WIN32
		status = _wcsicmp(stringA.c_str(), stringB.c_str());
#else
                status = std::strcmp(stringA.c_str(), stringB.c_str());
#endif
		return status;
	}

	double strToDbl(const String& value)
	{
		double retVal = 0;
		try
		{
#if (__cplusplus > 199711L)
			retVal = std::stod(value);
#else
			retVal = atof(value.c_str());
#endif
		}
		catch(...)
		{
		}
		return retVal;
	}

	String dblToStr(double value)
	{
		String retVal = newString("");
		try
		{
			char buf[255];
			buf[0] = '\0';
			sprintf(buf, "%f", value);
			retVal = newString(buf);
		}
		catch(...)
		{
		}
		return retVal;
	}

	const String WHITESPACE = newString(" \n\r\t");

	String strTrim(const String& s)
	{
		String retVal;
		bool test = false;
		for(int i = 0; i < WHITESPACE.length(); i++)
		{
			if(s.find(WHITESPACE[i]) != String::npos)
			{
				test = true;
				i = 10;
			}
		}
		if(test)
		{
			retVal = strTrimLeft(s);
			retVal = strTrimRight(retVal);
		}
		else
			retVal = s;
		return retVal;
	}

	String strTrimLeft(const String& s)
	{
		String retVal;
		size_t startpos = s.find_first_not_of(WHITESPACE);
		if(startpos != String::npos)
		{
			retVal = s.substr(startpos);
		}
		else
		{
			retVal = s;
		}
		return retVal;
	}

	String strTrimRight(const String& s)
	{
		String retVal;
		size_t endpos = s.find_last_not_of(WHITESPACE);
		if(endpos != String::npos)
		{
			retVal = s.substr(0, endpos+1);
		}
		else
		{
			retVal = s;
		}
		return retVal;
	}
        
	UtilAPI unsigned long hash(unsigned char *str)
	{
		unsigned long l_Hash = 5381;
		int c;

		while (c = *str++)
			l_Hash = ((l_Hash << 5) + l_Hash) + c; /* l_Hash * 33 + c */

		return l_Hash;
	}
}