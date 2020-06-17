using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class Convert
    {
        public static String ToHexString(byte[] bytes)
        {
            String hex = "";
            for (int i = 0; i < bytes.Length; i++)
                hex += bytes[i].ToString("X2"); /* hex format */
            return hex;
        }

        public static DateTime FromUnixTime(Int32 seconds)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(seconds);
            return dtDateTime;
        }

        public static DateTime FromUnixTime(long milliseconds)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddMilliseconds(milliseconds);
            return dtDateTime;
        }

        public static Int32 ToUnixTime(DateTime time)
        {
            return ToUnixTimeSeconds(time);
        }

        public static Int32 ToUnixTimeSeconds(DateTime time)
        {
            return (Int32)(time.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds;
        }

        public static long ToUnixTimeMilliseconds(DateTime time)
        {
            return (long)(time - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        protected static object GetDefault(Type type)
        {
            if(type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
        public static Object ToType(object value, Type t)
        {
            var result = GetDefault(t);
            if (value != null)
            {
                if (t == typeof(String))
                    result = System.Convert.ChangeType(value.ToString(), t);
                if (t == typeof(int))
                    result = System.Convert.ChangeType(ToInt(value), t);
                if (t == typeof(decimal))
                    result = System.Convert.ChangeType(ToDecimal(value), t);
                if (t == typeof(float))
                    result = System.Convert.ChangeType(ToFloat(value), t);
                if (t == typeof(double))
                    result = System.Convert.ChangeType(ToDouble(value), t);
                if (t == typeof(long))
                    result = System.Convert.ChangeType(ToLong(value), t);
                if (t == typeof(DateTime))
                    result = System.Convert.ChangeType(ToDateTime(value), t);
                if (t == typeof(bool))
                    result = System.Convert.ChangeType(ToBool(value), t);
            }
            return result;
        }

        public static String ToString(Object value)
        {
            String result = "";
            if (value is String)
                result = (String)value;
            return result;
        }

        public static DateTime ToDateTime(Object value)
        {
            DateTime result = DateTime.MinValue;
            if (value is DateTime)
                result = (DateTime)value;
            else if (value is String)
            {
                if (!DateTime.TryParse((String)value, out result))
                {
                    String str = value as string;
                    try
                    {
                        result = DateTime.ParseExact(str, "yyyy-MM-dd h-tt", CultureInfo.InvariantCulture);
                    }
                    catch(Exception ex)
                    {

                    }
                }
            }
            return result;
        }

        public static bool ToBool(Object value)
        {
            bool result = false;
            if (value is bool)
                result = (bool)value;
            else if (value is String)
            {
                if(!bool.TryParse((String)value, out result))
                {
                    int val = ToInt(value);
                    result = ((double)val == 0 ? false : true);
                }
            }
            else if (Util.isNumber(value) ||
                value is SByte)
            {
                result = (System.Convert.ToDouble(value) == 0 ? false : true);
            }
            return result;
        }

        public static int ToInt(Object value)
        {
            int result = 0;
            if (value is int)
                result = (int)value;
            else if(value is String)
            {
                int.TryParse((String)value,
                    System.Globalization.NumberStyles.AllowThousands,
                    CultureInfo.CurrentCulture, out result);
            }
            else if(Util.isNumber(value))
            {
                result = (int)System.Convert.ToInt32(value);
            }
            return result;
        }

        public static long ToLong(Object value)
        {
            long result = 0;
            if (value is long)
                result = (long)value;
            else if (value is String)
            {
                long.TryParse((String)value,
                    System.Globalization.NumberStyles.AllowThousands,
                    CultureInfo.CurrentCulture, out result);
            }
            else if (Util.isNumber(value))
            {
                result = (long)System.Convert.ToInt64(value);
            }
            return result;
        }

        public static decimal ToDecimal(Object value)
        {
            decimal result = 0;
            if (value is decimal)
                result = (decimal)value;
            else if (value is String)
            {
                decimal dec = 0;
                var d = Decimal.TryParse((String)value,
                    System.Globalization.NumberStyles.Currency &
                    NumberStyles.Float,
                    CultureInfo.CurrentCulture,
                    out dec);
                result = (decimal)dec;
            }
            else if (Util.isNumber(value))
            {
                result = (decimal)System.Convert.ToDouble(value);
            }
            return result;
        }

        public static float ToFloat(Object value)
        {
            float result = 0;
            if (value is float)
                result = (float)value;
            else if (value is String)
            {
                decimal dec = 0;
                var d = Decimal.TryParse((String)value,
                    System.Globalization.NumberStyles.Any,
                    CultureInfo.CurrentCulture,
                    out dec);
                result = (float)dec;
            }
            else if (Util.isNumber(value))
            {
                result = (float)System.Convert.ToDouble(value);
                if (float.IsNaN(result))
                    result = 0;
            }
            if (float.IsInfinity(result))
                result = 0;
            if (float.IsNaN(result))
                result = 0;
            return result;
        }

        public static double ToDouble(Object value)
        {
            double result = 0;
            if (value is double)
                result = (double)value;
            else if (value is String)
            {
                decimal dec = 0;
                var d = Decimal.TryParse((String)value,
                    System.Globalization.NumberStyles.Currency |
                    NumberStyles.Float,
                    CultureInfo.CurrentCulture,
                    out dec);
                result = (double)dec;
            }
            else if (Util.isNumber(value))
            {
                result = System.Convert.ToDouble(value);
                if (double.IsNaN(result))
                    result = 0;
            }
            if (double.IsInfinity(result))
                result = 0;
            if (double.IsNaN(result))
                result = 0;
            return result;
        }

        public static decimal TruncateDecimal(decimal value, int precision)
        {
            decimal step = (decimal)Math.Pow(10, precision);
            decimal tmp = Math.Truncate(step * value);
            return tmp / step;
        }
    }
}
