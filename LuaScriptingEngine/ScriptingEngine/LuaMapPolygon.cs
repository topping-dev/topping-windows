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
using Microsoft.Phone.Controls.Maps;
#else
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using Bing.Maps;
#endif

namespace ScriptingEngine
{
    /**
     * Class that is used to create polygons on map.
     */
    [LuaClass("LuaMapPolygon")]
    public class LuaMapPolygon : LuaInterface
    {
        private MapPolygon polygon;

	    /**
	     * (Ignore) 
	     */	
	    public LuaMapPolygon(MapPolygon polygon)
	    {
		    this.polygon = polygon;
	    }
	
	    /**
	     * Sets the fill color
	     * @param color
	     */
	    [LuaFunction(typeof(LuaColor))]
	    public void SetFillColor(LuaColor color)
	    {
#if WINDOWS_PHONE
		    polygon.Fill = new SolidColorBrush(LuaScriptingEngine.CustomControls.ColorExtension.FromColorInt(color.GetColorValue()));
#elif NETFX_CORE
            polygon.FillColor = color.GetColorObject();
#endif
	    }
	
	    /**
	     * Sets the stroke color
	     * @param color
	     */
	    [LuaFunction(typeof(LuaColor))]
	    public void SetStrokeColor(LuaColor color)
	    {
#if WINDOWS_PHONE
		    polygon.Stroke = new SolidColorBrush(LuaScriptingEngine.CustomControls.ColorExtension.FromColorInt(color.GetColorValue()));
#endif
	    }
	
	    /**
	     * Sets the stroke width
	     * @param value
	     */
	    [LuaFunction(typeof(float))]
	    public void SetStrokeWidth(float value)
	    {
#if WINDOWS_PHONE
		    polygon.StrokeThickness = value;
#endif
	    }
	
	    /**
	     * Sets the visibility
	     * @param value
	     */
	    [LuaFunction(typeof(bool))]
	    public void SetVisible(bool value)
	    {
#if WINDOWS_PHONE
            if (value)
                polygon.Visibility = Visibility.Visible;
            else
                polygon.Visibility = Visibility.Collapsed;
#elif NETFX_CORE
            polygon.Visible = value;
#endif
	    }
	
	    /**
	     * Sets the z-index
	     * @param value
	     */
	    [LuaFunction(typeof(float))]
	    public void SetZIndex(float value)
	    {
#if WINDOWS_PHONE
            Canvas.SetZIndex(polygon, (int)value);
#endif
	    }
    
        #region LuaInterface Members

        public void  RegisterEventFunction(string var, LuaTranslator lt)
        {
 	        
        }

        public string  GetId()
        {
 	        return "LuaMapPolygon";
        }

        #endregion
    }
}
