using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Utilities.Extensions
{
    public class ExWebClient : WebClient
    {
        /// <summary>
        /// Timeout in milliseconds
        /// </summary>
        public int TimeOut = 15000;

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = TimeOut;
            return w;
        }

        public String PostJson(String url, String json)
        {
            this.Headers[HttpRequestHeader.ContentType] = "application/json";
            return this.UploadString(url, json);
        }

        public String Post(String url, Dictionary<String, String> parms)
        {
            String urlend_parms = "";

            foreach(String key in parms.Keys)
            {
                urlend_parms += key + "=" + parms[key] + "&";
            }
            urlend_parms = urlend_parms.Substring(0, urlend_parms.Length - 2);

            this.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            return this.UploadString(url, urlend_parms);
        }
    }
}
