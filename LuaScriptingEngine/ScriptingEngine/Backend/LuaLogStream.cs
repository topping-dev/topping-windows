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
using System.IO;
using LoggerNamespace;
using LuaCSharp;

namespace ScriptingEngine
{
    public class LuaLogStream : MemoryStream
    {
        private int type = 0; //0 debug, 1 error

        public LuaLogStream() : base()
        {
        }

        public LuaLogStream(int type)
            : base()
        {
            this.type = type;
        }

        public override void WriteByte(byte value)
        {
            if (type == 0)
                Log.i("LuaLogStream", ((char)value).ToString());
            else
                Log.e("LuaLogStream", ((char)value).ToString());
        }

        public void Write(byte[] array)
        {
            Write(array, 0, array.Length);
        }

        public override void Write(byte[] array, int offset, int count)
        {
            try
            {
                Lua.CharPtr ptr = new Lua.CharPtr();
                ptr.setByteArray(array);
                if (type == 0)
                    Log.i("LuaLogStream", ptr.toString());
                else
                    Log.e("LuaLogStream", ptr.toString());
            }
            catch (Exception e)
            {
                Log.d("LuaLogStream.cs", e.Message);
            }
        }
    }
}
