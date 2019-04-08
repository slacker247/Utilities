using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.system
{
    public class ParseArgs
    {
        public Dictionary<String, String> OptionsSet = new Dictionary<string, string>();

        public ParseArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    if (!OptionsSet.ContainsKey(args[i]))
                    {
                        OptionsSet.Add(args[i], args[i + 1]);
                        i += 1;
                    }
                }
                else
                {
                    if (!OptionsSet.ContainsKey(args[i]))
                        OptionsSet.Add(args[i], "true");
                }
            }
        }

        public String getOption(String option)
        {
            String opt = "false";
            if(OptionsSet.ContainsKey(option))
            {
                opt = OptionsSet[option];
            }
            return opt;
        }
    }
}
