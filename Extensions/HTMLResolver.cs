using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Utilities.Extensions
{
    public class HtmlResolver : XmlUrlResolver
    {
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            if (absoluteUri.AbsoluteUri.Equals("urn:XHTMLEntities", StringComparison.OrdinalIgnoreCase))
            {
                //ensure the embedded resource is suitably namespaced
                return System.Reflection.Assembly.GetExecutingAssembly().
                    GetManifestResourceStream("Library.ParseXHtml.xhtml-entities.ent");
            }
            return null; //we don't return any other external resources
        }

        public override Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            //make all the XHTML urls resolve to the single "dtd" which is actually just the entities
            if (relativeUri.Equals("-//W3C//DTD XHTML 1.0 Transitional//EN", StringComparison.OrdinalIgnoreCase)
                || relativeUri.Equals("-//W3C//DTD XHTML 1.0 Strict//EN", StringComparison.OrdinalIgnoreCase)
                || relativeUri.Equals("-//W3C//DTD XHTML 1.0 Frameset//EN", StringComparison.OrdinalIgnoreCase)
                || relativeUri.Equals("-//W3C//DTD XHTML 1.1//EN", StringComparison.OrdinalIgnoreCase))
            {
                return new Uri("urn:XHTMLEntities");
            }
            return base.ResolveUri(baseUri, relativeUri);
        }
    }
}
