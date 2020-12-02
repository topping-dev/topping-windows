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
    public class LuaNFC : LuaInterface
    {
        LuaTranslator tagRead;

        #region LuaInterface Members

        public void RegisterEventFunction(string var, LuaTranslator lt)
        {
            if (var == "TagRead")
            {
                tagRead = lt;
            }
        }

        public string GetId()
        {
            return "LuaNFC";
        }

        #endregion
    }
}
