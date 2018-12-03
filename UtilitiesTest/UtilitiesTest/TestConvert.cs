using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesTest
{
    class TestConvert
    {
        public static void test()
        {
            DateTime now = DateTime.Now;
            int ut = Utilities.Convert.ToUnixTime(now);
            DateTime con = Utilities.Convert.FromUnixTime(ut);

            Console.WriteLine(now.ToString() + " = " + con.ToString());
        }
    }
}
