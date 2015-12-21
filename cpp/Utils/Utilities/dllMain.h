
// included by all classes

#pragma once

#ifdef WIN32
    #if defined(UtilLIBRARY_EXPORT) // inside DLL
    #   define UtilAPI   __declspec(dllexport)
    #else // outside DLL
    #   define UtilAPI   __declspec(dllimport)
    #endif  // UtilLIBRARY_EXPORT
#else
    #define UtilAPI
#endif
