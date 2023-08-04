using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;
using System.Globalization;
using System.Xml;
#if !SILVERLIGHT
using IWshRuntimeLibrary;
#else
using System.Xml.Linq;
#endif

namespace Utilities
{
    public class Util
    {
#if DEBUG
        public const bool Debug = true;
#else
        public const bool Debug = false;
#endif
        protected static int m_LastMonth = 1;
#if !SILVERLIGHT
        protected static ArrayList m_Files = new ArrayList();
#else
        protected static List<String> m_Files = new List<String>();
#endif

        public static
#if !SILVERLIGHT
            XmlDocument
#else
            XDocument
#endif
            getXml(String xml)
        {
            // Encode the XML string in a UTF-8 byte array
            byte[] encodedString = Encoding.UTF8.GetBytes(xml);
            // Put the byte array into a stream and rewind it to the beginning
            MemoryStream ms = new MemoryStream(encodedString);
            ms.Flush();
            ms.Position = 0; 
#if !SILVERLIGHT
            XmlTextReader textReader = new XmlTextReader(ms);
            XmlDocument doc = new XmlDocument();
#else
            XmlReader textReader = XmlReader.Create(ms);
            XDocument doc = null;
#endif
            bool test = false;
            try
            {
#if !SILVERLIGHT
                doc.Load(textReader);
#else
                doc = XDocument.Load(textReader);
#endif
                test = true;
            }
            catch (System.Xml.XmlException ex)
            {
                System.Console.WriteLine("Util.getXml: Failed to load the Xml.\n");
            }
            return doc;
        }

        public static bool validateEmail(String email)
        {
            bool test = false;
            const String pattern =
               @"^([0-9a-zA-Z]" + //Start with a digit or alphabate
               @"([\+\-_\.][0-9a-zA-Z]+)*" + // No continues or ending +-_. chars in email
               @")+" +
               @"@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,17})$";

            /// other suggestions:
            /// http://stackoverflow.com/questions/5342375/c-sharp-regex-email-validation

            Regex regex = new Regex(pattern);
            Match match = regex.Match(email);
            if (match.Success)
                test = true;
            return test;
        }

#if !SILVERLIGHT
        public static void createShortcut(String pathToExe, String pathToSave, String args = "", String shortcutName = "", String description = "")
        {
            FileInfo exe = null;
            debug(pathToExe, MessageSeverity.INFO, MessageDestination.FILE);
            if(String.IsNullOrEmpty(pathToExe))
                exe = new FileInfo(pathToExe);
            if (exe != null && exe.Exists)
            {
                debug(pathToSave, MessageSeverity.INFO, MessageDestination.FILE);

                DirectoryInfo savePath = new DirectoryInfo(pathToSave);
                savePath.Create();
                if (shortcutName.Length == 0)
                    shortcutName = exe.Name;
                FileInfo fi = new FileInfo(savePath.FullName + "\\" + shortcutName + ".lnk");
                if (fi.Exists)
                    fi.Delete();
                String strDesktop = savePath.FullName;
                WshShell shell = (WshShell)new IWshShell_Class();
                IWshShortcut oShellLink = (IWshShortcut)shell.
                    CreateShortcut(savePath.FullName + "\\" + shortcutName + ".lnk");
                oShellLink.TargetPath = "\"" + exe.FullName + "\"";
                oShellLink.WindowStyle = 1;
                oShellLink.IconLocation = "\"" + exe.FullName + ", 0\"";
                // oShellLink.Description = "\"Shortcut Script\"";
                oShellLink.WorkingDirectory = strDesktop; 
                oShellLink.Arguments = "\"" + args + "\"";

                //debug(savePath.FullName, MessageSeverity.INFO, MessageDestination.FILE);
                //debug(shortcutName, MessageSeverity.INFO, MessageDestination.FILE);
                //debug(exe.FullName, MessageSeverity.INFO, MessageDestination.FILE);
                //debug(strDesktop, MessageSeverity.INFO, MessageDestination.FILE);

                oShellLink.Save();
            }
        }

        [Obsolete("Use Utilities.HTMLFormatting.includeJS")]
        public static String includeJS(String file)
        {
            String html = HTMLFormatting.includeJS(file);
            return html;
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ToHash"></param>
        /// <returns></returns>
        [Obsolete("Use Utilities.Hash.GetMd5Hash")]
        public static string Hash(string ToHash)
        {
            String hash = "";

            /*
            // First we need to convert the string into bytes,
            // which means using a text encoder.
            Encoder enc = System.Text.Encoding.ASCII.GetEncoder();

            // Create a buffer large enough to hold the string
            byte[] data = new byte[ToHash.Length];
            enc.GetBytes(ToHash.ToCharArray(), 0, ToHash.Length, data, 0, true);

            // This is one implementation of the abstract class MD5.
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(data);

            hash = BitConverter.ToString(result).Replace("-", "").ToLower();
            */

            hash = Utilities.Hash.GetMd5Hash(ToHash);
            return hash;
        }

        // Hash an input string and return the hash as
        // a 32 character hexadecimal string.
        [Obsolete("Use Utilities.Hash.GetMd5Hash")]
        public static string getMd5Hash(string input)
        {
            String hash = "";
            hash = Utilities.Hash.GetMd5Hash(input);
            return hash;
        }

#if !SILVERLIGHT
        // TODO : Move to FileIO
        public static String fileHash(String file)
        {
            String hashed = "";
            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    using (SHA1Managed sha1 = new SHA1Managed())
                    {
                        StringBuilder formatted = null;
                        byte[] hash = sha1.ComputeHash(bs);
                        formatted = new StringBuilder(2 * hash.Length);
                        foreach (byte b in hash)
                        {
                            formatted.AppendFormat("{0:X2}", b);
                        }
                        hashed = formatted.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return hashed;
        }

        // TODO : Move to FileIO
        // Does not work with unc paths
        public static long getTotalFreeSpace(string driveName)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    return drive.TotalFreeSpace;
                }
            }
            return -1;
        }

        // TODO : Move to FileIO
        public static ArrayList getFiles(String dir)
        {
            return getFiles(dir, "");
        }

        // TODO : Move to FileIO
        // filter splits multiple file extensions based on the '|'
        // character.
        // Returns an array of strings.
        public static ArrayList getFiles(String dir, String filter)
        {
            m_Files.Clear();
            //DirSearch(dir, filter);
            String[] subFilter = filter.Split('|');
            for (int n = 0; n < subFilter.Length; n++)
            {
                //foreach (String f in Directory.EnumerateFiles(dir, subFilter[n], SearchOption.AllDirectories))
                //{
                //    m_Files.Add(f);
                //}
                DirSearch(dir, subFilter[n]);
            }
            return m_Files;
        }

        // TODO : Move to FileIO
        protected static void DirSearch(String sDir, String filter)
        {
            try
            {
                String[] files = null;
                if(filter.Length > 0)
                    files = Directory.GetFiles(sDir, filter);
                else
                    files = Directory.GetFiles(sDir);
                foreach (String f in files)
                {
                    m_Files.Add(f);
                }
                foreach (String d in Directory.GetDirectories(sDir))
                {
                    DirSearch(d, filter);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        // TODO : Move to FileIO
        public static bool byteListToFile(String fileName, List<byte[]> bytes)
        {
            bool retVal = false;
            bool first = false;
            foreach (byte[] byteArray in bytes)
            {
                retVal = byteArrayToFile(fileName, byteArray, first);
                if (!retVal)
                    break;
                if (!first)
                    first = true;
            }
            return retVal;
        }

        // TODO : Move to FileIO
        public static bool byteArrayToFile(string fileName, byte[] byteArray, bool append = false)
        {
            try
            {
                System.IO.FileMode fm = FileMode.Create;
                if (append)
                    fm = FileMode.Append;
                // Open file for reading
                System.IO.FileStream fileStream =
                   new System.IO.FileStream(fileName, fm,
                                            System.IO.FileAccess.Write);
                // Writes a block of bytes to this stream using data from
                // a byte array.
                fileStream.Write(byteArray, 0, byteArray.Length);

                // close file stream
                fileStream.Close();

                return true;
            }
            catch (Exception ex)
            {
                // Error
                Console.WriteLine("Exception caught in process: {0}",
                                  ex.ToString());
            }

            // error occured, return false
            return false;
        }
#endif
        public static bool validateUrl(String url)
        {
            bool status = false;
            if (url != null && url.Length > 0)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
#if !SILVERLIGHT
                    request.Proxy = null;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
#else
                    //https://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.begingetresponse%28v=vs.110%29.aspx
//                    HttpWebResponse response = (HttpWebResponse)request.BeginGetResponse();
#endif
                    status = true;
                }
                catch { }
            }
            return status;
        }

        public static DateTime parseDateTime(String date)
        {
            DateTime result = new DateTime(0);
            if (date.Length > 0)
            {
                try
                {
                    date = date.Replace("00:00:00 ", "01:00:00");
                    result = DateTime.Parse(date);
                }
                catch (Exception e)
                {
                    //Tue, 11 May 2010 00:00:00 PST
                    try
                    {
                        result = DateTime.Parse(
                            date.Substring(0, date.Length - 4).Trim());
                    }
                    catch (Exception ex)
                    {
                        // Do to utc issues
                        try
                        {
                            Regex r = new Regex("[0-9][0-9] [a-zA-Z]+ [0-9][0-9][0-9][0-9]");
                            Match m = r.Match(date);
                            if (m.Length > 0)
                            {
                                String tdate = date.Substring(m.Index, m.Length);
                                String[] split = tdate.Split(' ');
                                int year = System.Convert.ToInt32(split[2]);
                                int month = Utilities.Util.convertMonth(split[1]);
                                if (month == -1)
                                    month = m_LastMonth;
                                else
                                    m_LastMonth = month;
                                int day = System.Convert.ToInt32(split[0]);
                                DateTime dt = new DateTime(year,
                                    month,
                                    day);
                                System.Console.WriteLine("Date: " + dt.ToString());
                                result = dt;
                            }
                        }
                        catch (Exception er)
                        {
                            System.Console.WriteLine("VideoShow: Error: Can't convert the string to a DateTime for airdate.");
                            System.Console.WriteLine("AirDate: " + date + "\n Short: " + (date.Substring(0, date.Length - 4)));
                        }
                    }
                }
            }
            return result;
        }

        public static bool isNumber(object num)
        {
            bool test = false;
            if (num is int ||
                num is uint ||
                num is float ||
                num is double ||
                num is short ||
                num is ushort ||
                num is long ||
                num is ulong ||
                num is decimal)
            {
                test = true;
            }
            return test;
        }

        public static double toNumber(object num)
        {
            double retVal = 0;
            if (isNumber(num))
                retVal = Convert.ToDouble(num);
            else if (num is String)
                double.TryParse((String)num, out retVal);
            return retVal;
        }

        // TODO : Move to FileIO
        public static bool checkExt(String ext)
        {
            bool status = false;
            switch (ext)
            {
                case ".mov":
                    status = true;
                    break;
                case ".m4v":
                    status = true;
                    break;
                case ".mp4":
                    status = true;
                    break;
            }
            return status;
        }
        
        //return "IEnumerable<T>" if all you need to do is iterate over elements
        public static IEnumerable<ConsoleKeyInfo> GetInput()
        {
            //use "var" if return type is obvious from context
            //use "HashSet<T>" if you need a collection of unique items
            var input = new HashSet<ConsoleKeyInfo>();
            var keyCount = 0;
            while (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                //use constants instead of magic numbers - MaxInputLength
                if (++keyCount > 1) continue;
                //no need to call "Contains", non-unique items will be dropped automatically
                input.Add(key);
            }
            //no need to convert anything
            return input;
        }

        public static int convertMonth(String month)
        {
            int mon = -1;
            switch (month)
            {
                case "January":
                case "Jan":
                    mon = 1;
                    break;
                case "February":
                case "Feb":
                    mon = 2;
                    break;
                case "March":
                case "Mar":
                    mon = 3;
                    break;
                case "April":
                case "Apr":
                    mon = 4;
                    break;
                case "May":
                    mon = 5;
                    break;
                case "June":
                case "Jun":
                    mon = 6;
                    break;
                case "July":
                case "Jul":
                    mon = 7;
                    break;
                case "August":
                case "Aug":
                    mon = 8;
                    break;
                case "September":
                case "Sep":
                case "Sept":
                    mon = 9;
                    break;
                case "October":
                case "Oct":
                    mon = 10;
                    break;
                case "November":
                case "Nov":
                    mon = 11;
                    break;
                case "December":
                case "Dec":
                    mon = 12;
                    break;
            }
            return mon;
        }

        public static String toUTF8(String str)
        {
            if (str != null)
            {
                str = str.Replace("&lsquo;", "‘");
                str = str.Replace("&rsquo;", "’");
                str = str.Replace("&sbquo;", "‚");
                str = str.Replace("&ldquo;", "“");
                str = str.Replace("&rdquo;", "”");
                str = str.Replace("&bdquo;", "„");
                str = str.Replace("&dagger;", "†");
                str = str.Replace("&Dagger;", "‡");
                str = str.Replace("&permil;", "‰");
                str = str.Replace("&lsaquo;", "‹");
                str = str.Replace("&rsaquo;", "›");
                str = str.Replace("&spades;", "♠");
                str = str.Replace("&clubs;", "♣");
                str = str.Replace("&hearts;", "♥");
                str = str.Replace("&diams;", "♦");
                str = str.Replace("&oline;", "‾");
                str = str.Replace("&larr;", "←");
                str = str.Replace("&uarr;", "↑");
                str = str.Replace("&rarr;", "→");
                str = str.Replace("&darr;", "↓");
                str = str.Replace("&#x2122;", "™");
                str = str.Replace("&trade;", "™");
                str = str.Replace("&#09;", "\t");
                str = str.Replace("&#32;", " ");
                str = str.Replace("&#33;", "!");
                str = str.Replace("&#x22;", "\"");
                str = str.Replace("&#34;", "\"");
                str = str.Replace("&quot;", "\"");
                str = str.Replace("&#35;", "#");
                str = str.Replace("&#36;", "$");
                str = str.Replace("&#37;", "%");
                str = str.Replace("&#38;", "&");
                str = str.Replace("&#x26;", "&");
                str = str.Replace("&amp;", "&");
                str = str.Replace("&#x27;", "'");
                str = str.Replace("&#39;", "'");
                str = str.Replace("&#40;", "(");
                str = str.Replace("&#41;", ")");
                str = str.Replace("&#42;", "*");
                str = str.Replace("&#43;", "+");
                str = str.Replace("&#44;", ",");
                str = str.Replace("&#45;", "-");
                str = str.Replace("&#46;", ".");
                str = str.Replace("&#47;", "/");
                str = str.Replace("&frasl;", "/");
                str = str.Replace("&#58;", ":");
                str = str.Replace("&#59;", ";");
                str = str.Replace("&#60;", "<");
                str = str.Replace("&lt;", "<");
                str = str.Replace("&#61;", "=");
                str = str.Replace("&#62;", ">");
                str = str.Replace("&gt;", ">");
                str = str.Replace("&#63;", "?");
                str = str.Replace("&#64;", "@");
                str = str.Replace("&#91;", "[");
                str = str.Replace("&#92;", "\\");
                str = str.Replace("&#93;", "]");
                str = str.Replace("&#94;", "^");
                str = str.Replace("&#95;", "_");
                str = str.Replace("&#96;", "`");
                str = str.Replace("&#123;", "{");
                str = str.Replace("&#124;", "|");
                str = str.Replace("&#125;", "}");
                str = str.Replace("&#126;", "~");
                str = str.Replace("&#150;", "–");
                str = str.Replace("&ndash;", "–");
                str = str.Replace("&#151;", "—");
                str = str.Replace("&mdash;", "—");
                str = str.Replace("&#160;", " ");
                str = str.Replace("&nbsp;", " ");
                str = str.Replace("&#161;", "¡");
                str = str.Replace("&iexcl;", "¡");
                str = str.Replace("&#162;", "¢");
                str = str.Replace("&cent;", "¢");
                str = str.Replace("&#163;", "£");
                str = str.Replace("&pound;", "£");
                str = str.Replace("&#164;", "¤");
                str = str.Replace("&curren;", "¤");
                str = str.Replace("&#165;", "¥");
                str = str.Replace("&yen;", "¥");
                str = str.Replace("&#166;", "¦");
                str = str.Replace("&brvbar;", "¦");
                str = str.Replace("&brkbar;", "¦");
                str = str.Replace("&#167;", "§");
                str = str.Replace("&sect;", "§");
                str = str.Replace("&#168;", "¨");
                str = str.Replace("&uml;", "¨");
                str = str.Replace("&die;", "¨");
                str = str.Replace("&#169;", "©");
                str = str.Replace("&copy;", "©");
                str = str.Replace("&#170;", "ª");
                str = str.Replace("&ordf;", "ª");
                str = str.Replace("&#171;", "«");
                str = str.Replace("&laquo;", "«");
                str = str.Replace("&#172;", "¬");
                str = str.Replace("&not;", "¬");
                str = str.Replace("&#173;", "­");
                str = str.Replace("&shy;", "­");
                str = str.Replace("&#174;", "®");
                str = str.Replace("&reg;", "®");
                str = str.Replace("&#175;", "¯");
                str = str.Replace("&macr;", "¯");
                str = str.Replace("&hibar;", "¯");
                str = str.Replace("&#176;", "°");
                str = str.Replace("&deg;", "°");
                str = str.Replace("&#177;", "±");
                str = str.Replace("&plusmn;", "±");
                str = str.Replace("&#178;", "²");
                str = str.Replace("&sup2;", "²");
                str = str.Replace("&#179;", "³");
                str = str.Replace("&sup3;", "³");
                str = str.Replace("&#180;", "´");
                str = str.Replace("&acute;", "´");
                str = str.Replace("&#181;", "µ");
                str = str.Replace("&micro;", "µ");
                str = str.Replace("&#182;", "¶");
                str = str.Replace("&para;", "¶");
                str = str.Replace("&#183;", "·");
                str = str.Replace("&middot;", "·");
                str = str.Replace("&#184;", "¸");
                str = str.Replace("&cedil;", "¸");
                str = str.Replace("&#185;", "¹");
                str = str.Replace("&sup1;", "¹");
                str = str.Replace("&#186;", "º");
                str = str.Replace("&ordm;", "º");
                str = str.Replace("&#187;", "»");
                str = str.Replace("&raquo;", "»");
                str = str.Replace("&#188;", "¼");
                str = str.Replace("&frac14;", "¼");
                str = str.Replace("&#189;", "½");
                str = str.Replace("&frac12;", "½");
                str = str.Replace("&#190;", "¾");
                str = str.Replace("&frac34;", "¾");
                str = str.Replace("&#191;", "¿");
                str = str.Replace("&iquest;", "¿");
                str = str.Replace("&#192;", "À");
                str = str.Replace("&Agrave;", "À");
                str = str.Replace("&#xC1;", "Á");
                str = str.Replace("&#193;", "Á");
                str = str.Replace("&Aacute;", "Á");
                str = str.Replace("&#194;", "Â");
                str = str.Replace("&Acirc;", "Â");
                str = str.Replace("&#195;", "Ã");
                str = str.Replace("&Atilde;", "Ã");
                str = str.Replace("&#196;", "Ä");
                str = str.Replace("&Auml;", "Ä");
                str = str.Replace("&#197;", "Å");
                str = str.Replace("&Aring;", "Å");
                str = str.Replace("&#198;", "Æ");
                str = str.Replace("&AElig;", "Æ");
                str = str.Replace("&#199;", "Ç");
                str = str.Replace("&Ccedil;", "Ç");
                str = str.Replace("&#200;", "È");
                str = str.Replace("&Egrave;", "È");
                str = str.Replace("&#201;", "É");
                str = str.Replace("&Eacute;", "É");
                str = str.Replace("&#202;", "Ê");
                str = str.Replace("&Ecirc;", "Ê");
                str = str.Replace("&#203;", "Ë");
                str = str.Replace("&Euml;", "Ë");
                str = str.Replace("&#204;", "Ì");
                str = str.Replace("&Igrave;", "Ì");
                str = str.Replace("&#205;", "Í");
                str = str.Replace("&Iacute;", "Í");
                str = str.Replace("&#206;", "Î");
                str = str.Replace("&Icirc;", "Î");
                str = str.Replace("&#207;", "");
                str = str.Replace("&Iuml;", "Ï");
                str = str.Replace("&#208;", "Ð");
                str = str.Replace("&ETH;", "Ð");
                str = str.Replace("&#209;", "Ñ");
                str = str.Replace("&Ntilde;", "Ñ");
                str = str.Replace("&#210;", "Ò");
                str = str.Replace("&Ograve;", "Ò");
                str = str.Replace("&#211;", "Ó");
                str = str.Replace("&Oacute;", "Ó");
                str = str.Replace("&#212;", "Ô");
                str = str.Replace("&Ocirc;", "Ô");
                str = str.Replace("&#213;", "Õ");
                str = str.Replace("&Otilde;", "Õ");
                str = str.Replace("&#214;", "Ö");
                str = str.Replace("&Ouml;", "Ö");
                str = str.Replace("&#215;", "×");
                str = str.Replace("&times;", "×");
                str = str.Replace("&#216;", "Ø");
                str = str.Replace("&Oslash;", "Ø");
                str = str.Replace("&#217;", "Ù");
                str = str.Replace("&Ugrave;", "Ù");
                str = str.Replace("&#218;", "Ú");
                str = str.Replace("&Uacute;", "Ú");
                str = str.Replace("&#219;", "Û");
                str = str.Replace("&Ucirc;", "Û");
                str = str.Replace("&#220;", "Ü");
                str = str.Replace("&Uuml;", "Ü");
                str = str.Replace("&#221;", "Ý");
                str = str.Replace("&Yacute;", "Ý");
                str = str.Replace("&#222;", "Þ");
                str = str.Replace("&THORN;", "Þ");
                str = str.Replace("&#223;", "ß");
                str = str.Replace("&szlig;", "ß");
                str = str.Replace("&#224;", "à");
                str = str.Replace("&agrave;", "à");
                str = str.Replace("&#xE1;", "á");
                str = str.Replace("&#225;", "á");
                str = str.Replace("&aacute;", "á");
                str = str.Replace("&#226;", "â");
                str = str.Replace("&acirc;", "â");
                str = str.Replace("&#227;", "ã");
                str = str.Replace("&atilde;", "ã");
                str = str.Replace("&#xE4;", "ä");
                str = str.Replace("&#228;", "ä");
                str = str.Replace("&auml;", "ä");
                str = str.Replace("&#229;", "å");
                str = str.Replace("&aring;", "å");
                str = str.Replace("&#230;", "æ");
                str = str.Replace("&aelig;", "æ");
                str = str.Replace("&#xE7;", "ç");
                str = str.Replace("&#231;", "ç");
                str = str.Replace("&ccedil;", "ç");
                str = str.Replace("&#232;", "è");
                str = str.Replace("&egrave;", "è");
                str = str.Replace("&#xE9;", "é");
                str = str.Replace("&#233;", "é");
                str = str.Replace("&eacute;", "é");
                str = str.Replace("&#234;", "ê");
                str = str.Replace("&ecirc;", "ê");
                str = str.Replace("&#235;", "ë");
                str = str.Replace("&euml;", "ë");
                str = str.Replace("&#236;", "ì");
                str = str.Replace("&igrave;", "ì");
                str = str.Replace("&#xED;", "í");
                str = str.Replace("&#237;", "í");
                str = str.Replace("&iacute;", "í");
                str = str.Replace("&#238;", "î");
                str = str.Replace("&icirc;", "î");
                str = str.Replace("&#xEF;", "ï");
                str = str.Replace("&#239;", "ï");
                str = str.Replace("&iuml;", "ï");
                str = str.Replace("&#240;", "ð");
                str = str.Replace("&eth;", "ð");
                str = str.Replace("&#xF1;", "ñ");
                str = str.Replace("&#241;", "ñ");
                str = str.Replace("&ntilde;", "ñ");
                str = str.Replace("&#242;", "ò");
                str = str.Replace("&ograve;", "ò");
                str = str.Replace("&#xF3;", "ó");
                str = str.Replace("&#243;", "ó");
                str = str.Replace("&oacute;", "ó");
                str = str.Replace("&#244;", "ô");
                str = str.Replace("&ocirc;", "ô");
                str = str.Replace("&#245;", "õ");
                str = str.Replace("&otilde;", "õ");
                str = str.Replace("&#246;", "ö");
                str = str.Replace("&ouml;", "ö");
                str = str.Replace("&#247;", "÷");
                str = str.Replace("&divide;", "÷");
                str = str.Replace("&#248;", "ø");
                str = str.Replace("&oslash;", "ø");
                str = str.Replace("&#249;", "ù");
                str = str.Replace("&ugrave;", "ù");
                str = str.Replace("&#xFA;", "ú");
                str = str.Replace("&#250;", "ú");
                str = str.Replace("&uacute;", "ú");
                str = str.Replace("&#251;", "û");
                str = str.Replace("&ucirc;", "û");
                str = str.Replace("&#xFC;", "ü");
                str = str.Replace("&#252;", "ü");
                str = str.Replace("&uuml;", "ü");
                str = str.Replace("&#253;", "ý");
                str = str.Replace("&yacute;", "ý");
                //str = str.Replace("Icelandic", "þ");
                str = str.Replace("&#254;", "þ");
                str = str.Replace("&thorn;", "þ");
                str = str.Replace("&#255;", "ÿ");
                str = str.Replace("&yuml;", "ÿ");
                str = str.Replace("&Alpha;", "Α");
                str = str.Replace("&alpha;", "α");
                str = str.Replace("&Beta;", "Β");
                str = str.Replace("&beta;", "β");
                str = str.Replace("&Gamma;", "Γ");
                str = str.Replace("&gamma;", "γ");
                str = str.Replace("&Delta;", "Δ");
                str = str.Replace("&delta;", "δ");
                str = str.Replace("&Epsilon;", "Ε");
                str = str.Replace("&epsilon;", "ε");
                str = str.Replace("&Zeta;", "Ζ");
                str = str.Replace("&zeta;", "ζ");
                str = str.Replace("&Eta;", "Η");
                str = str.Replace("&eta;", "η");
                str = str.Replace("&Theta;", "Θ");
                str = str.Replace("&theta;", "θ");
                str = str.Replace("&Iota;", "Ι");
                str = str.Replace("&iota;", "ι");
                str = str.Replace("&Kappa;", "Κ");
                str = str.Replace("&kappa;", "κ");
                str = str.Replace("&Lambda;", "Λ");
                str = str.Replace("&lambda;", "λ");
                str = str.Replace("&Mu;", "Μ");
                str = str.Replace("&mu;", "μ");
                str = str.Replace("&Nu;", "Ν");
                str = str.Replace("&nu;", "ν");
                str = str.Replace("&Xi;", "Ξ");
                str = str.Replace("&xi;", "ξ");
                str = str.Replace("&Omicron;", "Ο");
                str = str.Replace("&omicron;", "ο");
                str = str.Replace("&Pi;", "Π");
                str = str.Replace("&pi;", "π");
                str = str.Replace("&Rho;", "Ρ");
                str = str.Replace("&rho;", "ρ");
                str = str.Replace("&Sigma;", "Σ");
                str = str.Replace("&sigma;", "σ");
                str = str.Replace("&Tau;", "Τ");
                str = str.Replace("&tau;", "τ");
                str = str.Replace("&Upsilon;", "Υ");
                str = str.Replace("&upsilon;", "υ");
                str = str.Replace("&Phi;", "Φ");
                str = str.Replace("&phi;", "φ");
                str = str.Replace("&Chi;", "Χ");
                str = str.Replace("&chi;", "χ");
                str = str.Replace("&Psi;", "Ψ");
                str = str.Replace("&psi;", "ψ");
                str = str.Replace("&Omega;", "Ω");
                str = str.Replace("&omega;", "ω");
                str = str.Replace("&#9679;", "●");
            }
            return str;
        }

        public static String fromUTF8(String str)
        {
            if (str != null)
            {
                str = str.Replace("&", "&amp;");
                str = str.Replace(">", "&gt;");
                str = str.Replace("<", "&lt;");

                /*str = str.Replace("‘", "&lsquo;");
                str = str.Replace("’", "&rsquo;");
                str = str.Replace("‚", "&sbquo;");
                str = str.Replace("“", "&ldquo;");
                str = str.Replace("”", "&rdquo;");
                str = str.Replace("„", "&bdquo;");
                str = str.Replace("†", "&dagger;");
                str = str.Replace("‡", "&Dagger;");
                str = str.Replace("‰", "&permil;");
                str = str.Replace("‹", "&lsaquo;");
                str = str.Replace("›", "&rsaquo;");
                str = str.Replace("♠", "&spades;");
                str = str.Replace("♣", "&clubs;");
                str = str.Replace("♥", "&hearts;");
                str = str.Replace("♦", "&diams;");
                str = str.Replace("‾", "&oline;");
                str = str.Replace("←", "&larr;");
                str = str.Replace("↑", "&uarr;");
                str = str.Replace("→", "&rarr;");
                str = str.Replace("↓", "&darr;");
                str = str.Replace("™", "&#x2122;");
                str = str.Replace("™", "&trade;");
                str = str.Replace("\t", "&#09;");
                str = str.Replace(" ", "&#32;");
                str = str.Replace("!", "&#33;");
                str = str.Replace("\"", "&#x22;");
                str = str.Replace("\"", "&#34;");
                str = str.Replace("\"", "&quot;");
                str = str.Replace("#", "&#35;");
                str = str.Replace("$", "&#36;");
                str = str.Replace("%", "&#37;");
                str = str.Replace("&", "&#38;");
                str = str.Replace("&", "&#x26;");
                str = str.Replace("'", "&#x27;");
                str = str.Replace("'", "&#39;");
                str = str.Replace("(", "&#40;");
                str = str.Replace(")", "&#41;");
                str = str.Replace("*", "&#42;");
                str = str.Replace("+", "&#43;");
                str = str.Replace(",", "&#44;");
                str = str.Replace("-", "&#45;");
                str = str.Replace(".", "&#46;");
                str = str.Replace("/", "&#47;");
                str = str.Replace("/", "&frasl;");
                str = str.Replace(":", "&#58;");
                str = str.Replace(";", "&#59;");
                str = str.Replace("<", "&#60;");
                str = str.Replace("=", "&#61;");
                str = str.Replace(">", "&#62;");
                str = str.Replace("?", "&#63;");
                str = str.Replace("@", "&#64;");
                str = str.Replace("[", "&#91;");
                str = str.Replace("\\", "&#92;");
                str = str.Replace("]", "&#93;");
                str = str.Replace("^", "&#94;");
                str = str.Replace("_", "&#95;");
                str = str.Replace("`", "&#96;");
                str = str.Replace("{", "&#123;");
                str = str.Replace("|", "&#124;");
                str = str.Replace("}", "&#125;");
                str = str.Replace("~", "&#126;");
                str = str.Replace("–", "&#150;");
                str = str.Replace("–", "&ndash;");
                str = str.Replace("—", "&#151;");
                str = str.Replace("—", "&mdash;");
                str = str.Replace(" ", "&#160;");
                str = str.Replace(" ", "&nbsp;");
                str = str.Replace("¡", "&#161;");
                str = str.Replace("¡", "&iexcl;");
                str = str.Replace("¢", "&#162;");
                str = str.Replace("¢", "&cent;");
                str = str.Replace("£", "&#163;");
                str = str.Replace("£", "&pound;");
                str = str.Replace("¤", "&#164;");
                str = str.Replace("¤", "&curren;");
                str = str.Replace("¥", "&#165;");
                str = str.Replace("¥", "&yen;");
                str = str.Replace("¦", "&#166;");
                str = str.Replace("¦", "&brvbar;");
                str = str.Replace("¦", "&brkbar;");
                str = str.Replace("§", "&#167;");
                str = str.Replace("§", "&sect;");
                str = str.Replace("¨", "&#168;");
                str = str.Replace("¨", "&uml;");
                str = str.Replace("¨", "&die;");
                str = str.Replace("©", "&#169;");
                str = str.Replace("©", "&copy;");
                str = str.Replace("ª", "&#170;");
                str = str.Replace("ª", "&ordf;");
                str = str.Replace("«", "&#171;");
                str = str.Replace("«", "&laquo;");
                str = str.Replace("¬", "&#172;");
                str = str.Replace("¬", "&not;");
                str = str.Replace("­", "&#173;");
                str = str.Replace("­", "&shy;");
                str = str.Replace("®", "&#174;");
                str = str.Replace("®", "&reg;");
                str = str.Replace("¯", "&#175;");
                str = str.Replace("¯", "&macr;");
                str = str.Replace("¯", "&hibar;");
                str = str.Replace("°", "&#176;");
                str = str.Replace("°", "&deg;");
                str = str.Replace("±", "&#177;");
                str = str.Replace("±", "&plusmn;");
                str = str.Replace("²", "&#178;");
                str = str.Replace("²", "&sup2;");
                str = str.Replace("³", "&#179;");
                str = str.Replace("³", "&sup3;");
                str = str.Replace("´", "&#180;");
                str = str.Replace("´", "&acute;");
                str = str.Replace("µ", "&#181;");
                str = str.Replace("µ", "&micro;");
                str = str.Replace("¶", "&#182;");
                str = str.Replace("¶", "&para;");
                str = str.Replace("·", "&#183;");
                str = str.Replace("·", "&middot;");
                str = str.Replace("¸", "&#184;");
                str = str.Replace("¸", "&cedil;");
                str = str.Replace("¹", "&#185;");
                str = str.Replace("¹", "&sup1;");
                str = str.Replace("º", "&#186;");
                str = str.Replace("º", "&ordm;");
                str = str.Replace("»", "&#187;");
                str = str.Replace("»", "&raquo;");
                str = str.Replace("¼", "&#188;");
                str = str.Replace("¼", "&frac14;");
                str = str.Replace("½", "&#189;");
                str = str.Replace("½", "&frac12;");
                str = str.Replace("¾", "&#190;");
                str = str.Replace("¾", "&frac34;");
                str = str.Replace("¿", "&#191;");
                str = str.Replace("¿", "&iquest;");
                str = str.Replace("À", "&#192;");
                str = str.Replace("À", "&Agrave;");
                str = str.Replace("Á", "&#xC1;");
                str = str.Replace("Á", "&#193;");
                str = str.Replace("Á", "&Aacute;");
                str = str.Replace("Â", "&#194;");
                str = str.Replace("Â", "&Acirc;");
                str = str.Replace("Ã", "&#195;");
                str = str.Replace("Ã", "&Atilde;");
                str = str.Replace("Ä", "&#196;");
                str = str.Replace("Ä", "&Auml;");
                str = str.Replace("Å", "&#197;");
                str = str.Replace("Å", "&Aring;");
                str = str.Replace("Æ", "&#198;");
                str = str.Replace("Æ", "&AElig;");
                str = str.Replace("Ç", "&#199;");
                str = str.Replace("Ç", "&Ccedil;");
                str = str.Replace("È", "&#200;");
                str = str.Replace("È", "&Egrave;");
                str = str.Replace("É", "&#201;");
                str = str.Replace("É", "&Eacute;");
                str = str.Replace("Ê", "&#202;");
                str = str.Replace("Ê", "&Ecirc;");
                str = str.Replace("Ë", "&#203;");
                str = str.Replace("Ë", "&Euml;");
                str = str.Replace("Ì", "&#204;");
                str = str.Replace("Ì", "&Igrave;");
                str = str.Replace("Í", "&#205;");
                str = str.Replace("Í", "&Iacute;");
                str = str.Replace("Î", "&#206;");
                str = str.Replace("Î", "&Icirc;");
                str = str.Replace("", "&#207;");
                str = str.Replace("Ï", "&Iuml;");
                str = str.Replace("Ð", "&#208;");
                str = str.Replace("Ð", "&ETH;");
                str = str.Replace("Ñ", "&#209;");
                str = str.Replace("Ñ", "&Ntilde;");
                str = str.Replace("Ò", "&#210;");
                str = str.Replace("Ò", "&Ograve;");
                str = str.Replace("Ó", "&#211;");
                str = str.Replace("Ó", "&Oacute;");
                str = str.Replace("Ô", "&#212;");
                str = str.Replace("Ô", "&Ocirc;");
                str = str.Replace("Õ", "&#213;");
                str = str.Replace("Õ", "&Otilde;");
                str = str.Replace("Ö", "&#214;");
                str = str.Replace("Ö", "&Ouml;");
                str = str.Replace("×", "&#215;");
                str = str.Replace("×", "&times;");
                str = str.Replace("Ø", "&#216;");
                str = str.Replace("Ø", "&Oslash;");
                str = str.Replace("Ù", "&#217;");
                str = str.Replace("Ù", "&Ugrave;");
                str = str.Replace("Ú", "&#218;");
                str = str.Replace("Ú", "&Uacute;");
                str = str.Replace("Û", "&#219;");
                str = str.Replace("Û", "&Ucirc;");
                str = str.Replace("Ü", "&#220;");
                str = str.Replace("Ü", "&Uuml;");
                str = str.Replace("Ý", "&#221;");
                str = str.Replace("Ý", "&Yacute;");
                str = str.Replace("Þ", "&#222;");
                str = str.Replace("Þ", "&THORN;");
                str = str.Replace("ß", "&#223;");
                str = str.Replace("ß", "&szlig;");
                str = str.Replace("à", "&#224;");
                str = str.Replace("à", "&agrave;");
                str = str.Replace("á", "&#xE1;");
                str = str.Replace("á", "&#225;");
                str = str.Replace("á", "&aacute;");
                str = str.Replace("â", "&#226;");
                str = str.Replace("â", "&acirc;");
                str = str.Replace("ã", "&#227;");
                str = str.Replace("ã", "&atilde;");
                str = str.Replace("ä", "&#xE4;");
                str = str.Replace("ä", "&#228;");
                str = str.Replace("ä", "&auml;");
                str = str.Replace("å", "&#229;");
                str = str.Replace("å", "&aring;");
                str = str.Replace("æ", "&#230;");
                str = str.Replace("æ", "&aelig;");
                str = str.Replace("ç", "&#xE7;");
                str = str.Replace("ç", "&#231;");
                str = str.Replace("ç", "&ccedil;");
                str = str.Replace("è", "&#232;");
                str = str.Replace("è", "&egrave;");
                str = str.Replace("é", "&#xE9;");
                str = str.Replace("é", "&#233;");
                str = str.Replace("é", "&eacute;");
                str = str.Replace("ê", "&#234;");
                str = str.Replace("ê", "&ecirc;");
                str = str.Replace("ë", "&#235;");
                str = str.Replace("ë", "&euml;");
                str = str.Replace("ì", "&#236;");
                str = str.Replace("ì", "&igrave;");
                str = str.Replace("í", "&#xED;");
                str = str.Replace("í", "&#237;");
                str = str.Replace("í", "&iacute;");
                str = str.Replace("î", "&#238;");
                str = str.Replace("î", "&icirc;");
                str = str.Replace("ï", "&#xEF;");
                str = str.Replace("ï", "&#239;");
                str = str.Replace("ï", "&iuml;");
                str = str.Replace("ð", "&#240;");
                str = str.Replace("ð", "&eth;");
                str = str.Replace("ñ", "&#xF1;");
                str = str.Replace("ñ", "&#241;");
                str = str.Replace("ñ", "&ntilde;");
                str = str.Replace("ò", "&#242;");
                str = str.Replace("ò", "&ograve;");
                str = str.Replace("ó", "&#xF3;");
                str = str.Replace("ó", "&#243;");
                str = str.Replace("ó", "&oacute;");
                str = str.Replace("ô", "&#244;");
                str = str.Replace("ô", "&ocirc;");
                str = str.Replace("õ", "&#245;");
                str = str.Replace("õ", "&otilde;");
                str = str.Replace("ö", "&#246;");
                str = str.Replace("ö", "&ouml;");
                str = str.Replace("÷", "&#247;");
                str = str.Replace("÷", "&divide;");
                str = str.Replace("ø", "&#248;");
                str = str.Replace("ø", "&oslash;");
                str = str.Replace("ù", "&#249;");
                str = str.Replace("ù", "&ugrave;");
                str = str.Replace("ú", "&#xFA;");
                str = str.Replace("ú", "&#250;");
                str = str.Replace("ú", "&uacute;");
                str = str.Replace("û", "&#251;");
                str = str.Replace("û", "&ucirc;");
                str = str.Replace("ü", "&#xFC;");
                str = str.Replace("ü", "&#252;");
                str = str.Replace("ü", "&uuml;");
                str = str.Replace("ý", "&#253;");
                str = str.Replace("ý", "&yacute;");
                //str = str.Replace("þ", "Icelandic");
                str = str.Replace("þ", "&#254;");
                str = str.Replace("þ", "&thorn;");
                str = str.Replace("ÿ", "&#255;");
                str = str.Replace("ÿ", "&yuml;");
                str = str.Replace("Α", "&Alpha;");
                str = str.Replace("α", "&alpha;");
                str = str.Replace("Β", "&Beta;");
                str = str.Replace("β", "&beta;");
                str = str.Replace("Γ", "&Gamma;");
                str = str.Replace("γ", "&gamma;");
                str = str.Replace("Δ", "&Delta;");
                str = str.Replace("δ", "&delta;");
                str = str.Replace("Ε", "&Epsilon;");
                str = str.Replace("ε", "&epsilon;");
                str = str.Replace("Ζ", "&Zeta;");
                str = str.Replace("ζ", "&zeta;");
                str = str.Replace("Η", "&Eta;");
                str = str.Replace("η", "&eta;");
                str = str.Replace("Θ", "&Theta;");
                str = str.Replace("θ", "&theta;");
                str = str.Replace("Ι", "&Iota;");
                str = str.Replace("ι", "&iota;");
                str = str.Replace("Κ", "&Kappa;");
                str = str.Replace("κ", "&kappa;");
                str = str.Replace("Λ", "&Lambda;");
                str = str.Replace("λ", "&lambda;");
                str = str.Replace("Μ", "&Mu;");
                str = str.Replace("μ", "&mu;");
                str = str.Replace("Ν", "&Nu;");
                str = str.Replace("ν", "&nu;");
                str = str.Replace("Ξ", "&Xi;");
                str = str.Replace("ξ", "&xi;");
                str = str.Replace("Ο", "&Omicron;");
                str = str.Replace("ο", "&omicron;");
                str = str.Replace("Π", "&Pi;");
                str = str.Replace("π", "&pi;");
                str = str.Replace("Ρ", "&Rho;");
                str = str.Replace("ρ", "&rho;");
                str = str.Replace("Σ", "&Sigma;");
                str = str.Replace("σ", "&sigma;");
                str = str.Replace("Τ", "&Tau;");
                str = str.Replace("τ", "&tau;");
                str = str.Replace("Υ", "&Upsilon;");
                str = str.Replace("υ", "&upsilon;");
                str = str.Replace("Φ", "&Phi;");
                str = str.Replace("φ", "&phi;");
                str = str.Replace("Χ", "&Chi;");
                str = str.Replace("χ", "&chi;");
                str = str.Replace("Ψ", "&Psi;");
                str = str.Replace("ψ", "&psi;");
                str = str.Replace("Ω", "&Omega;");
                str = str.Replace("ω", "&omega;");
                str = str.Replace("●", "&#9679;");*/
            }
            return str;
        }

        public static String escapeXML(String xml)
        {
            String newXml = xml;
            newXml = xml.Replace("&", "&amp;");
            return newXml;
        }

        public static String escapeSQL(String str)
        {
            String newStr = str;
            if (str != null)
            {
                newStr = toUTF8(str);
                //String[] badChars = { "\\", "\"", "%" };
                //while (newStr.Contains("\\\\"))
                //    newStr = newStr.Replace("\\\\", "\\");
                //while (newStr.Contains("\"\""))
                //    newStr = newStr.Replace("\"\"", "\"");
                //while (newStr.Contains("%%"))
                //    newStr = newStr.Replace("%%", "%");
                //while (newStr.Contains("''"))
                //    newStr = newStr.Replace("''", "'");
                //foreach (String c in badChars)
                //{
                //    newStr = newStr.Replace(c, "\\" + c);
                //}
                newStr = newStr.Replace("'", "''");
            }
            return newStr;
        }

        public static String getByteSize(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = size;
            int order = 0;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = String.Format("{0:0.##} {1}", len, sizes[order]);
            return result;
        }

#if !SILVERLIGHT
        public static int removeBadChars(ref String file)
        {
            int status = -1;
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                file = file.Replace(c, '_');
            }
            return status;
        }

        // TODO : Move to FileIO
        public static int scrubFile(ref String file)
        {
            int status = -1;
            if (file != null)
            {
                file = Regex.Replace(file, "\\\\", "/", RegexOptions.IgnoreCase);
                file = Regex.Replace(file, "//192.168.0.11/media", "/Media/", RegexOptions.IgnoreCase);
                file = Regex.Replace(file, "//filedbsrv/media", "/Media/", RegexOptions.IgnoreCase);
                file = Regex.Replace(file, "y:/", "/Media/", RegexOptions.IgnoreCase);
                file = Regex.Replace(file, "//filedbsrv.j-a-m.net/media", "/Media/", RegexOptions.IgnoreCase);
                file = Regex.Replace(file, "file:////", "/", RegexOptions.IgnoreCase);
                status = 0;
            }
            return status;
        }

        [Obsolete("Use Utilities.System.runCmd")]
        public static int runCmd(String cmd, String args = "", bool elevate = false)
        {
            int status = -1;
            {
                // Execute Convert.bat on dir.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo(cmd);
                //-really-quiet
                //procStartInfo.WorkingDirectory = g_RootPath + g_Folder;
                procStartInfo.Arguments = args;
                if (elevate)
                {
                    procStartInfo.UseShellExecute = true;
                    procStartInfo.Verb = "runas";
                }
                else
                {
                    // The following commands are needed to redirect the standard output.
                    // This means that it will be redirected to the Process.StandardOutput StreamReader.
                    procStartInfo.RedirectStandardOutput = true;
                    procStartInfo.RedirectStandardError = true;
                    procStartInfo.UseShellExecute = false;
                }
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process g_Proc = new System.Diagnostics.Process();
                g_Proc.StartInfo = procStartInfo;
                //g_Proc.EnableRaisingEvents = true;
                //g_Proc.OutputDataReceived += new DataReceivedEventHandler(receivedData);
                //g_Proc.ErrorDataReceived += new DataReceivedEventHandler(receivedData);
                //g_Proc.Exited += new EventHandler(finishedConvertingVideo);
                string result = "Command Failed";
                try
                {
                    System.Console.WriteLine("Starting command.");
                    if (g_Proc.Start())
                        result = "Executing Command...";
                    while (!g_Proc.HasExited)
                        Thread.Sleep(120);
                    // Get the output into a string
                    //result = g_Proc.StandardOutput.ReadToEnd();
                    //Console.WriteLine(result);
                    //result = g_Proc.StandardError.ReadToEnd();
                    //Console.WriteLine(result);
                    //result = "";
                    //System.Console.WriteLine("Finished converting video. " + g_Folder);
                    //System.Console.WriteLine("Deleting files: " + g_Folder);
                    //procStartInfo.Arguments = "/F /Q *.jpg";
                    //procStartInfo.FileName = "del";
                    //g_Proc.StartInfo = procStartInfo;
                    //g_Proc.Start();
                    //System.Console.WriteLine("Files Deleted: " + g_Folder);
                }
                catch (Win32Exception e)
                {
                    result = "Error:" + e.Message;
                    //g_Stop = true;
                }
                catch (Exception ex)
                {
                    result = "Error:" + ex.Message;
                    //g_Stop = true;
                }
                try
                {
                    if (g_Proc.HasExited)
                    {
                        status = g_Proc.ExitCode;
                    }
                }
                catch (Exception ex)
                {
                }
                System.Console.WriteLine("Result: " + result);
                //System.Console.ReadKey();
            }
            return status;
        }

        public static void ConfigureUnhandledException()
        {
            AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;
        }

        private static void LogUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            Utilities.Logger.log(ex.StackTrace, Utilities.MessageSeverity.ERROR, Utilities.MessageDestination.FILE);
            //Environment.Exit(5);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logLevel">
        /// 0 = Error
        /// 1 = Warning
        /// 2 = Info
        /// 3 = Debug
        /// </param>
        /// <param name="output">
        /// 0 = Console
        /// 1 = File
        /// 2 = System Log
        /// </param>
        public static void debug(string message, int logLevel = (int)MessageSeverity.ERROR, int output = (int)MessageDestination.CONSOLE)
        {
            Logger.log(message, logLevel, output);
        }
        public static void debug(string message,
            MessageSeverity logLevel,
            MessageDestination output)
        {
            Logger.log(message, logLevel, output);
        }

        // TODO : Move to FileIO
        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;
            bool test = false;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                test = true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return test;
        }
#endif
        public static byte[] hexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] HexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < HexAsBytes.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                HexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return HexAsBytes;
        }


#if !SILVERLIGHT
        /// <summary>
        /// The function determines whether the current operating system is a 
        /// 64-bit operating system.
        /// </summary>
        /// <returns>
        /// The function returns true if the operating system is 64-bit; 
        /// otherwise, it returns false.
        /// </returns>
        public static bool Is64BitOperatingSystem()
        {
            if (IntPtr.Size == 8)  // 64-bit programs run only on Win64
            {
                return true;
            }
            else  // 32-bit programs run on both 32-bit and 64-bit Windows
            {
                // Detect whether the current process is a 32-bit process 
                // running on a 64-bit system.
                bool flag;
                return ((DoesWin32MethodExist("kernel32.dll", "IsWow64Process") &&
                    PInvoke.desktop.Kernel32.IsWow64Process(PInvoke.desktop.Kernel32.GetCurrentProcess(), out flag)) && flag);
            }
        }

        /// <summary>
        /// The function determins whether a method exists in the export 
        /// table of a certain module.
        /// </summary>
        /// <param name="moduleName">The name of the module</param>
        /// <param name="methodName">The name of the method</param>
        /// <returns>
        /// The function returns true if the method specified by methodName 
        /// exists in the export table of the module specified by moduleName.
        /// </returns>
        static bool DoesWin32MethodExist(string moduleName, string methodName)
        {
            IntPtr moduleHandle = PInvoke.desktop.Kernel32.GetModuleHandle(moduleName);
            if (moduleHandle == IntPtr.Zero)
            {
                return false;
            }
            return (PInvoke.desktop.Kernel32.GetProcAddress(moduleHandle, methodName) != IntPtr.Zero);
        }
#endif
    }
}
