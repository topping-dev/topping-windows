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
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
#endif

namespace LuaScriptingEngine.CustomControls
{
    public static class StringExtension
    {
        // FontSize: 0 will not apply size calculation
        // FontFamily: null will not apply size calculation
        // FontWeight is not nullable, default use FontWeights.Normal
        public static Size Measure (this string strText, double dbFontSize, FontFamily fontFamily, FontWeight fontWeight)
        {
            var tb = new TextBlock();
            if (dbFontSize> 0)
                tb.FontSize = dbFontSize;

            if (fontFamily != null)
                tb.FontFamily = fontFamily;

            tb.FontWeight = fontWeight;
            tb.Text = strText;

            return new Size (tb.ActualWidth, tb.ActualHeight);
        } 
    }
}
