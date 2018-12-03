#pragma once

#include "dllMain.h"

#include "Options/anyoption.h"
#include <sys/stat.h> 
#include <cstring>
#include <string>
#include <vector>

using namespace std;

namespace utilities
{
	class UtilAPI Util
	{
	public:
		Util(void);
		~Util(void);
		static int Tokenize(const string& str, vector<string>& tokens, const char delimiter);
		static bool FileExists(char* strFilename);
		static int trimWhiteSpace(char* s);
		static int split(char* str, char delim, vector<char*>* results);
		static string replaceString(string str, string search, string replace);
		static int parseArgs(int argc, char* argv[], char*& fileName);
	};
}