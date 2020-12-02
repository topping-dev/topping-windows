using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using ScriptingEngine.LuaUI;
using LuaScriptingEngine.CustomControls;
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
using Windows.UI.Xaml;
using Windows.UI;
#endif

namespace ScriptingEngine.LuaUI
{
    [LuaClass("LGTextView")]
    public class LGTextView : LGView
    {
        private Border border;
        private TextBlock block;

        public LGTextView(LuaContext context) 
            : base(context)
        {

        }

        public LGTextView(LuaContext context, String luaId)
            : base(context, luaId)
        {

        }

        public override void Setup()
        {
            border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.Black);
            border.BorderThickness = new Thickness(0);
            block = new TextBlock();
            border.Child = block;
            view = border;

            block.TextWrapping = TextWrapping.Wrap;
        }

        /**
         * Sets the text
         * @param val
         */
        [LuaFunction(typeof(String))]
        public virtual void SetText(String val)
        {
            block.Text = val;
            /*Size sz = val.Measure(block.FontSize, block.FontFamily, block.FontWeight);
            double lines = sz.Width / block.ActualWidth;
            block.Height = block.ActualHeight * Math.Ceiling(lines);*/

        }
    
        /**
         * Gets the text
         * @return text
         */
        [LuaFunction(false)]
        public virtual String GetText()
        {
            return block.Text;
        }
    
        /**
         * Sets the text color
         * @param color
         */
        [LuaFunction(typeof(String))]
        public virtual void SetTextColor(String color)
	    {
            block.Foreground = new SolidColorBrush(LGParser.Instance.ColorParser.ParseColor(color));
	    }
    }
}
