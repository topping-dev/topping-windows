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
using Windows.UI;
#endif
using ScriptingEngine.LuaUI;
using LuaScriptingEngine.CustomControls;

namespace ScriptingEngine
{
    [LuaClass("LuaColor")]
    [LuaGlobalUInt(
        new String[] { 
            "LuaColor.BLACK",
			"LuaColor.BLUE",
			"LuaColor.CYAN",
			"LuaColor.DKGRAY",
			"LuaColor.GRAY",
			"LuaColor.GREEN",
			"LuaColor.LTGRAY",
			"LuaColor.MAGENTA",
			"LuaColor.RED",
			"LuaColor.TRANSPARENT",
			"LuaColor.WHITE",
			"LuaColor.YELLOW"
        },
        new UInt32[] {
            0xff000000,
			0xff0000ff,
			0xff00ffff,
			0xff444444,
			0xff888888,
			0xff00ff00,
			0xffcccccc,
			0xffff00ff,
			0xffff0000,
			0x00000000,
			0xffffffff,
			0xffffff00
		})
    ]
    public class LuaColor : LuaInterface
    {
	    private int colorValue;
        private Color colorObject;

	    /**
	     * Returns LuaColor from string value.
	     * Example #ffffffff or #ffffff
	     * @param colorStr
	     * @return
	     */
	    [LuaFunction(typeof(String))]
	    public static LuaColor FromString(String colorStr)
	    {
		    LuaColor color = new LuaColor();
            color.colorObject = LGColorParser.Instance.ParseColor(colorStr);
            color.colorValue = color.colorObject.ToColorInt();
		    return color;
	    }
	
	    /**
	     * Returns LuaColor from argb.
	     * @param alpha
	     * @param red
	     * @param green
	     * @param blue
	     * @return
	     */
	    [LuaFunction(typeof(int), typeof(int), typeof(int), typeof(int))]
	    public static LuaColor CreateFromARGB(int alpha, int red, int green, int blue)
	    {
		    LuaColor color = new LuaColor();
		    color.colorObject = Color.FromArgb((byte)alpha, (byte)red, (byte)green, (byte)blue);
            color.colorValue = color.colorObject.ToColorInt();
		    return color;
	    }
	
	    /**
	     * Returns LuaColor from rgb.
	     * @param red
	     * @param green
	     * @param blue
	     * @return
	     */
	    [LuaFunction(typeof(int), typeof(int), typeof(int))]
	    public static LuaColor CreateFromRGB(int red, int green, int blue)
	    {
		    LuaColor color = new LuaColor();
		    color.colorObject = Color.FromArgb(255, (byte)red, (byte)green, (byte)blue);
            color.colorValue = color.colorObject.ToColorInt();
		    return color;
	    }
	
	    /**
	     * Returns the integer color value
	     * @return
	     */
	    [LuaFunction(false)]
	    public int GetColorValue()
	    {
		    return colorValue;
	    }
	
	    /**
	     * Frees LuaColor.
	     */
	    [LuaFunction(false)]
	    public void Free()
	    {
	    }

        /**
         * (Ignore)
         */
        public Color GetColorObject()
        {
            return colorObject;
        }

        #region LuaInterface Members

        public void RegisterEventFunction(string var, LuaTranslator lt)
        {
        }

        public string GetId()
        {
            return "LuaColor";
        }

        #endregion
    }
}
