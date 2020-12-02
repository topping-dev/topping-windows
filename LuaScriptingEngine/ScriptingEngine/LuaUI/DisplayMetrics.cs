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
    public class DisplayMetrics
    {
        public const int FILL_PARENT = -1;
        public const int WRAP_CONTENT = -2;

        public static float density=2.0f;
	    public static float scaledDensity=2.0f;
	    public static float xdpi=160;
	    public static float ydpi=160;

	    public const float MM_TO_IN = 0.0393700787f;
	    public const float PT_TO_IN = 1/72.0f;

	    public static int readSize(String sz)
	    {
		    if (sz == null)
                return FILL_PARENT;
		    if ("wrap_content" == sz) {
                return WRAP_CONTENT;
		    }
		    else if ("fill_parent" == sz) {
                return FILL_PARENT;
		    }
		    try {
			    float size;
			    if (sz.EndsWith("dip"))
				    size = Convert.ToSingle(sz.Substring(0, sz.Length-3));
			    else
                    size = Convert.ToSingle(sz.Substring(0, sz.Length - 2));
			
			    if (sz.EndsWith("px")) {
				    return (int)size;
			    }
			    else if (sz.EndsWith("in")) {
				    return (int)(size*xdpi);
			    }
			    else if (sz.EndsWith("mm")) {
				    return (int)(size*MM_TO_IN*xdpi);
			    }
			    else if (sz.EndsWith("pt")) {
				    return (int)(size*PT_TO_IN*xdpi);
			    }
			    else if (sz.EndsWith("dp") || sz.EndsWith("dip")) {
				    return (int)(size*density);
			    }
			    else if (sz.EndsWith("sp")) {
				    return (int)(size*scaledDensity);
			    }
			    else {
				    return Convert.ToInt32(sz);
			    }
		    } catch (Exception ex) {
			    return -1;
		    }
	    }
    }
}
