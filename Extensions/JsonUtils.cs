using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Extensions
{
    public class JsonUtils
    {
        public static bool isNull(JObject json, String parm)
        {
            return json[parm] == null || json[parm].Type == JTokenType.Null;
        }
    }
}
