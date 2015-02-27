#include "Settings.h"

namespace utilities
{
	Settings::Settings(void)
	{
	}

	Settings::Settings(const Settings& in)
	{
		this->m_Params = in.m_Params;
	}

	Settings::~Settings(void)
	{
	}

	/////////////////////////////////////////////////////////
	/// Function: parseSettings
	///
	/// <summary>Parses the file for all the settings and creates
	/// a table of named value pairs.</summary>
	///
	/// <param>fileName: The complete path to the settings file.</param>
	///
	/// <returns>returns: 0 for a success. Otherwise the
	/// value will be in the lookup table: <see></see> </returns>
	/////////////////////////////////////////////////////////
	int Settings::parseSettings(const char* fileName)
	{
		//Logger::log("Parsing settings...\n", Logger::DEBUGINFO);
		int status = -1;
		string line;
		ifstream file(fileName);
		if(file.is_open())
		{
			status = 0;
			int lineCount = 0;
			while(!file.eof())
			{
				getline(file, line);
				lineCount++;
				if(line == "")
					continue;
				vector<string> tokens;
				char delim = '|';
				Util::Tokenize(line, tokens, delim);
				if(tokens.size() == 2)
				{
					m_Params[tokens[0]] = tokens[1];
				}
				else
				{
					printf("Error: A problem occured while reading the line %d.\n", lineCount);
					// TODO : insert proper error code.
					status = -1;
					break;
				}
			}
			file.close();
		}
		else
			// TODO : insert proper error code.
			status = -1;
		//Logger::log("Parsed settings.\n", Logger::DEBUGINFO);
		return status;
	}

	/////////////////////////////////////////////////////////
	/// Function: getValue
	///
	/// <summary>Takes the given value and searches the internal
	/// hash map for the value.  If it finds it the function will
	/// return the value, otherwise it will return NULL.</summary>
	///
	/// <param>tag: the identifier for the value.</param>
	///
	/// <returns>returns: NULL if the value is not found, otherwise
	/// it will return a string representation of the value.</returns>
	/////////////////////////////////////////////////////////
	char* Settings::getValue(char* tag)
	{
		char value[4072] = "";
		if(m_Params.find(tag) != m_Params.end())
		{
			sprintf(value, "%s", ((string)m_Params[tag]).c_str());
		}
		return value;
	}
}