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
#else
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
#endif

namespace ScriptingEngine
{
    [LuaClass("LuaMapMarker")]
    public class LuaMapMarker : LuaInterface
    {
        MapMarker marker;

        /**
	     * (Ignore) 
	     */	
	    public LuaMapMarker(MapMarker marker)
	    {
		    this.marker = marker;
	    }
	
	    /**
	     * Set marker draggable
	     * @param draggable
	     */
	    [LuaFunction(typeof(bool))]
	    public void SetDraggable(bool draggable)
	    {
		    marker.setDraggable(draggable);
	    }
	
	    /**
	     * Set marker position 
	     * @param point
	     */	
	    [LuaFunction(typeof(LuaPoint))]
	    public void SetPosition(LuaPoint point)
	    {
		    marker.setPosition(new Point(point.x, point.y));
	    }
	
	    /**
	     * Set marker position
	     * @param x
	     * @param y
	     */	
	    [LuaFunction(typeof(Double), typeof(Double))]
	    public void SetPositionEx(double x, double y)
	    {
		    marker.setPosition(new Point(x, y));
	    }
	
	    /**
	     * Set marker snippet 
	     * @param value
	     */	
	    [LuaFunction(typeof(String))]
	    public void SetSnippet(String value)
	    {
		    marker.setSnippet(value);
	    }
	
	    /**
	     * Set marker title
	     * @param value 
	     */	
	    [LuaFunction(typeof(String))]
	    public void SetTitle(String value)
	    {
		    marker.setTitle(value);
	    }
	
	    /**
	     * Set marker visibility 
	     * @param value
	     */	
	    [LuaFunction(typeof(bool))]
	    public void SetVisible(bool value)
	    {
		    marker.setVisible(value);
	    }

        #region LuaInterface Members

        public void RegisterEventFunction(string var, LuaTranslator lt)
        {
        }

        public string GetId()
        {
            return "LuaMapMarker";
        }

        #endregion
    }
}
