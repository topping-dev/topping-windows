using System;
using System.Net;
using System.Windows;
using ScriptingEngine;
using LoggerNamespace;
using System.Text;
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
    public class Tools
    {
        public static void LogException(String tag, Exception e)
        {
            Lua.lua_Debug ld = new Lua.lua_Debug();
            try
            {
                Lua.lua_getinfo(LuaEngine.Instance.GetLuaState(), "S", ld);
            }
            catch (Exception ex)
            {
                ld.source = "";
                Log.e("Tools.java", "Cannot parse lua debug");
            }
            StringBuilder exStr = new StringBuilder();
            if (e.Message == null)
                exStr.Append(e.ToString());
            else
                exStr.Append(e.Message);
            exStr.AppendLine();
            exStr.AppendLine(e.StackTrace);
            exStr.Append("Lua: ").Append(ld.source).Append(" ").AppendLine(ld.currentline.ToString());
            Log.e(tag, exStr.ToString());
        }
    }
}
