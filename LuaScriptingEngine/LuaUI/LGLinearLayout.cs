using System;
using System.Net;
using System.Windows;
using LuaScriptingEngine.CustomControls;
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
    [LuaClass("LGLinearLayout")]
    public class LGLinearLayout : LGView
    {
        public LGLinearLayout() : base()
        {
        }

        public LGLinearLayout(LuaContext context)
            : base(context)
        {
        }

        public LGLinearLayout(LuaContext context, String luaId)
            : base(context, luaId)
        {
        }

        public override void Setup()
        {
            view = new LinearLayout();
        }

        public override void AfterSetup()
        {
            base.AfterSetup();
        }

        public override void Populate(LGView parent)
        {
            if(parent != null)
                base.Populate(parent);
            foreach (LGView w in subviews)
            {
                w.Populate(this);
            }
        }
    }
}
