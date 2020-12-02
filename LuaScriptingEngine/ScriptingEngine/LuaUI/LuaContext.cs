using System;
using System.Net;
using System.Windows;
using ScriptingEngine;
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

namespace ScriptingEngine.LuaUI
{
    [LuaClass("LuaContext")]
    public class LuaContext : LuaInterface
    {
        #region LuaInterface Members

        public void RegisterEventFunction(string var, LuaTranslator lt)
        {
        }

        public string GetId()
        {
            return "LuaContext";
        }

        #endregion

        public static LuaContext CreateLuaContext(LuaForm form)
        {
            return new LuaContext();
        }
    }
}
