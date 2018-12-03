#include <iostream>

#include "dllMain.h"
#include <Logging/Logger.h>

#if defined WIN32
	#include <xerces-c-3.1.1/src/xercesc/util/PlatformUtils.hpp>
	#include <xerces-c-3.1.1/src/xercesc/parsers/XercesDOMParser.hpp>
	#include <xerces-c-3.1.1/src/xercesc/dom/DOM.hpp>
	#include <xerces-c-3.1.1/src/xercesc/sax/HandlerBase.hpp>
	#include <xerces-c-3.1.1/src/xercesc/util/XMLString.hpp>
	#include <xerces-c-3.1.1/src/xercesc/util/PlatformUtils.hpp>
#elif defined linux
	#include "xerces-c-3.1.1/src/xercesc/util/PlatformUtils.hpp"
	#include "xerces-c-3.1.1/src/xercesc/parsers/XercesDOMParser.hpp"
	#include "xerces-c-3.1.1/src/xercesc/dom/DOM.hpp"
	#include "xerces-c-3.1.1/src/xercesc/sax/HandlerBase.hpp"
	#include "xerces-c-3.1.1/src/xercesc/util/XMLString.hpp"
	#include "xerces-c-3.1.1/src/xercesc/util/PlatformUtils.hpp"
#endif

#pragma once

using namespace std;

namespace utilities
{
	class UtilAPI LoadXML
	{
		char* m_FileName;
		
	public:
		LoadXML(void);
		~LoadXML(void);
		// Loads the given xml file into memory.
		int loadXML(char* xmlFile);
		static string getTagContent(string tag, xercesc::DOMDocument* data);
        static string getTagContent(string tag, xercesc::DOMElement* data);
    };
}