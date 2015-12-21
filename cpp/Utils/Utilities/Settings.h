#pragma once

#include <string>
#include <map>
#include <fstream>

#include "dllMain.h"
#include "Logging/Logger.h"
#include "Util.h"

using namespace std;

namespace utilities
{
	class UtilAPI Settings
	{
	public:
		Settings(void);
		Settings(const Settings& in);
		~Settings(void);
		int parseSettings(const char* fileName);
		char* getValue(char* tag);
	protected:
		/////////////////////////////////////////////////////////
		/// Variable: m_Params
		///
		/// <summary>The name/value pairs that will be parsed
		/// from the file passed in.</summary>
		/////////////////////////////////////////////////////////
		map<string, string> m_Params;
	};
}