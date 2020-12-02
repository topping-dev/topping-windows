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
using LuaCSharp;

namespace ScriptingEngine
{
    public interface LuaGlobalFunction
    {
        int __tostring(Lua.lua_State L);
        int __gc(Lua.lua_State L);
        int __index(Lua.lua_State L);
        int __newindex(Lua.lua_State L);
    }
}
