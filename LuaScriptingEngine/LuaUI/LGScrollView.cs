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
    [LuaClass("LGScrollView")]
    public class LGScrollView : LGView
    {
        public LGScrollView(LuaContext context) 
            : base(context)
        {

        }

        public LGScrollView(LuaContext context, String luaId)
            : base(context, luaId)
        {

        }

        public override void Setup()
        {
            view = new ScrollViewer();
        }

        public override void Populate(LGView parent)
        {
            if (parent != null)
                base.Populate(parent);
            foreach (LGView w in subviews)
            {
                w.Populate(this);
            }
        }
    }
}
