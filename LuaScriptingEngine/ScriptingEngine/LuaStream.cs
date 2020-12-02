using System;
using System.Net;
using System.Windows;
using System.IO;
using LoggerNamespace;
using LuaCSharp;
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

namespace ScriptingEngine
{
    [LuaClass("LuaStream")]
    public class LuaStream : LuaInterface
    {
        private const int INPUTSTREAM = 0;
	    private const int OUTPUTSTREAM = 1;
	
	    private int type = -1;
        private Stream stream = null;
	
	    /*[LuaFunction(Int32.class })
        public static Object Create(Int32 type)
        {
		    LuaStream ls = new LuaStream();
		    if(type == LuaStream.INPUTSTREAM)
			    OutputStream os = new FileOutputStream()
            LuaDatabase db = new LuaDatabase();
            db.db = new DatabaseHelper(context.GetContext());
            return db;
        }*/
	
	    /**
	     * Get stream.
	     * @return InputStream or OutputStream value. 
	     */
	    [LuaFunction(false)]
	    public Object GetStream()
	    {
		    if(type == -1)
		    {
			    Log.e("LuaStream.java", "Stream not set");
			    return null;
		    }
		
		    return stream;
	    }
	
	    /**
	     * Set stream.
	     * @param stream InputStream or OutputStream value.
	     */
	    [LuaFunction(typeof(Object))]
	    public void SetStream(Object stream)
	    {
            Stream strm = (Stream)stream;
            if (strm != null)
            {
                if (strm.CanRead)
                    type = INPUTSTREAM;
                else
                    type = OUTPUTSTREAM;
            }

            this.stream = strm;
	    }
	
	    /**
	     * Reads a single byte from this stream and returns it as an Int32 in the range from 0 to 255. Returns -1 if the end of the stream has been reached. Blocks until one byte has been read, the end of the source stream is detected or an exception is thrown.
	     * @return Int32 value of byte.
	     */
	    [LuaFunction(false)]
	    public Int32 ReadOne()
	    {
		    if(type == OUTPUTSTREAM)
		    {
			    Log.e("LuaStream.java", "Tried to read output stream.");
			    return -1;
		    }
		
		    try
		    {
                return stream.ReadByte();
		    }
		    catch (Exception e) 
		    {
			    Tools.LogException("LuaStream.java", e);
			    return -1;
		    }
	    }
	
	    /**
	     * Reads at most length bytes from this stream and stores them in the byte array b starting at offset.
	     * @param bufferO buffer object.
	     * @param offset offset to start.
	     * @param length length to read.
	     */
	    [LuaFunction(typeof(Object), typeof(Int32), typeof(Int32))]
	    public void Read(LuaBuffer bufferO, Int32 offset, Int32 length)
	    {
		    if(type == OUTPUTSTREAM)
		    {
			    Log.e("LuaStream.java", "Tried to read output stream.");
			    return;
		    }
		
		    byte[] buffer = bufferO.GetBuffer();
		
		    try
		    {
			    stream.Read(buffer, offset, length);
		    }
		    catch (Exception e) 
		    {
			    Tools.LogException("LuaStream.java", e);
		    }
	    }
	
	    /**
	     * Writes a single byte to this stream. Only the least significant byte of the Int32 oneByte is written to the stream.
	     * @param oneByte byte value.
	     */
	    [LuaFunction(typeof(Int32))]
	    public void WriteOne(int oneByte)
	    {
		    if(type == INPUTSTREAM)
		    {
			    Log.e("LuaStream.java", "Tried to write input stream.");
			    return;
		    }
		
		    try
		    {
                stream.WriteByte((byte)oneByte);
		    }
		    catch (Exception e) 
		    {
			    Tools.LogException("LuaStream.java", e);
		    }
	    }
	
	    /**
	     * Writes count bytes from the byte array buffer starting at position offset to this stream.
	     * @param bufferO buffer object.
	     * @param offset offset to start.
	     * @param length length to write.
	     */
	    [LuaFunction(typeof(Object), typeof(Int32), typeof(Int32))]
	    public void Write(LuaBuffer bufferO, Int32 offset, Int32 length)
	    {
		    if(type == INPUTSTREAM)
		    {
			    Log.e("LuaStream.java", "Tried to write input stream.");
			    return;
		    }
		
		    byte[] buffer = bufferO.GetBuffer();
		
		    try
		    {
			    stream.Write(buffer, offset, length);
		    }
		    catch (Exception e) 
		    {
			    Tools.LogException("LuaStream.java", e);
		    }
	    }
        #region LuaInterface Members

        public void RegisterEventFunction(string var, LuaTranslator lt)
        {
        }

        public string GetId()
        {
            return "LuaStream";
        }

        #endregion
    }
}
