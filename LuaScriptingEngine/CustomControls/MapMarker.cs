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
using System.Device.Location;
#else
using Windows.UI.Xaml.Controls;
using Bing.Maps;
using Windows.Foundation;
using Windows.UI.Xaml;
#endif

namespace LuaScriptingEngine.CustomControls
{
    public class MapMarker
    {
        MapLayer markerLayer;
        Pushpin marker;
        Map mapSystem;
        bool draggable = false;
        String snippet = "";
        private string title;

        public MapMarker(Map mapSystem)
        {
            this.mapSystem = mapSystem;
            markerLayer = new MapLayer();
            marker = new Pushpin();
        }

        public void setDraggable(bool draggable)
        {
            this.draggable = draggable;
        }

        public void setPosition(Point point)
        {
#if WINDOWS_PHONE
            marker.Location = new GeoCoordinate(point.X, point.Y);
#elif NETFX_CORE
            MapLayer.SetPosition(marker, new Location(point.X, point.Y));
#endif
        }

        public void setSnippet(string value)
        {
            this.snippet = value;
        }

        public void setTitle(string value)
        {
            this.title = value;
        }

        public void setVisible(bool value)
        {
            if (value)
                marker.Visibility = Visibility.Visible;
            else
                marker.Visibility = Visibility.Collapsed;
        }
    }
}
