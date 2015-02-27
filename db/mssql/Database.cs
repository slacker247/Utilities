using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Threading;
using System.Data.SqlTypes;

namespace Utilities.db
{
    public class Database
    {
        protected SqlConnection m_Conn;
        protected SqlCommand m_Cmd;
        protected SqlDataReader m_Reader;
        protected String m_Database = "Media";
        protected String m_Server = "192.168.0.11";
        protected String m_User = "webuser";
        protected String m_Password = "pass";
        protected bool m_PersistSecurityInfo = true;

        protected String m_TableName = "Object";

        public List<Object> m_Data = new List<Object>();

        public Database()
        {
            // FIX : This should be retrieved from a settings file.
            m_Conn = new SqlConnection(@"Data " +
                                        "Source=" + m_Server +
                                        ";Initial Catalog=" + m_Database +
                                        ";Persist Security Info=True;" +
                                        "User ID=" + m_User +
                                        ";Password=" + m_Password);
        }

        ~Database()
        {
            close();
        }

        public virtual String getFullTableName()
        {
            return getLongTableName();
        }

        public virtual String getLongTableName()
        {
            String tlbName = "";
            tlbName += "[" + m_Database + "].[dbo].[" + this.m_TableName + "]";
            return tlbName;
        }

        public String connect()
        {
            String status = null;
            bool connected = false;
            do
            {
                try
                {
                    m_Conn.Open();
                    status = "success";
                }
                catch (SqlException e)
                {
                    Console.WriteLine("Database1: Error: " + e.Message);
                    status = e.Message;
                }
                catch (InvalidOperationException ex)
                {
                    status = ex.Message;
                    if (ex.Message == "Timeout expired.  The timeout period elapsed prior to obtaining a connection from the pool.  This may have occurred because all pooled connections were in use and max pool size was reached.")
                    {
                        SqlConnection.ClearAllPools();
                        Thread.Sleep(200);
                    }
                }
            } while (status != "success");
            return status;
        }

        protected virtual long executeSql(String sql)
        {
            long status = -1;
            m_Cmd = new SqlCommand(sql);
            status = executeSql(m_Cmd);
            return status;
        }

        protected virtual long executeSql(SqlCommand cmd)
        {
            long status = -1;
            if (m_Conn.State != ConnectionState.Open)
            {
                connect();
            }
            if (m_Conn.State == ConnectionState.Open)
            {
                try
                {
                    cmd.Connection = m_Conn;
                    if (cmd.CommandText.ToLower().Contains("insert"))
                    {
                        if (!cmd.CommandText.EndsWith(";"))
                            cmd.CommandText += ";";
                        cmd.CommandText += "SELECT SCOPE_IDENTITY() AS [SCOPE_IDENTITY];";
                        object obj = cmd.ExecuteScalar();
                        long.TryParse(obj.ToString(), out status);
                    }
                    else
                    {
                        m_Reader = cmd.ExecuteReader();
                        while (m_Reader.Read())
                        {
                            loadRow();
                        }
                        m_Reader.Close();
                    }
                }
                catch (SqlException e)
                {
                    status = e.ErrorCode * (-1);
                    Utilities.Util.debug(e.Message);
                }
                catch (SqlTypeException ste)
                {
                    status = -2;
                    Utilities.Util.debug(ste.Message);
                }
                catch (Exception ex)
                {
                    status = -3;
                    Utilities.Util.debug(ex.Message);
                }
            }
            return status;
        }

        protected virtual int loadRow()
        {
            int status = -1;
            //m_Data.Add(new Object());  // Must override
            for (int i = 0; i < m_Reader.FieldCount; i++)
            {
                String column = m_Reader.GetName(i);
                if (m_Reader[column] != null && m_Reader[column] != DBNull.Value)
                {
                    m_Data[m_Data.Count - 1].set(column, m_Reader[column]);
                }
            }
            return status;
        }

        protected virtual int popSQL(Object data)
        {
            int status = -1;
            if (data != null)
            {
                foreach (String prop in data.getPropertyNames())
                {
                    if (data.get(prop) != null)
                        m_Cmd.Parameters.Add("@" + prop + "", SqlDbType.VarChar).Value = data.get(prop);
                }
            }
            return status;
        }

        public Object getById(long id)
        {
            Object data = null;
            m_Data.Clear();
            m_Cmd = m_Conn.CreateCommand();
            m_Cmd.CommandText = "Select * From " + this.getFullTableName() + " ";
            m_Cmd.CommandText += "Where id=" + id;
            m_Cmd.CommandText += ";";
            //popSQL(data);
            executeSql(m_Cmd);
            if (m_Data.Count > 0)
                data = m_Data[0];
            return data;
        }

        public void getByProp(String prop, Object data)
        {
            m_Data.Clear();
            if (data.get(prop) != null)
            {
                m_Cmd = m_Conn.CreateCommand();
                m_Cmd.CommandText = "Select * From " + this.getFullTableName() + " ";
                m_Cmd.CommandText += "Where [" + prop + "]=@" + prop + "";
                m_Cmd.CommandText += ";";
                popSQL(data);
                executeSql(m_Cmd);
            }
        }

        public virtual bool exists(ref Object data)
        {
            bool status = false;
            getByProp("Name", data);
            if (m_Data.Count > 0)
            {
                data.setId(m_Data[0].getId());
                status = true;
            }
            return status;
        }

        public virtual long save(ref Object data)
        {
            long status = -1;
            if (exists(ref data))
            {
                status = update(data);
            }
            else
            {
                status = add(data);
                data.setId(status);
            }
            return status;
        }

        public int loadAll()
        {
            int status = -1;
            m_Data.Clear();
            status = (int)executeSql("SELECT * FROM " + this.getFullTableName() + ";");
            return status;
        }

        public long add(Object data)
        {
            long status = -1;
            if (data.getId() == -1)
            {
                m_Cmd = m_Conn.CreateCommand();
                m_Cmd.CommandText = "INSERT INTO " + this.getFullTableName() + " (";
                foreach (String prop in data.getPropertyNames())
                {
                    if (prop.ToLower() == "id" ||
                        data.get(prop) == null)
                        continue;
                    m_Cmd.CommandText += "[" + prop + "], ";
                }
                m_Cmd.CommandText = m_Cmd.CommandText.Substring(0, m_Cmd.CommandText.Length - 2);
                m_Cmd.CommandText += ") ";
                m_Cmd.CommandText += "Values(";
                foreach (String prop in data.getPropertyNames())
                {
                    if (prop.ToLower() == "id" ||
                        data.get(prop) == null)
                        continue;
                    m_Cmd.CommandText += "@" + prop + ", ";
                }
                m_Cmd.CommandText = m_Cmd.CommandText.Substring(0, m_Cmd.CommandText.Length - 2);
                m_Cmd.CommandText += ");";
                popSQL(data);
                status = executeSql(m_Cmd);
            }
            return status;
        }

        public int update(Object data)
        {
            int status = -1;
            // TODO : This does not work right
            if (data.get("id") != null &&
                ((long)data.get("id")) > -1)
            {
                m_Cmd = m_Conn.CreateCommand();
                m_Cmd.CommandText = "UPDATE " + this.getFullTableName() + " SET ";
                foreach (String prop in data.getPropertyNames())
                {
                    if (prop.ToLower() == "id")
                        continue;
                    m_Cmd.CommandText += "[" + prop + "]=@" + prop + ", ";
                }
                m_Cmd.CommandText = m_Cmd.CommandText.Substring(0, m_Cmd.CommandText.Length - 2);
                m_Cmd.CommandText += " Where id=@id";
                m_Cmd.CommandText += ";";
                popSQL(data);
                executeSql(m_Cmd);
                status = 0;
            }
            return status;
        }

        public int delete(Object data)
        {
            int status = -1;
            String sql = "DELETE FROM " + this.getFullTableName() + " WHERE Id=" + data.getId();
            executeSql(sql);
            return status;
        }

        public void close()
        {
            try
            {
                m_Conn.Close();
                SqlConnection.ClearAllPools();
            }
            catch (SqlException e)
            {
                Console.WriteLine("Database3: Error: " + e.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database4: Error: " + ex.Message);
            }
            finally
            {
                m_Conn.Dispose();
            }
        }
    }
}
