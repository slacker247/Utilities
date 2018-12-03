using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class HTMLFormatting
    {
        public static String includeJS(String file)
        {
            String html = "";
            html += "<script type=\"text/javascript\" language=\"javascript\" src=\"";
            html += file;
            html += "\">";
            html += "</script>\n";

            return html;
        }
    }
}
