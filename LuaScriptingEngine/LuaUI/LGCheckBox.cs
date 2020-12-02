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
    [LuaClass("LGCheckBox")]
    public class LGCheckBox : LGButton
    {

        /**
	     * Creates LGCheckbox Object From Lua.
	     * @param lc
	     * @return LGCheckBox
	     */
	    [LuaFunction(typeof(LuaContext))]
	    public static LGCheckBox Create(LuaContext lc)
	    {
		    return new LGCheckBox(lc);
	    }

        public LGCheckBox(LuaContext context)
            : base(context)
        {
        }

        public LGCheckBox(LuaContext context, String luaId)
            : base(context, luaId)
        {
        }

        public override void Setup()
        {
            view = new CheckBox();
        }

        public override void RegisterEventFunction(string var, LuaTranslator lt)
        {
            if (var == "CheckedChanged")
            {
                ((CheckBox)view).Checked += new RoutedEventHandler(delegate(object sender, RoutedEventArgs e)
                {
                    lt.CallIn(sender, ((CheckBox)view).IsChecked);
                });
                ((CheckBox)view).Unchecked += new RoutedEventHandler(delegate(object sender, RoutedEventArgs e)
                {
                    lt.CallIn(sender, ((CheckBox)view).IsChecked);
                });
            }
            else
                base.RegisterEventFunction(var, lt);
        }
    }
}
