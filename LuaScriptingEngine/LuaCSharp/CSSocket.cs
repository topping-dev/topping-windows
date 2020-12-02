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
    public class CSSocket
    {
        public static int socket_send(pSocket ps, Lua.CharPtr data, int count, out int sent, pTimeout tm)
        {
            int err = 0;
            sent = 0;
            /* avoid making system calls on closed sockets */
            if (ps == null) return pIO.IO_CLOSED;
            /* loop until we send something or we give up on error */
            for (; ; )
            {
                /* try to send something */
                int put = ps.Send(data, (int)count, 0, out err);
                /* if we sent something, we are done */
                if (put > 0)
                {
                    sent = put;
                    return pIO.IO_DONE;
                }
                if (err > 0) return err;
                else return 0;

                //if ((err = socket_waitfd(ps, WAITFD_W, tm)) != IO_DONE) return err;
            }
            /* can't reach here */
            //return pIO.IO_UNKNOWN;
        }

        public static int socket_recv(pSocket ps, Lua.CharPtr data, int count, out int got, pTimeout tm)
        {
            //p_socket ps, char *data, size_t count, size_t *got, p_timeout tm
            int err;
            got = 0;
            if (ps == null) return pIO.IO_CLOSED;
            for (; ; )
            {
                int taken = ps.Recv(data, (int)count, 0);
                if (taken > 0)
                {
                    got = taken;
                    return pIO.IO_DONE;
                }
                if (taken == 0 || taken == -1) return pIO.IO_CLOSED;
                //if ((err = socket_waitfd(ps, WAITFD_R, tm)) != IO_DONE) return err;
            }
        }

        public static Lua.CharPtr socket_ioerror(pSocket ps, int val)
        {
            return "";
        }
    }
}
