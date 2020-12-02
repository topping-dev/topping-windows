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

namespace ScriptingEngine
{
    [LuaClass("LuaBuffer")]
    public class LuaBuffer : LuaInterface
    {
        public byte[] buffer;
	
	    /**
	     * Creates a buffer
	     * @param capacity
	     * @return
	     */
	    public static LuaBuffer Create(int capacity)
	    {
		    LuaBuffer lb = new LuaBuffer();
		    lb.buffer = new byte[capacity];
		    return lb;
	    }
	
	    /**
	     * Gets byte from index
	     * @param index
	     * @return
	     */
	    [LuaFunction(typeof(Int32))]
	    public Int32 GetByte(Int32 index)
	    {
		    return (int) buffer[index];
	    }
	
	    /**
	     * Set Byte at index
	     * @param index
	     * @param value
	     */
	    [LuaFunction(typeof(Int32), typeof(Int32))]
	    public void SetByte(Int32 index, Int32 value)
	    {
		    buffer[index] = (byte)value;
	    }
	
	    /**
	     * Frees LuaBuffer.
	     */
        [LuaFunction(false)]
	    public void Free()
	    {
		
	    }
	
	    /**
	     * (Ignore)
	     */
	    public byte[] GetBuffer() { return buffer; }

        #region LuaInterface Members

        public void RegisterEventFunction(string var, LuaTranslator lt)
        {
            throw new NotImplementedException();
        }

        public string GetId()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
