using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class Progress
    {
        protected int Current = 0;
        protected int Count = 0;
        protected int last = 0;

        public Progress(int cnt)
        {
            Count = cnt;
            Console.Write("\n0");
        }

        public void Next()
        {
            Current++;
            var p1 = (double)Current / (double)Count;
            var p2 = ((p1) * 100d);
            var p3 = p2 / 10;
            int perc = (int)p3;
            if(perc > last)
            {
                last = perc;
                Console.Write(perc);
            }
        }

        public void Finished()
        {
            while(Current <= Count)
            {
                Next();
            }
            Console.Write("\n");
        }
    }
}
