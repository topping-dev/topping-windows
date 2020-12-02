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
    public class pIO
    {
        public const int IO_DONE = 0;        /* operation completed successfully */
	    public const int IO_TIMEOUT = -1;    /* operation timed out */
	    public const int IO_CLOSED = -2;     /* the connection has been closed */
	    public const int IO_UNKNOWN = -3;
	
	    public pSocket ctx;
        public delegate int sendD(pSocket sps, Lua.CharPtr data, int count, out int done, pTimeout tm);
        public sendD send;
        public delegate int recvD(pSocket sps, Lua.CharPtr data, int count, out int got, pTimeout tm);
        public recvD recv;
        public delegate Lua.CharPtr errorD(pSocket sps, int err);
        public errorD error;
    }
}
