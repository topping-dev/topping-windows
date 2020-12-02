using System;
using System.Net;
using System.Windows;
#if !NETFX_CORE

using System.Threading;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Net.Sockets;

namespace LuaCSharp
{
    public class BlockingSocket : Socket
    {
        SocketAsyncEventArgs saea;
        Mutex mtx = new Mutex(false, "BlockingTcp");
        int lastError = 0;
        int timeout = 60000;

        public BlockingSocket()
            : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            saea = new SocketAsyncEventArgs();
            saea.Completed += new EventHandler<SocketAsyncEventArgs>(saea_Completed);
        }

        void saea_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                switch (e.LastOperation)
                {
                    case SocketAsyncOperation.Send:
                        {

                        } break;
                    case SocketAsyncOperation.Receive:
                        {
                        } break;
                }
            }
            else
                lastError = 1;
            mtx.ReleaseMutex();
        }

        public void Connect()
        {
        }

        public void Bind(EndPoint ep)
        {
            saea.RemoteEndPoint = ep;
        }

        public void Connect(EndPoint ep, int timeout)
        {
            saea.RemoteEndPoint = ep;
            this.timeout = timeout;
        }

        public int Send(byte[] data, int count, out int errP)
        {
            mtx.WaitOne(timeout);
            saea.SetBuffer(data, 0, count);
            SendAsync(saea);
            mtx.WaitOne(timeout);
            if (lastError != 0)
            {
                errP = lastError;
                lastError = 0;
                mtx.ReleaseMutex();
                throw new Exception();
            }
            errP = 0;
            mtx.ReleaseMutex();
            return 0;
        }

        public int Read(ref byte[] data)
        {
            mtx.WaitOne(timeout);
            ReceiveAsync(saea);
            mtx.WaitOne(timeout);
            data = saea.Buffer;
            if (lastError != 0)
            {
                lastError = 0;
                mtx.ReleaseMutex();
                throw new Exception();
            }
            mtx.ReleaseMutex();
            return data.Length;
        }

        public void setSoTimeout(int timeout)
        {
            this.timeout = timeout;
        }
    }
}

#endif
