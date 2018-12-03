using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesTest
{
    class TestThreading
    {
        class Node
        {
            public char value;
            public Node join = null;
            public Node plus = null;
            public Node mult = null;
        };

        // Spent a little to much time on this function...
        void replace(String str, uint rpl, int start, int end)
        {
            StringBuilder tempStr = new StringBuilder(str);
            String prodStr = String.Format("{0}", rpl);
            int prodStrLen = prodStr.Length;
            int strLen = str.Length;
            int i = 0;
            for (i = start; i < strLen && i - start < prodStr.Length; i++)
            {
                if (prodStr[i - start] != '\0')
                {
                    tempStr[i] = prodStr[i - start];
                }
            }
            for (i = 0;
                i + end < strLen + 3 &&
                i+end < str.Length &&
                i+prodStrLen+start < tempStr.Length;
                i++)
            {
                if (str[i + end] == '\0')
                    break;
                tempStr[i + prodStrLen + start] = str[i + end];
            }
            str = tempStr.ToString();
        }

        void calcOperator(String algo, char op)
        {
            int index = 0;
            int i = 0;
            uint algoLen = 0;
            // order of operations...
            // mult first
            uint rhs = 0;
            uint lhs = 0;
            uint result = 0;
            // find mult
            while (index > -1)
            {
                algoLen = (uint)algo.Length;
                index = -1;
                for (i = 0; i < algoLen; i++)
                {
                    if (algo[i] == '\0')
                        break;
                    if (algo[i] == op)
                    {
                        index = i;
                    }
                }
                if (index > -1 && index < algoLen)
                {
                    int start = -1;
                    int end = -1;
                    // get rhs
                    StringBuilder buf = new StringBuilder();
                    for (i = index - 1; i >= 0; i--)
                    {
                        if (algo[i] < 48 || algo[i] > 57 || i == 0) /* a Number */
                        {
                            if (i == 0)
                                start = 0;
                            else
                                start = i + 1;
                            for (int n = 0; n + start < index; n++)
                                buf.Append(algo[n + start]);
                            uint.TryParse(buf.ToString(), out rhs);
                            break;
                        }
                    }
                    // get lhs
                    buf.Clear();
                    for (i = index + 1; i > 0; i++)
                    {
                        if (algo[i] < 48 || algo[i] > 57) /* a Number */
                        {
                            end = i;
                            for (int n = 0; n < end - index; n++)
                                buf.Append(algo[n + index + 1]);
                            uint.TryParse(buf.ToString(), out lhs);
                            break;
                        }
                    }
                    // operator
                    if (op == '*')
                        result = rhs * lhs;
                    else if (op == '+')
                        result = rhs + lhs;
                    //Console.Write("\nrhs: {0}", rhs);
                    //Console.Write("\nlhs: {0}", lhs);
                    //Console.Write("\nresult: {0}", result);
                    //Console.Write("\nalgo: {0}", algo);
                    // remove rhs * lhs from string
                    // insert product at same index
                    replace(algo, result, start, end);
                    //Console.Write("\nalgo: {0}", algo);
                }
            }
        }

        int calcAlgo(String algo)
        {
            int result = -1;
            String tempAlgo = new String(algo.ToCharArray());

            calcOperator(tempAlgo, '*');
            calcOperator(tempAlgo, '+');

            int.TryParse(tempAlgo, out result);
            return result;
        }

        String g_Answer;
        bool g_Found = false;
        String genAlgo(Node current, StringBuilder algo, int index)
        {
            if (current != null)
            {
                algo[index] = current.value;
                // Join
                genAlgo(current.join, algo, index + 1);
                // Plus
                if (index > 0 && index < algo.Length)
                {
                    algo[index] = '+';
                    algo[index + 1] = current.value;
                    genAlgo(current.plus, algo, index + 2);
                    // Mult
                    algo[index] = '*';
                    //algo[index + 1] = current.value;
                    genAlgo(current.mult, algo, index + 2);
                }
            }
            else
            {
                // calc algo
                StringBuilder result = new StringBuilder();
                uint value = (uint)calcAlgo(algo.ToString());
                // compare algo to ans
                result.AppendFormat("{0}", value);
                if (result.ToString().CompareTo(g_Answer) == 0)
                {
                    // TODO : if I was using classes I would implement this differently
                    g_Found = true;
                    Console.Write("{0}", algo);
                }
                else
                {
                    //Console.Write("\nFail: %s=%s", algo, result);
                    int algoLen = algo.Length;
                    for(int i = index; i < algoLen - index; i++)
                        algo[index] = '\0';
                }
            }
            return algo.ToString();
        }

        String numCalc(String num, String ans)
        {
            Node base1 = new Node();
            Node current = base1;
            int i = 0;
            current.value = num[i];
            for (i = 1; i < num.Length; i++)
            {
                Node next = new Node();
                next.value = num[i];
                next.join = null;
                next.plus = null;
                next.mult = null;
                current.join = next;
                current.plus = next;
                current.mult = next;
                current = next;
            }

            // gen algo
            StringBuilder algo = new StringBuilder();
            algo.Append('\0', 512);
            genAlgo(base1, algo, 0);

            // TODO : free base
            return algo.ToString();
        }

        public void prob1()
        {
            String num = "";
            String ans = "";
            StringBuilder buf = new StringBuilder("");
            String result = "";

            Console.Write("Starting...\n");

            // Do something...
            Console.Write("Enter a number to compute (length < 256):\n");
            //scanf("%s", num);
            num = "3456237490";
            Console.Write("Enter a number that represents the answer:\n");
            //scanf("%s", ans);
            ans = "9191";

            g_Answer = ans;
            Console.Write("\n\n\"{0}\", {1} . \"", num, ans);
            result = numCalc(num, ans);
            if (!g_Found)
            {
                Console.Write("no solution");
            }
            Console.Write("\"\n");
        }

        public static void test()
        {
            Utilities.threading.ThreadManager tmgm = new Utilities.threading.ThreadManager();
            tmgm.MaxThreads = 300;
            tmgm.start();

            long i = 0;
            for (; i < 1000000L; i++)
            {
                TestThreading tt = new UtilitiesTest.TestThreading();
                tmgm.addThread(tt.prob1, "tt_" + i);
            }
            tmgm.waitAll();
            tmgm.stop();
        }
    }
}
