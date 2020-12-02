using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
#if WINDOWS_PHONE
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls.Maps;
#elif NETFX_CORE
using Bing.Maps;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#endif

namespace ScriptingEngine
{
    /**
      * Class that is used to create polygons on map.
      */
    [LuaClass("LuaMapPolyline")]
    public class LuaMapPolyline : LuaInterface
    {
        private MapPolyline polygon;

        /**
         * (Ignore) 
         */
        public LuaMapPolyline(MapPolyline polygon)
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
            polygon.
#if WINDOWS_PHONE
            Fill = new SolidColorBrush(LuaScriptingEngine.CustomControls.ColorExtension.FromColorInt(color.GetColorValue()));
#elif NETFX_CORE
            Color = color.GetColorObject();
#endif
                
        }

        /**
         * Sets the stroke color
         * @param color
         */
        [LuaFunction(typeof(LuaColor))]
        public void SetStrokeColor(LuaColor color)
        {
            polygon.
#if WINDOWS_PHONE
            Stroke = new SolidColorBrush(LuaScriptingEngine.CustomControls.ColorExtension.FromColorInt(color.GetColorValue()));
#elif NETFX_CORE
            Color = color.GetColorObject();
#endif
        }

        /**
         * Sets the stroke width
         * @param value
         */
        [LuaFunction(typeof(float))]
        public void SetStrokeWidth(float value)
        {
            polygon.
#if WINDOWS_PHONE
                StrokeThickness
#elif NETFX_CORE
                Width
#endif
                = value;
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

        public void RegisterEventFunction(string var, LuaTranslator lt)
        {

        }

        public string GetId()
        {
            return "LuaMapPolyline";
        }

        #endregion
    }
}
