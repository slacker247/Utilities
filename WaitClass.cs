using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Utilities
{
    public class WaitClass
    {
        protected Form m_Dialog = null;

        public WaitClass(Form dlg)
        {
            m_Dialog = dlg;
        }

        protected delegate void showDlgDelegate(bool show);
        protected void showDlg(bool show)
        {
            if (m_Dialog.InvokeRequired)
            {
                if (show)
                    m_Dialog.Show();
                else
                    m_Dialog.Hide();
            }
            else
            {
                if(m_Dialog.IsHandleCreated)
                    m_Dialog.Invoke(new showDlgDelegate(showDlg),
                        new object[] {show});
                else if(m_Dialog.ParentForm != null && m_Dialog.ParentForm.IsHandleCreated)
                    m_Dialog.ParentForm.Invoke(new showDlgDelegate(showDlg),
                        new object[] {show});
            }
        }

        // This method will be called when the thread is started.
        public void DoWork()
        {
            if (m_Dialog != null)
            {
                showDlg(true);
                while (!_shouldStop)
                {
                    Application.DoEvents();
                    Thread.Sleep(40);
                }
                showDlg(false);
            }
            _shouldStop = false;
        }
        public void RequestStop()
        {
            _shouldStop = true;
        }
        // Volatile is used as hint to the compiler that this data
        // member will be accessed by multiple threads.
        private volatile bool _shouldStop;
    }
}
