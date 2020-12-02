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
using Windows.UI.Xaml.Media;
#endif

namespace ScriptingEngine.LuaUI
{
    [LuaClass("LGButton")]
    public class LGButton : LGTextView
    {
        public LGButton(LuaContext context)
            : base(context)
        {
        }

        public LGButton(LuaContext context, String luaId)
            : base(context, luaId)
        {
        }

        public override void Setup()
        {
            view = new Button();
        }

        /**
         * Sets the text
         * @param val
         */
        public override void SetText(String val)
        {
            ((Button)view).Content = val;
        }

        /**
         * Gets the text
         * @return text
         */
        public override String GetText()
        {
            return (String)((Button)view).Content;
        }

        /**
         * Sets the text color
         * @param color
         */
        public override void SetTextColor(String color)
        {
            ((Button)view).Foreground = new SolidColorBrush(LGParser.Instance.ColorParser.ParseColor(color));
        } 
    }
}
