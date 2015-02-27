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

        // This method will be called when the thread is started.
        public void DoWork()
        {
            if (m_Dialog != null)
            {
                m_Dialog.Show();
                while (!_shouldStop)
                {
                    Application.DoEvents();
                    Thread.Sleep(40);
                }
                m_Dialog.Hide();
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
