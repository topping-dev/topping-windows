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
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class LuaGlobalManual : Attribute
    {
        String name;

        public LuaGlobalManual(String name)
        {
            this.name = name;
        }

        public String GetName() { return name; }
    }
}
