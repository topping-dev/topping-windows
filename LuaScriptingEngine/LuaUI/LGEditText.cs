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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#endif

namespace ScriptingEngine.LuaUI
{
    [LuaClass("LGEditText")]
    public class LGEditText : LGTextView
    {
        private Brush lastBrush = null;
        /**
	     * Creates LGEditText Object From Lua.
	     * @param lc
	     * @return LGEditText
	     */
	    [LuaFunction(typeof(LuaContext))]
	    public static LGEditText Create(LuaContext lc)
	    {
		    return new LGEditText(lc);
	    }

        public LGEditText(LuaContext context) 
            : base(context)
        {

        }

        public LGEditText(LuaContext context, String luaId)
            : base(context, luaId)
        {

        }

        public override void Setup()
        {
            view = new TextBox();
            ((TextBox)view).GotFocus += new RoutedEventHandler(LGEditText_GotFocus);
            ((TextBox)view).LostFocus += new RoutedEventHandler(LGEditText_LostFocus);
        }

        void LGEditText_LostFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)view).Foreground = lastBrush;
        }

        void LGEditText_GotFocus(object sender, RoutedEventArgs e)
        {
            lastBrush = ((TextBox)view).Foreground;
#if WINDOWS_PHONE
            ((TextBox)view).Foreground = (SolidColorBrush)Application.Current.Resources["PhoneTextBoxForegroundBrush"];
#else
            ((TextBox)view).Foreground = (SolidColorBrush)Application.Current.Resources["TextBoxForegroundThemeBrush"];
#endif
        }

        /**
         * Sets the text
         * @param val
         */
        public override void SetText(String val)
        {
            ((TextBox)view).Text = val;
        }

        /**
         * Gets the text
         * @return text
         */
        public override String GetText()
        {
            return ((TextBox)view).Text;
        }

        /**
         * Sets the text color
         * @param color
         */
        public override void SetTextColor(String color)
        {
            ((TextBox)view).Foreground = new SolidColorBrush(LGParser.Instance.ColorParser.ParseColor(color));
        } 

        public override void RegisterEventFunction(string var, LuaTranslator lt)
        {
            if (var == "TextChanged")
            {
                ((TextBox)view).TextChanged += new TextChangedEventHandler(delegate(object sender, TextChangedEventArgs e)
                {
                    lt.CallIn(sender);
                });
            }
            else if (var == "BeforeTextChanged")
            {
                ((TextBox)view).TextChanged += new TextChangedEventHandler(delegate(object sender, TextChangedEventArgs e)
                {
                    lt.CallIn(sender);
                });
            }
            else if (var == "AfterTextChanged")
            {
                ((TextBox)view).TextChanged += new TextChangedEventHandler(delegate(object sender, TextChangedEventArgs e)
                {
                    lt.CallIn(sender);
                });
            }
            else
                base.RegisterEventFunction(var, lt);
        }
    }
}
