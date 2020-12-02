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
    /**
    * Class that is used to create images on map.
    */
    [LuaClass("LuaMapImage")]
    public class LuaMapImage : LuaInterface
    {
        MapImage image;

        /**
	     * (Ignore)
	     */
	    public LuaMapImage(MapImage image)
	    {
		    this.image = image;
	    }
	
	    /**
	     * Sets the bearing of image
	     * @param bearing
	     */
	    [LuaFunction(typeof(float))]
	    public void SetBearing(float bearing)
	    {
		    image.setBearing(bearing);
	    }
	
	    /**
	     * Sets the dimension of image, height automatically calculated
	     * @param width
	     */
	    [LuaFunction(typeof(float))]
	    public void SetDimensions(float width)
	    {
		    image.setDimensions(width);
	    }
	
	    /**
	     * Sets the dimesion of image
	     * @param width
	     * @param height
	     */
	    [LuaFunction(typeof(float), typeof(float))]
	    public void SetDimensionsEx(float width, float height)
	    {
		    image.setDimensions(width, height);
	    }
	
	    /**
	     * Sets the position of image
	     * @param point
	     */
	    [LuaFunction(typeof(LuaPoint))]
	    public void SetPosition(LuaPoint point)
	    {
		    image.setPosition(new Point(point.x, point.y));
	    }
	
	    /**
	     * Sets the position of the image
	     * @param x
	     * @param y
	     */
	    [LuaFunction(typeof(float), typeof(float))]
	    public void SetPositionEx(float x, float y)
	    {
		    image.setPosition(new Point(x, y));
	    }
	
	    /*public void SetPositionFromBound(LuaPoint point)
	    { 
	    }*/
	
	    /**
	     * Sets the transparency of the image
	     * @param transperency
	     */
	    [LuaFunction(typeof(float))]
	    public void SetTransparency(float transperency)
	    {
		    image.setTransparency(transperency);
	    }
	
	    /**
	     * Sets the visibility of the image
	     * @param value
	     */
	    [LuaFunction(typeof(Boolean))]
	    public void SetVisible(bool value)
	    {
		    image.setVisible(value);
	    }
	
	    /**
	     * Sets the z-index of the image
	     * @param index
	     */
	    [LuaFunction(typeof(float))]
	    public void SetZIndex(float index)
	    {
		    image.setZIndex(index);
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
            return "LuaMapImage";
        }

        #endregion
    }
}
