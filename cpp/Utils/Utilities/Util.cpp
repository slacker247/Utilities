#include "Util.h"

namespace utilities
{
	Util::Util(void)
	{
	}

	Util::~Util(void)
	{
	}

	int Util::split(char* str, char delim, vector<char*>* results)
	{
		int status = -1;
		int start = 0;
		int end = 0;
		while(end < strlen(str))
		{
			if(str[end] == delim ||
			   end == strlen(str) - 1)
			{
				char* type = new char[end - start + 2];
				memset(type, 0, end - start + 2);
				for(int i = 0; i < end - start; i++)
				{
					type[i] = str[start+i];
				}
				results->push_back(type);
				start = end + 1;
			}
			end++;
		}

		//int cutAt;
		//while( (cutAt = str.find_first_of(delim)) != str.npos )
		//{
		//	if(cutAt > 0)
		//	{
		//		char* type = new char[cutAt + 1];
		//		memset(type, 0, cutAt + 1);
		//		strcpy(type, str.substr(0,cutAt).c_str());
		//		results->push_back(type);
		//	}
		//	str = str.substr(cutAt+1);
		//}
		//if(str.length() > 0)
		//{
		//	char* type = new char[str.length() + 1];
		//	memset(type, 0, str.length() + 1);
		//	strcpy(type, str.c_str());
		//	results->push_back(type);
		//}

		return status;
	}

	string Util::replaceString(string str, string search, string replace)
	{
		if(search != replace)
		{
			string::size_type pos = 0;
			while ( (pos = str.find(search, pos)) != string::npos ) {
				str.replace( pos, search.size(), replace );
				pos++;
			}
		}
		return str;
	}

	/////////////////////////////////////////////////////////
	/// Function: Tokenize
	///
	/// <summary>Splits up a string based on a delimiter.</summary>
	///
	/// <param>str: The string to split up.</param>
	/// <param>tokens: The results of the split.</param>
	/// <param>delimiter: Individual character to be searched for.</param>
	///
	/// <returns>returns: 0 for a success. Otherwise the
	/// value will be in the lookup table: <see></see> </returns>
	/////////////////////////////////////////////////////////
	int Util::Tokenize(const string& str, vector<string>& tokens, const char delimiter = ' ')
	{
		int status = -1;
		// Skip delimiters at beginning.
		string::size_type lastPos = str.find_first_not_of(delimiter, 0);
		// Find first "non-delimiter".
		string::size_type pos     = str.find_first_of(delimiter, lastPos);

		while (string::npos != pos || string::npos != lastPos)
		{
			// Found a token, add it to the vector.
			tokens.push_back(str.substr(lastPos, pos - lastPos));
			// Skip delimiters.  Note the "not_of"
			lastPos = str.find_first_not_of(delimiter, pos);
			// Find next "non-delimiter"
			pos = str.find_first_of(delimiter, lastPos);
		}
		return status;
	}

	/////////////////////////////////////////////////////////
	/// Function: FileExists
	///
	/// <summary>Check to see if a file exists.</summary>
	///
	/// <param>strFilename: The complete path and file name.</param>
	///
	/// <returns>returns: a boolean of true if the file exists
	/// and false if the file does not.</returns>
	/////////////////////////////////////////////////////////
	bool Util::FileExists(string strFilename)
	{
		struct stat stFileInfo; 
		bool blnReturn; 
		int intStat; 

		// Attempt to get the file attributes 
		intStat = stat(strFilename.c_str(),&stFileInfo); 
		if(intStat == 0)
		{ 
			// We were able to get the file attributes 
			// so the file obviously exists. 
			blnReturn = true; 
		}
		else
		{ 
			// We were not able to get the file attributes. 
			// This may mean that we don't have permission to 
			// access the folder which contains this file. If you 
			// need to do that level of checking, lookup the 
			// return values of stat which will give you 
			// more details on why stat failed. 
			blnReturn = false; 
		} 

		return(blnReturn); 
	}

	/////////////////////////////////////////////////////////
	/// Function: trimWhiteSpace
	///
	/// <summary>removes leading/trailing whitespace from
	/// the string that is passed in.</summary>
	///
	/// <param>s: is the manipulated string it is a ref.</param>
	///
	/// <returns>returns: 0 for a success. Otherwise the
	/// value will be in the lookup table: <see></see> </returns>
	/////////////////////////////////////////////////////////
	int Util::trimWhiteSpace(char* s)
	{
		int status = -1;
		char * p = s;
		int l = strlen(p);
		if(l > 0)
		{
			while(l > 0 && isspace(p[l - 1]))
				p[--l] = 0;
			while(* p && isspace(* p))
				++p, --l;
			memmove(s, p, l + 1);
			status = 0;
		}
		return status;
	}

	/////////////////////////////////////////////////////////
	/// Function: parseArgs
	///
	/// <summary></summary>
	///
	/// <param></param>
	///
	/// <returns>returns: 0 for a success. Otherwise the
	/// value will be in the lookup table: <see></see> </returns>
	/////////////////////////////////////////////////////////
	/* TODO : Move to the right location (static function in AnyOptions)
	int Util::parseArgs(int argc, char* argv[], char*& fileName)
	{
		int status = -1;
		// Setup argument parser
		AnyOption *opt = new AnyOption();

		opt->addUsage("");
		opt->addUsage("Usage: ");
		opt->addUsage("");
		opt->addUsage(" -h  --help  		Prints this help ");
		opt->addUsage(" -s  --settings  		The settings file ");
		opt->addUsage(" example: appname -s settings.ini");
		opt->addUsage("");

		opt->setFlag("help", 'h');
		opt->setOption("settings", 's');

		opt->processCommandArgs( argc, argv );

		// Check args
		 /* print usage if no options /
		bool opts = opt->hasOptions();
		bool help = opt->getFlag("help");
		bool help2 = opt->getFlag('h');
		if(!opts ||
		   help ||
		   help2)
		{
			opt->printUsage();
		}
		else
			status = 0;

		// Set settings
		if(status > -1)
		{
			if(opt->getValue('s') != NULL || opt->getValue("settings") != NULL)
        		fileName = opt->getValue('s');
			status = 1;
		}

		delete opt;
		return status;
	}*/
}