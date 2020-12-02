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
using Windows.UI.Xaml;
#endif

namespace ScriptingEngine.LuaUI
{
    [LuaClass("LGRadioButton")]
    public class LGRadioButton : LGButton
    {
        public LGRadioButton(LuaContext context)
            : base(context)
        {
        }

        public LGRadioButton(LuaContext context, String luaId)
            : base(context, luaId)
        {
        }

        public override void Setup()
        {
            view = new RadioButton();
        }

        public override void RegisterEventFunction(string var, LuaTranslator lt)
        {
            if (var == "CheckedChanged")
            {
                ((RadioButton)view).Checked += new RoutedEventHandler(delegate(object sender, RoutedEventArgs e)
                {
                    lt.CallIn(sender);
                });
            }
            else
                base.RegisterEventFunction(var, lt);
        }
    }
}
