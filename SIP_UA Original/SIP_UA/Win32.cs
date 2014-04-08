using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace LumiSoft.SIP.UA
{
    /// <summary>
    /// Unmanaged win32 methods.
    /// </summary>
    internal class Win32
    {
        [DllImport("user32.dll")]
        internal static extern bool FlashWindow(IntPtr hwnd,bool bInvert);
    }
}
