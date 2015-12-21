#include "LoadXML.h"

namespace utilities
{
	LoadXML::LoadXML(void)
	{
		try{
			xercesc::XMLPlatformUtils::Initialize();
		}
		catch(const xercesc::XMLException& toCatch)
		{
			// Do your failure processing here
			char* message = xercesc::XMLString::transcode(toCatch.getMessage());
			cout << "Error during initialization! :\n"
				 << message << "\n";
			xercesc::XMLString::release(&message);
		}
	}

	LoadXML::~LoadXML(void)
	{
		xercesc::XMLPlatformUtils::Terminate();
	}


	/////////////////////////////////////////////////////////
	/// Function: loadXML
	///
	/// <summary>Loads the given xml file into memory.</summary>
	///
	/// <param>xmlFile: Full or relative path and file name to 
	/// the xml file to be parsed.</param>
	///
	/// <returns>Returns 0 for a success. Otherwise the
	/// value will be in the error lookup table: <see></see>
	/// </returns>
	/////////////////////////////////////////////////////////
	int LoadXML::loadXML(char* xmlFile)
	{
		int status = -1;
		xercesc::XercesDOMParser* parser = new xercesc::XercesDOMParser();
		parser->setValidationScheme(xercesc::XercesDOMParser::Val_Always);
		parser->setDoNamespaces(true);    // optional

		xercesc::ErrorHandler* errHandler = (xercesc::ErrorHandler*) new xercesc::HandlerBase();
		parser->setErrorHandler(errHandler);

		try
		{
			parser->parse(xmlFile);
			xercesc::DOMDocument* doc = parser->getDocument();
			XMLCh nodeName[100];
			xercesc::XMLString::transcode("vert", nodeName, 99);
			xercesc::DOMNodeList* nodeList = doc->getElementsByTagName(nodeName);
			char* msg = new char[256];
			for(int i = 0; i < nodeList->getLength(); i++)
			{
				xercesc::DOMNode* node = nodeList->item(i);
				char* value = xercesc::XMLString::transcode(node->getTextContent());
				sprintf(msg, "Found Node: %s\n", value);
				Logger::log(msg, Logger::DEBUGINFO);
				xercesc::XMLString::release(&value);
			}
			status = 0;
		}
		catch (const xercesc::XMLException& toCatch)
		{
			char* message = xercesc::XMLString::transcode(toCatch.getMessage());
			char* msg = new char[256];
			sprintf(msg, "Exception message is: \n%s\n", message);
			Logger::log(msg, Logger::DEBUGINFO);
			xercesc::XMLString::release(&message);
		}
		catch (const xercesc::DOMException& toCatch)
		{
			char* message = xercesc::XMLString::transcode(toCatch.msg);
			char* msg = new char[256];
			sprintf(msg, "Exception message is: \n%s\n", message);
			Logger::log(msg, Logger::DEBUGINFO);
			xercesc::XMLString::release(&message);
		}
		catch(xercesc::RuntimeException toCatch)
		{
			char* message = xercesc::XMLString::transcode(toCatch.getMessage());
			char* msg = new char[256];
			sprintf(msg, "Exception message is: \n%s\n", message);
			Logger::log(msg, Logger::DEBUGINFO);
			xercesc::XMLString::release(&message);
		}
		catch (...)
		{
			Logger::log("Unexpected Exception \n", Logger::DEBUGINFO);
		}

		delete parser;
		delete errHandler;
		return status;
	}

	string LoadXML::getTagContent(string tag, xercesc::DOMDocument* data)
	{
		string result = "";
		XMLCh nodeName[100];
		xercesc::XMLString::transcode(tag.c_str(), nodeName, 99);
		if(data->getElementsByTagName(nodeName)->getLength() > 0)
		{
			char* name = xercesc::XMLString::transcode(data->getElementsByTagName(nodeName)->item(0)->getTextContent());
			result = name;
			xercesc::XMLString::release(&name);
		}
		return result;
	}

	string LoadXML::getTagContent(string tag, xercesc::DOMElement* data)
	{
		string result = "";
		XMLCh nodeName[100];
		xercesc::XMLString::transcode(tag.c_str(), nodeName, 99);
		if(data->getElementsByTagName(nodeName)->getLength() > 0)
		{
			char* name = xercesc::XMLString::transcode(data->getElementsByTagName(nodeName)->item(0)->getTextContent());
			result = name;
			xercesc::XMLString::release(&name);
		}
		return result;
	}
}