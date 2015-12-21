using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.db.mssql
{
    public class DBServer
    {
        protected String m_Database = "Media";
        protected String m_Server = "192.168.0.11";
        protected String m_User = "webuser";
        protected String m_Password = "pass";
        protected bool m_PersistSecurityInfo = true;
    }
}
