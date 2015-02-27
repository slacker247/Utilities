using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesTest
{
    class Program
    {
        static void Main(string[] args)
        {
            /// "^Rs-3ByyHZm(+rK"
	        string mid = "123412341234";
	        string serial = "321432143214";
	        string userName = "jeff@a.com";

	        string encryptText;

	        Utilities.Crypto.setSalt(mid);
	        encryptText = Utilities.Crypto.Encrypt(userName, serial);

	        Console.WriteLine("Encrypted Text: {0}\n", encryptText);

	        string uncryptText = Utilities.Crypto.Decrypt(encryptText, serial);

            Console.WriteLine("Uncrypted Text: {0}\n", uncryptText);
            Console.ReadKey();
        }
    }
}
