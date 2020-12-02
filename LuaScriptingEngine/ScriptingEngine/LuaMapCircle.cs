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
using Microsoft.Phone.Controls.Maps;
#else
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
#endif

namespace ScriptingEngine
{
    /**
     * Class that is used to create circles on map.
     */
    [LuaClass("LuaMapCircle")]
    public class LuaMapCircle : LuaInterface
    {
        MapCircle circle;
	
	    /**
	     * (Ignore) 
	     */	
	    public LuaMapCircle(MapCircle circle)
	    {
		    this.circle = circle;
	    }
	
	    /**
	     * Set circle center
	     * @param center
	     */
	    [LuaFunction(typeof(LuaPoint))]
	    public void SetCenter(LuaPoint center)
	    {
		    circle.setCenter(new Point(center.x, center.y));
	    }
	
	    /**
	     * Set circle center
	     * @param x
	     * @param y
	     */
	    [LuaFunction(typeof(Double), typeof(Double))]
	    public void SetCenterEx(double x, double y)
	    {
		    circle.setCenter(new Point(x, y));
	    }
	
	    /**
	     * Set circle radius
	     * @param radius
	     */
	    [LuaFunction(typeof(Double))]
	    public void SetRadius(double radius)
	    {
		    circle.setRadius(radius);
	    }
	
	    /**
	     * Set circle stroke color
	     * @param color
	     */
	    [LuaFunction(typeof(LuaColor))]
	    public void SetStrokeColor(LuaColor color)
	    {
		    circle.setStrokeColor(color.GetColorValue());
	    }
	
	    /**
	     * Set circle stroke color with integer
	     * @param color
	     */
	    [LuaFunction(typeof(int))]
	    public void SetStrokeColorEx(int color)
	    {
		    circle.setStrokeColor(color);
	    }
	
	    /**
	     * Set circle stroke width
	     * @param width
	     */
	    [LuaFunction(typeof(Double))]
	    public void SetStrokeWidth(double width)
	    {
		    circle.setStrokeWidth((float)width);
	    }
	
	    /**
	     * Set circle fill color
	     * @param color
	     */
	    [LuaFunction(typeof(LuaColor))]
	    public void SetFillColor(LuaColor color)
	    {
		    circle.setFillColor(color.GetColorValue());
	    }
	
	    /**
	     * Set circle fill color with integer
	     * @param color
	     */
	    [LuaFunction(typeof(int))]
	    public void SetFillColorEx(int color)
	    {
		    circle.setFillColor(color);
	    }
	
	    /**
	     * Set z-index of circle
	     * @param index
	     */
	    [LuaFunction(typeof(Double))]
	    public void SetZIndex(double index)
	    {
		    circle.setZIndex((float) index);
	    }

        #region LuaInterface Members

        /**
         * (Ignore)
         */
        public void RegisterEventFunction(string var, LuaTranslator lt)
        {
        }

        /**
         * (Ignore)
         */
        public string GetId()
        {
            return "LuaMapCircle";
        }

        #endregion
    }
}
