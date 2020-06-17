using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Extensions
{
    public class ExByte
    {
        public static bool compare(byte[] rhs, byte[] lhs)
        {
            bool result = false;
            if(rhs.Length == lhs.Length)
            {
                result = true;
                for (int i = 0; i < rhs.Length && result; i++)
                    if (rhs[i] != lhs[i])
                        result = false;
            }
            return result;
        }
    }
}
