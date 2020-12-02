using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
#else
using Windows.UI.Xaml.Controls;
#endif

namespace LuaCSharp
{
    public class pBuffer
    {
        public static int BUF_SIZE = 8192;

        public double birthday;        /* throttle support info: creation time, */
        public long sent, received;  /* bytes sent, and bytes received */
        public pIO io;                /* IO driver used for this buffer */
        public pTimeout tm;           /* timeout management for this buffer */
        public int first, last;     /* index of first and last bytes of stored data */
        public Lua.CharPtr data;    /* storage space for buffer data */

        public pBuffer()
        {
            data = new Lua.CharPtr(new char[BUF_SIZE]);
        }
    }
}
