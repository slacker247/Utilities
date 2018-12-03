using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;


namespace Utilities.db
{
	public class Object : IComparable
	{
		protected Dictionary<String, object> m_Propteries = new Dictionary<String, object>();
		protected String m_SortBy = "";
		protected int m_SortOrder = 0;

		public int SortOrder
		{
			get
			{
				return m_SortOrder;
			}
			set
			{
				m_SortOrder = value;
			}
		}

        /// <summary>
        /// Property Getter method.
        /// </summary>
        /// <returns>The value of the class property.</returns>
        public virtual long getId()
        {
            object obj = get("Id");
            if (obj == null)
                obj = -1L;
            return (long)obj;
        }

        /// <summary>
        /// Property Setter method.
        /// </summary>
        /// <param name="value">Value to store in the class property.</param>
        public void setId(long value)
        {
            if (value >= 0)
                set("Id", value);
        }

		public object get(String name)
		{
			object obj = null;
			if (m_Propteries.ContainsKey(name))
				obj = m_Propteries[name];
			return obj;
		}

        /// <summary>
        /// Convert and return the property as a boolean.  Convert an integer, SByte, etc. if found to a boolean.
        /// </summary>
        /// <param name="name">The property to return</param>
        /// <returns></returns>
        public bool getBoolean(String name)
        {
            object obj = get(name);
            if (obj == null)
                obj = false;
            if (obj is int)
                obj = (0 != ((int) obj));
            else if (obj is SByte)
                obj = (0 != ((SByte) obj));
            return (bool)obj;
        }

		public long set(String name, object value)
		{
			long status = -1;
			m_Propteries[name] = value;
			return status;
		}

		public String[] getPropertyNames()
		{
			String[] keys = new String[m_Propteries.Keys.Count];
			m_Propteries.Keys.CopyTo(keys, 0);
			return keys;
		}

        public static String toXmlValueFormat(String original)
        {
            // TODO: implement thorough XML format code for values that might have XML characters
            // return (XmlConvert.EncodeName(original)); // does too much!
            String encoded = original.Replace("<", "&lt;");
            encoded = encoded.Replace(">", "&gt;");
            return encoded;
        }
        
        public static String fromXmlValueFormat(String original)
        {
            // TODO: implement thorough XML format code for values that might have XML characters
            // return (XmlConvert.DecodeName(original)); // does too much!
            String decoded = original.Replace("&lt;", "<");
            decoded = decoded.Replace("&gt;", ">");
            return decoded;
        }
        
        public static String dateToXmlFormat(DateTime? date)
        {
            if (date == null)
            {
                return null;
            }
            return dateToXmlFormat(date.Value);
        }

        public static String dateToXmlFormat(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }

		public int CompareTo(object obj)
		{
			int status = -1;
			
			if (obj is Object)
			{
				Object p = (Object)obj;
				object param = this.get(m_SortBy);
				if (param == null)
				{
					status = -1;
				}
				else if (p.get(m_SortBy) == null)
				{
					status = 1;
				}
				else if (param is long)
				{
					if (SortOrder == 1)
						status = ((long)p.get(m_SortBy)).CompareTo(param);
					else
						status = ((long)param).CompareTo(p.get(m_SortBy));
				}
				else if (param is String)
				{
					if (SortOrder == 1)
						status = ((String)p.get(m_SortBy)).CompareTo(param);
					else
						status = ((String)param).CompareTo(p.get(m_SortBy));
				}
				else if (param is int)
				{
					if (SortOrder == 1)
						status = ((int)p.get(m_SortBy)).CompareTo(param);
					else
						status = ((int)param).CompareTo(p.get(m_SortBy));
				}
				else if (param is float)
				{
					object toParam = p.get(m_SortBy);
					if (SortOrder == 1)
						status = ((float)toParam).CompareTo(param);
					else
						status = ((float)param).CompareTo(toParam);
				}
				else if (param is double)
				{
					object toParam = p.get(m_SortBy);
					if (SortOrder == 1)
						status = ((double)toParam).CompareTo(param);
					else
						status = ((double)param).CompareTo(toParam);
				}
				else if (param is DateTime)
				{
					if (SortOrder == 1)
						status = ((DateTime)p.get(m_SortBy)).CompareTo(param);
					else
						status = ((DateTime)param).CompareTo(p.get(m_SortBy));
				}
				else if (param is bool)
				{
					if (SortOrder == 1)
						status = ((bool)p.get(m_SortBy)).CompareTo(param);
					else
						status = ((bool)param).CompareTo(p.get(m_SortBy));
				}
			}

			return status;
		}
	}
}
