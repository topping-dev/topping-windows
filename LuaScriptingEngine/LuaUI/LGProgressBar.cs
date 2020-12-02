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

namespace ScriptingEngine.LuaUI
{
    [LuaClass("LGProgressBar")]
    public class LGProgressBar : LGView
    {
        public LGProgressBar(LuaContext context)
            : base(context)
        {
        }

        public LGProgressBar(LuaContext context, String luaId)
            : base(context, luaId)
        {
        }

        public override void Setup()
        {
            view = new ProgressBar();
        }
    }
}
