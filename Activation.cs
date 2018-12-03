using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using Microsoft.Win32;
using System.Management;
using System.Globalization;
using System.Web;

namespace Utilities
{
    public class Activation
    {
        public static int KEY_GOOD = 0;
        public static int KEY_INVALID = 1;
        public static int KEY_BLACKLISTED = 2;
        public static int KEY_PHONY = 3; 

        /// <summary>
        /// Just of the seed values.
        /// </summary>
        protected static List<String> BLACKLIST = new List<String>
                                            {
                                                "11111111"
                                            };

        protected String m_RegKey = @"SOFTWARE\JAM Solutions\SMIME_Search\";
        /// <summary>
        /// Minus the "HKCU\"
        /// or HKLM\ or whatever base tag.
        /// with trailing slash \
        /// </summary>
        public String RegKey
        {
            get
            {
                return m_RegKey;
            }
            set
            {
                m_RegKey = value;
            }
        }
        protected bool m_AllUsers = false;
        public bool AllUsers
        {
            get
            {
                return m_AllUsers;
            }
            set
            {
                m_AllUsers = value;
            }
        }
        public int m_DaysLeft = 0;
        protected String m_TrialPass = "^Rs-3ByyHZm(+rK";

        public void activateTrial()
        {
            String machId = getMachineId();
            Crypto.setSalt(machId);
            String encText = getRegEncText();
            DateTime installDate = DateTime.Now;
            if (encText == null)
            {
                String iDate = Crypto.Encrypt(installDate.ToLongDateString(), m_TrialPass);
                setRegEncText(iDate);
            }
        }

        /// <summary>
        /// Takes a username and a purchased serial number then queries the
        /// server to determine the validity of the pair along with the
        /// machine id, and if they match up the activation key is returned.
        /// </summary>
        /// <param name="userName">generally the user's email</param>
        /// <param name="serial">This is given to the user after purchase</param>
        /// <returns>
        /// The activation code on success
        /// a -1 as a string if it failed to connect
        /// and invalid if the information is incorrect
        /// </returns>
        public String activate(String userName, String serial)
        {
            String aKey = "";
            // get machine id
            String mid = FingerPrint.Value();
            // send machine id, serial number, email to
            String root = "";
#if REMOTE
            root = "www.jamsolutions.net";
#elif LOCAL
            root = "192.168.0.105";
#endif
            String url = "http://%ROOT%/pages/transaction/Activate.ashx?";
            url = url.Replace("%ROOT%", root);
            url += "un=" + HttpUtility.UrlEncode(userName);
            url += "&sk=" + HttpUtility.UrlEncode(serial);
            url += "&mk=" + HttpUtility.UrlEncode(mid);

            // Activating can return 2 possible results:
            // Activation key or invalid
            System.Net.WebClient wc = new System.Net.WebClient();
            String response = wc.DownloadString(url);
            if (response.Length == 0)
            {
                // unable to connect
                aKey = "-1";
            }
            else if (response == "invalid")
            {
                // failed to validate serial key or user name
                aKey = "invalid";
            }
            else
            {
                aKey = response;
                setRegSerialKey(serial);
                setRegEmail(userName);
                setRegEncText(aKey);
            }

            return aKey;
        }

        public bool validateChecksum()
        {
            bool test = false;
            String serialKey = getRegSerialKey();
            // TODO - test serial key
            // verify serial key length
            // check if serial key is blacklisted???
            // verify serial key checksum
            String s = "";
            String c = "";

            if (serialKey != null)
            {
                // remove cosmetic hypens and normalize case
                s = serialKey.ToUpper();
                while (s.Contains('-'))
                {
                    s = s.Replace("-", "");
                }
            }
            // Our keys are always 24 characters long
            if(s.Length == 24)
            {
                // last four characters are the checksum
                c = s.Substring(s.Length - 4, 4);

                // compare the supplied checksum against the real checksum for
                // the key string.
                int check = System.Convert.ToInt32(c, 16);
                int sum = SerialKey.PKV_GetChecksum(s.Substring(0, 20));
                if(sum == check)
                    test = true;
            }
            return test;
        }

        public bool validateActivation()
        {
            bool test = false;
            bool validSerialKey = false;
            String machId = getMachineId();
            Crypto.setSalt(machId);
            String serialKey = getRegSerialKey();
            String email = getRegEmail();
            String encText = getRegEncText();

            // if it fails the checks set serial key to ""
            if (validateChecksum())
            {
                bool testing = true;
                // check availible bytes
                int testBytes = 3;
                String serial = serialKey.Replace("-", "");
                int[] byteIndex = {
#region Keys
#if KEY00
                                    8,
#endif
#if KEY01
                                    10,
#endif
#if KEY02
                                    12,
#endif
#if KEY03
                                    14,
#endif
#if KEY04
                                    16,
#endif
#if KEY05
                                    18,
#endif
#endregion
                                    0
                                };
                long seed = -1;
                String strSeed = "";
                if(serial != null)
                    strSeed = serial.Substring(0, 8);
                seed = System.Convert.ToInt64(strSeed, 16);
                // verify parts of serial key
                for (int i = 0; i < testBytes; i++)
                {
                    String hex = "";
                    if(serial != null)
                        hex = serial.Substring(byteIndex[i], 2);
                    byte b = System.Convert.ToByte(hex, 16);
                    byte bA = SerialKey.PKV_GetKeyByte(seed,
                        SerialKey.m_ByteSeeds[i,0],
                        SerialKey.m_ByteSeeds[i,1],
                        SerialKey.m_ByteSeeds[i,2]);
                    if (b != bA)
                    {
                        testing = false;
                        break;
                    }
                }

                if (testing)
                {
                    validSerialKey = true;
                }
            }

            if (validSerialKey &&
                machId != null &&
                machId.Length > 0 &&
                serialKey != null &&
                serialKey.Length > 0 &&
                email != null &&
                email.Length > 0)
            {
                String result = Crypto.Decrypt(encText, serialKey);
                if (result == email)
                {
                    m_DaysLeft = -1;
                    test = true;
                }
            }
            if (!test)
            {
                // Test for 15-day trial.
                // Encrypt the install date
                DateTime installDate = new DateTime(1970, 1, 1, 0, 0, 1);
                if (encText != null)
                {
                    String iDate = Crypto.Decrypt(encText, m_TrialPass);
                    DateTime.TryParse(iDate, out installDate);
                    TimeSpan sinceInstall = DateTime.Now - installDate;
                    if (sinceInstall.TotalDays <= 15)
                    {
                        m_DaysLeft = 15 - (int)sinceInstall.TotalDays;
                        test = true;
                    }
#if BETA
                // BETA_TEST_KEY
                    else if(serialKey == "BETA_TEST_KEY" &&
                        sinceInstall.TotalDays <= 60)
                    {
                        m_DaysLeft = 60 - (int)sinceInstall.TotalDays;
                        test = true;
                    }
#endif
                }
                //if (!test)
                //    setRegSerialKey("");
            }
            return test;
        }

        public static String getMachineId()
        {
            String machId = FingerPrint.Value();
            return machId;
        }

        public int setRegEncText(String text)
        {
            int status = -1;
            RegistryKey myKey = null;
            if(m_AllUsers)
                myKey = Registry.LocalMachine.CreateSubKey(m_RegKey);
            else
                myKey = Registry.CurrentUser.CreateSubKey(m_RegKey);
            myKey.SetValue("Activated", text, RegistryValueKind.String);
            if (getRegEncText() == text)
                status = 0;
            return status;
        }

        public String getRegEncText()
        {
            String encText = readRegKey("Activated");
            return encText;
        }

        public int setRegSerialKey(String serialKey)
        {
            int status = -1;
            RegistryKey myKey = null;
            if (m_AllUsers)
                myKey = Registry.LocalMachine.CreateSubKey(m_RegKey);
            else
                myKey = Registry.CurrentUser.CreateSubKey(m_RegKey);
            myKey.SetValue("SerialKey", serialKey, RegistryValueKind.String);
            if (getRegSerialKey() == serialKey)
                status = 0;
            return status;
        }

        public String getRegSerialKey()
        {
            String serialKey = readRegKey("SerialKey");
            return serialKey;
        }

        public int setRegEmail(String email)
        {
            int status = -1;
            RegistryKey myKey = null;
            if (m_AllUsers)
                myKey = Registry.LocalMachine.CreateSubKey(m_RegKey);
            else
                myKey = Registry.CurrentUser.CreateSubKey(m_RegKey);
            myKey.SetValue("Email", email, RegistryValueKind.String);
            if (getRegEmail() == email)
                status = 0;
            return status;
        }

        public String getRegEmail()
        {
            String email = readRegKey("Email");
            return email;
        }

        protected String readRegKey(String key)
        {
            String keyValue = "";
            try
            {
                RegistryKey myKey = null;
                if (m_AllUsers)
                    myKey = Registry.LocalMachine.OpenSubKey(m_RegKey, false);
                else
                    myKey = Registry.CurrentUser.OpenSubKey(m_RegKey, false);
                keyValue = myKey.GetValue(key).ToString();
            }
            catch
            {
                keyValue = null;
            }
            return keyValue;
        }
    }
}
