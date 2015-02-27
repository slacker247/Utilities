using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Utilities.db
{
    public abstract class BaseToXmlObject : Object
    {
        public int readXML(String xml)
        {
            int status = -1;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            readXMLInput(doc.FirstChild);

            return status;
        }

        public abstract void readXMLInput(XmlNode doc);

        /// <summary>
        /// The function serializes the object into an xml format.
        /// </summary>
        /// <returns>A xml string of the serialized data.</returns>
        public String toXML()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.CloseOutput = false;
            settings.NewLineOnAttributes = true;
            settings.Indent = true;

            StringBuilder encodedString = new StringBuilder(1024);
            XmlWriter writer = XmlWriter.Create(encodedString, settings);
            writeToXML(writer);
            writer.Flush();
            writer.Close();
            return encodedString + "\n";
        }

        /// <summary>
        /// The function serializes the object into an xml writer.
        /// </summary>
        /// <returns>none</returns>
        public abstract void writeToXML(XmlWriter writer);

        protected static void propertyToXml<T>(System.Xml.XmlWriter writer, String tagName, T value)
        {
            propertyToXml(writer, tagName, value.ToString());
        }

        protected static void propertyToXml(System.Xml.XmlWriter writer, String tagName, String value)
        {
            writer.WriteStartElement(tagName);
            writer.WriteString(value);
            writer.WriteEndElement();
        }
    }
}
