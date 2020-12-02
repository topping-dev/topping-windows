using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using LoggerNamespace;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Net.Sockets;
#else
using Windows.Networking.Sockets;
#endif
namespace LuaCSharp
{
    public class pSocket
    {
#if !NETFX_CORE
        BlockingSocket tcpSocket;
        Socket udpSocket;
        Socket listener;
#else
        StreamSocket tcpSocket;
        DatagramSocket udpSocket;
        StreamSocketListener listener;
#endif
	
	    int type = -1;
	
	    private bool cantSetNonBlocking = false;
	    private bool cantSetNonBlockingValue = true;
	    private bool isNonBlocking = false;
	
	    public const int SOCKET_TYPE_TCP = 0;
	    public const int SOCKET_TYPE_LISTENER = 1;
	    public const int SOCKET_TYPE_UDP = 2;
	    public const int SOCKET_TYPE_TCP_CHANNEL = 3;
	    public const int SOCKET_TYPE_TCP_ANDROID = 4;
	
	    public pSocket(int type)
	    {
		    this.type = type;
		    switch(type)
		    {
			    case SOCKET_TYPE_TCP:
#if !NETFX_CORE
                    tcpSocket = new BlockingSocket();
				    try
				    {
                        tcpSocket.SendBufferSize = pBuffer.BUF_SIZE;
                        tcpSocket.ReceiveBufferSize = pBuffer.BUF_SIZE;
				    }
				    catch (Exception e) 
				    {
					    Log.d("pSocket", e.Message);
				    }
#else
                    tcpSocket = new StreamSocket();
                    try
                    {
                        tcpSocket.Control.OutboundBufferSizeInBytes = (uint)pBuffer.BUF_SIZE;
                    }
                    catch (Exception e) 
				    {
					    Log.d("pSocket", e.Message);
				    }
#endif
				    break;
			    case SOCKET_TYPE_LISTENER:
#if !NETFX_CORE
				    try
				    {
					    listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				    }
				    catch(Exception e)
				    {
					    Log.d("pSocket", e.Message);
				    }
#else
                    try
                    {
                        listener = new StreamSocketListener();
                    }
                    catch (Exception e)
                    {
                    }
#endif
				    break;
			    case SOCKET_TYPE_UDP:
#if !NETFX_CORE
				    try
				    {
					    udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				    }
				    catch(Exception e)
				    {
					    Log.d("pSocket", e.Message);
				    }
#else
                    try
                    {
                        udpSocket = new DatagramSocket();
                    }
                    catch (Exception e)
                    {
                        Log.d("pSocket", e.Message);
                    }
#endif
				    break;
			    /*case SOCKET_TYPE_TCP_CHANNEL:
				    try
				    {
					    tcpSocketChannel = SocketChannel.open();
				    }
				    catch(Exception e)
				    {
					    e.printStackTrace();
				    }
				    break;
			    case SOCKET_TYPE_TCP_ANDROID:
				    try
				    {
					    tcpSocketAndroid = new LuaSocket();
				    }
				    catch(Exception e)
				    {
					    e.printStackTrace();
				    }
				    break;*/
		    }
	    }
	
	    public 
#if !NETFX_CORE
            Socket 
#else
            StreamSocket
#endif
        GetTcpSocket()
	    {
		    if(type == SOCKET_TYPE_TCP)
			    return tcpSocket;
		    /*else if(type == SOCKET_TYPE_TCP_CHANNEL)
			    return tcpSocketChannel.socket();*/
		
		    return null;
	    }
	
	    public void SetTcpSocket(
#if !NETFX_CORE
        BlockingSocket
#else
        StreamSocket
#endif
            value)
	    {
		    tcpSocket = value;
	    }
	
	    public
#if !NETFX_CORE
        Socket 
#else
        DatagramSocket
#endif
            GetUdpSocket()
	    {
		    return udpSocket;
	    }

	    public int Accept(out pSocket sockP, Object object1, Object object2, pTimeout tm)
	    {
		    /*if(listener != null)
		    {
			    try
			    {		
				    sockP.SetTcpSocket(listener.accept());
			    }
			    catch(Exception e)
			    {
				    return pIO.IO_UNKNOWN;
			    }
		    }
		    else
			    return pIO.IO_UNKNOWN;*/
            sockP = new pSocket(0);
		    return pIO.IO_DONE;
	    }

	    public void SetNonBlocking(bool value)
	    {
		    /*if(tcpSocketChannel != null)
		    {
			    try
			    {
				    tcpSocketChannel.socket();
				    tcpSocketChannel.configureBlocking(!value);
				    //isNonBlocking  = value;
			    }
			    catch(Exception e)
			    {
				    Log.e("LuaSocket", "Cannot set blocking, will try later: " + e.getMessage());
				    cantSetNonBlocking = true;
				    cantSetNonBlockingValue = value;
			    }
		    }
		    else if(tcpSocketAndroid != null)
		    {
			    tcpSocketAndroid.SetBlocking(!value);
		    }*/
	    }
	
	    public static byte[] stringToBytesASCII(String str) {
		     char[] buffer = str.ToCharArray();
		     byte[] b = new byte[buffer.Length];
		     for (int i = 0; i < b.Length; i++) {
		      b[i] = (byte) buffer[i];
		     }
		     return b;
	    }

	    public static byte[] stringToBytesUTFCustom(String str) {
		     char[] buffer = str.ToCharArray();
		     byte[] b = new byte[buffer.Length << 1];
		     for(int i = 0; i < buffer.Length; i++) {
		      int bpos = i << 1;
		      b[bpos] = (byte) ((buffer[i]&0xFF00)>>8);
		      b[bpos + 1] = (byte) (buffer[i]&0x00FF);
		     }
		     return b;
	    }
	
	    public static String bytesToStringUTFCustom(byte[] bytes) {
		     char[] buffer = new char[bytes.Length >> 1];
		     for(int i = 0; i < buffer.Length; i++) {
		      int bpos = i << 1;
		      char c = (char)(((bytes[bpos]&0x00FF)<<8) + (bytes[bpos+1]&0x00FF));
		      buffer[i] = c;
		     }
		     return new String(buffer);
	    }


	    public int Send(Lua.CharPtr data, int count, int flag, out int errP)
	    {
            if (tcpSocket != null)
            {
                try
                {
#if !NETFX_CORE
                    tcpSocket.Send(data.toByteArray(), count, out errP);
#else
                    //TODO:
                    errP = 0;
#endif
                    return count;
                }
                catch (Exception e)
                {
                    errP = 1;
                }
            }
            errP = 0;
		    return 0;
	    }

	    public int Recv(Lua.CharPtr data, int count, int flag)
	    {
		    if(tcpSocket != null)
		    {
			    byte[] arr = new byte[count];
			    try
			    {
#if !NETFX_CORE
				    int read = tcpSocket.Read(ref arr);
				    data.setByteArray(arr);
                    return read;
#else
                    //TODO:
#endif
                }
			    catch(Exception e)
			    {
				    //Tools.LogException("pSocket", e);
			    }
		    }
		    /*else if(tcpSocketChannel != null)
		    {
			    byte[] arr = new byte[count];
			    try
			    {
				    int read = 0;
					read = tcpSocketChannel.read(ByteBuffer.wrap(arr));
				    data.setByteArray(arr);
				    return read;
			    }
			    catch (Exception e) 
			    {
				    Tools.LogException("pSocket", e);
			    }
		    }
		    else if(tcpSocketAndroid != null)
		    {
			    tcpSocketAndroid.Recv(data, count, null);
		    }*/
		    return 0;
	    }

	    public Lua.CharPtr Bind(Lua.CharPtr address, int port)
	    {
		    if(tcpSocket != null)
		    {
			    try
			    {
#if !NETFX_CORE
                    tcpSocket.Bind(new DnsEndPoint(address.ToString(), port));
#else
                    //TODO:
#endif
                }
			    catch(Exception e)
			    {
				    return address;
			    }
		    }
		    return null;
	    }

	    public Lua.CharPtr Connect(Lua.CharPtr address, int port, pTimeout tm)
	    {
		    if(tcpSocket != null)
		    {
			    try
			    {
				    int timeout = (int) tm.block * 1000;
				    if(timeout < 0)
					    timeout = 0;
#if !NETFX_CORE
                    tcpSocket.Connect(new DnsEndPoint(address.ToString(), port), timeout);
#else
                    //TODO:
#endif
			    }
			    catch(Exception e)
			    {
				    return address;
			    }
		    }
		    /*else if(tcpSocketChannel != null)
		    {
			    try
			    {
				    int timeout = (int) tm.block * 1000;
				    if(timeout < 0)
					    timeout = 0;
				    tcpSocketChannel.configureBlocking(true);
				    boolean connected = tcpSocketChannel.connect(new InetSocketAddress(InetAddress.getByName(address.toString()), port));
				    while(!tcpSocketChannel.finishConnect())
				    {
					    Thread.sleep(50);
				    }
				    tcpSocketChannel.socket().setSoTimeout(timeout);
				    tcpSocketChannel.socket().setKeepAlive(true);
				    tcpSocketChannel.socket().setTcpNoDelay(true);
				    //tcpSocketWriter = new BufferedWriter(new OutputStreamWriter(tcpSocketChannel.socket().getOutputStream()));
				    if(cantSetNonBlocking)
				    {
					    SetNonBlocking(cantSetNonBlockingValue);
					    cantSetNonBlocking = false;
				    }
			    }
			    catch (Exception e) 
			    {
				    return address;
			    }
		    }*/
		    return null;
	    }

	    public void Destroy()
	    {
		    if(tcpSocket != null)
		    {
			    try
			    {
#if !NETFX_CORE
				    tcpSocket.Shutdown(SocketShutdown.Both);
#else
                    //TODO:
#endif
			    }
			    catch(Exception e)
			    {
				    //Tools.LogException("pSocket", e);
			    }
		    }
		    /*else if(tcpSocketChannel != null)
		    {
			    try
			    {
				    tcpSocketChannel.close();
			    }
			    catch (Exception e) 
			    {
				    Tools.LogException("pSocket", e);
			    }
		    }*/
	    }

	    public int Listen(int backlog)
	    {
		    return pIO.IO_DONE;
	    }
	
	    public void Shutdown(int flag)
	    {
		    try
		    {
			    if(tcpSocket != null)
			    {
#if !NETFX_CORE
				    switch(flag)
				    {
					    case 0:
						    tcpSocket.Shutdown(SocketShutdown.Receive);
						    break;
					    case 1:
						    tcpSocket.Shutdown(SocketShutdown.Send);
						    break;
					    default:
                            tcpSocket.Shutdown(SocketShutdown.Both);
						    break;
				    }
#else
                    //TODO:
#endif
			    }
			    /*else if(tcpSocketChannel != null)
			    {
				    switch(flag)
				    {
					    case 0:
						    tcpSocketChannel.socket().shutdownInput();
						    break;
					    case 1:
						    tcpSocketChannel.socket().shutdownOutput();
						    break;
					    case 2:
					    default:
						    tcpSocketChannel.socket().shutdownInput();
						    tcpSocketChannel.socket().shutdownOutput();
						    break;
				    }
			    }*/
		    }
		    catch (Exception e) 
		    {
			
		    }
	    }
	
	    public Lua.CharPtr GetPeerName(ref int portP)
	    {
		    return new Lua.CharPtr("");
	    }

        public Lua.CharPtr GetSockName(ref int portP)
	    {
            return new Lua.CharPtr("");
	    }
	
	    public void SetTimeout(int val)
	    {
		    int timeout = val;
		    if(timeout == 0)
			    timeout = 1;
		    else if(timeout < 0)
			    timeout = 0;
			
		    if(tcpSocket != null)
		    {
#if !NETFX_CORE
                tcpSocket.setSoTimeout(timeout);			    
#else
                //TODO:
#endif
		    }
		    /*else if(tcpSocketChannel != null)
		    {
			    try
			    {
				    tcpSocketChannel.socket().setSoTimeout(timeout);
			    }
			    catch(SocketException e)
			    {
				    e.printStackTrace();
			    }
		    }*/
	    }
    }
}
