using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Utilities
{
    public class Web
    {
        public static JObject getJson(String url)
        {
            JObject json = null;
            WebClient wc = new WebClient();
            String jStr = wc.DownloadString(url);
            json = JObject.Parse(jStr);
            return json;
        }
    }
}
