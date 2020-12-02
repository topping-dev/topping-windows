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
using Windows.Foundation;
#endif

namespace ScriptingEngine
{
    [LuaClass("LuaPoint")]
    public class LuaPoint : LuaInterface
    {
        public double x;
        public double y;

        /**
	     * Creates LuaPoint
	     * @return LuaPoint
	     */
	    [LuaFunction(false)]
	    public static LuaPoint CreatePoint()
	    {
		    return new LuaPoint();
	    }

        /**
         * (Ignore)
         */
        public LuaPoint()
        {
        }
	
	    /**
	     * Creates LuaPoint with parameters
	     * @param x 
	     * @param y
	     * @return LuaPoint
	     */
	    [LuaFunction(typeof(float), typeof(float))]
	    public static LuaPoint CreatePointPar(float x, float y)
	    {
		    LuaPoint lp = new LuaPoint();
		    lp.Set(x, y);
		    return lp;
	    }
	
	    /**
	     * Sets the parameters of point
	     * @param x
	     * @param y
	     */
	    [LuaFunction(typeof(float), typeof(float))]
	    public void Set(float x, float y)
	    {
		    this.x = x;
            this.y = y;
	    }
	
	    /**
	     * Gets the x value
	     * @return x Integer
	     */
	    [LuaFunction(false)]
	    public float GetX() { return (float)x; }

        /**
         * (Ignore)
         */
        public Point ToPoint()
        {
            return new Point(x, y);
        }
	
	    /**
	     * Gets the y value
	     * @return y Integer
	     */
	    [LuaFunction(false)]
        public float GetY() { return (float)y; }

        #region LuaInterface Members

        public void RegisterEventFunction(string var, LuaTranslator lt)
        {
        }

        public string GetId()
        {
            return "LuaPoint";
        }

        #endregion
    }
}
