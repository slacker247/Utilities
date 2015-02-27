
// included by all classes

#pragma once

#if defined(WIN32) && !defined(STATIC_LIB)
	#if defined(UtilLIBRARY_EXPORT) || defined(UTILITIES_EXPORTS) // inside DLL
    #   define UtilAPI   __declspec(dllexport)
    #else // outside DLL
    #   define UtilAPI   __declspec(dllimport)
    #endif  // UtilLIBRARY_EXPORT
	#ifndef CDECL
		#define CDECL __cdecl
	#endif
#else
    #define UtilAPI
	#ifndef CDECL
		#define CDECL
	#endif
#endif
