using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace Utilities
{
    public class Send2Wnd
    {
        readonly static String[] m_WndClasses = { "GxWindowClassD3d", "IEFrame" };
        // Class Types
        public const int D3D = 0;
        public const int IE = 1;

        public static int sendKeys(String key, String wndTitle, int type = 0)
        {
            int hwnd = -1;
            //int hwnd = FindWindow(m_WndClasses[type], wndTitle);
            foreach (Process proc in Process.GetProcesses())
            {
                STRINGBUFFER windowTitle;
                GetWindowText(proc.MainWindowHandle, out windowTitle, 256);
                if (windowTitle.szText.Contains(wndTitle))
                {
                    hwnd = proc.MainWindowHandle.ToInt32();
                    break;
                } 
                hwnd = findChild(proc.MainWindowHandle, wndTitle, type);
            }

            if (hwnd != -1)
            {
                SetForegroundWindow(hwnd);
                Thread.Sleep(20);
                System.Windows.Forms.SendKeys.SendWait(key);
            }
            return hwnd;
        }

        public static void sendKeys(String key, int hwnd, int type = 0)
        {
            if (hwnd != -1)
            {
                SetForegroundWindow(hwnd);
                System.Windows.Forms.SendKeys.SendWait(key);
                Thread.Sleep(20);
            }
        }

        protected static int findChild(IntPtr handle, String windowTitle, int type)
        {
            int hwnd = -1;
            IntPtr hNext = IntPtr.Zero;

            do
            {
                hNext = FindWindowEx(handle,hNext,
                m_WndClasses[type], IntPtr.Zero);

                // we've got a hwnd to play with
                if ( !hNext.Equals(IntPtr.Zero) )
                {
                    // get window caption
                    STRINGBUFFER sLimitedLengthWindowTitle;
                    GetWindowText(hNext, out 
                    sLimitedLengthWindowTitle, 256);

                    String sWindowTitle = sLimitedLengthWindowTitle.szText;
                    if (sWindowTitle.Length>0)
                    {
                        if (sWindowTitle.StartsWith(windowTitle))
                            hwnd = hNext.ToInt32();
                    }
                }
            } 
            while (!hNext.Equals(IntPtr.Zero));
            return hwnd;
        }


        // The FindWindow function retrieves a handle
        // to the top-level window whose class name
        // and window name match the specified strings.
        // This function does not search child windows.
        // This function does not perform a case-sensitive search.
        [DllImport("User32.dll")]
        public static extern int FindWindow(string strClassName,
                                                 string strWindowName);
        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd,
            out STRINGBUFFER ClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindowEx(IntPtr parent /*HWND*/,
                                                 IntPtr next /*HWND*/,
                                                 string sClassName,
                                                 IntPtr sWindowTitle);

        // used for an output LPCTSTR parameter on a method call
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct STRINGBUFFER
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szText;
        }
    }
}
