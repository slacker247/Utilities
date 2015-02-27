#ifdef WIN32
#include <tchar.h>
#endif
#include <string>
#include <cstring>
#include <stdlib.h>
#include <stdio.h>

#include "dllMain.h"

#ifndef String

// HACK : to make a unified string class
#ifdef _UNICODE
typedef std::wstring String;
#else
typedef std::string String;
#endif
// END HACK

namespace utilities
{
	UtilAPI String newString(const char* s);
	UtilAPI int cmpStrI(const String& stringA , const String& stringB);
	UtilAPI double strToDbl(const String& value);
	UtilAPI String dblToStr(double value);

	UtilAPI String strTrim(const String& s);
	UtilAPI String strTrimLeft(const String& s);
	UtilAPI String strTrimRight(const String& s);

	// http://www.cse.yorku.ca/~oz/hash.html
	UtilAPI unsigned long hash(unsigned char *str);
}
#endif