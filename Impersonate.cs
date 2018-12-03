using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.ComponentModel;

namespace Utilities
{
    public class Impersonate
    {
        WindowsImpersonationContext impersonationContext;

        public Impersonate()
        {
        }

        public bool impersonateValidUser(String userName, String domain, String password)
        {
            WindowsIdentity tempWindowsIdentity;
            IntPtr token = IntPtr.Zero;
            IntPtr tokenDuplicate = IntPtr.Zero;

            if (PInvoke.desktop.Advapi32.RevertToSelf())
            {
                if (PInvoke.desktop.Advapi32.LogonUserA(userName, domain, password, 
                    PInvoke.Constants.LOGON32_LOGON_INTERACTIVE,
                    PInvoke.Constants.LOGON32_PROVIDER_DEFAULT, ref token) != 0)
                {
                    if (PInvoke.desktop.Advapi32.DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                    {
                        tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                        impersonationContext = tempWindowsIdentity.Impersonate();
                        if (impersonationContext != null)
                        {
                            PInvoke.desktop.Kernel32.CloseHandle(token);
                            PInvoke.desktop.Kernel32.CloseHandle(tokenDuplicate);
                            return true;
                        }
                    }
                }
            }
            if (token != IntPtr.Zero)
                PInvoke.desktop.Kernel32.CloseHandle(token);
            if (tokenDuplicate != IntPtr.Zero)
                PInvoke.desktop.Kernel32.CloseHandle(tokenDuplicate);
            return false;
        }

        public void undoImpersonation()
        {
            impersonationContext.Undo();
        }

        private string m_Domain;
        private string m_Password;
        private string m_Username;
        private IntPtr m_Token;

        private WindowsImpersonationContext m_Context = null;


        protected bool IsInContext
        {
            get { return m_Context != null; }
        }

        public Impersonate(string domain, string username, string password)
        {
            m_Domain = domain;
            m_Username = username;
            m_Password = password;
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public void Enter()
        {
            if (this.IsInContext) return;
            m_Token = new IntPtr(0);
            try
            {
                m_Token = IntPtr.Zero;
                int logonSuccessfull = PInvoke.desktop.Advapi32.LogonUserA(
                m_Username,
                m_Domain,
                m_Password,
                PInvoke.Constants.LOGON32_LOGON_INTERACTIVE,
                PInvoke.Constants.LOGON32_PROVIDER_DEFAULT,
                ref m_Token);
                if (logonSuccessfull < 0)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(error);
                }
                WindowsIdentity identity = new WindowsIdentity(m_Token);
                m_Context = identity.Impersonate();
            }
            catch (Exception exception)
            {
                // Catch exceptions here
                Utilities.Logger.log(exception.Message);
            }
        }


        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public void Leave()
        {
            if (this.IsInContext == false) return;
            m_Context.Undo();

            if (m_Token != IntPtr.Zero)
                PInvoke.desktop.Kernel32.CloseHandle(m_Token);
            m_Context = null;
        }
    }
}