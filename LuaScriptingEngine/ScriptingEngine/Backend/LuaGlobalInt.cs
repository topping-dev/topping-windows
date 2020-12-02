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
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class LuaGlobalInt : Attribute
    {
        private String[] keys;
        private Int32[] vals;

        public LuaGlobalInt(String[] keys, Int32[] vals)
        {
            this.keys = keys;
            this.vals = vals;
        }

        public String[] GetKeys() { return keys; }
        public Int32[] GetVals() { return vals; }
    }
}
