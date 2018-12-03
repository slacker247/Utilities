using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class Reflection
    {
        public static object newType(System.String type)
        {
            return System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(type);
        }
    }
}
