using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Bing.Maps
{
    public enum PositionOrigin
    {
        Center
    }

    public struct GeoCoordinate
    {
        public double lat;
        public double lng;
        public GeoCoordinate(double lat, double lng)
        {
            this.lat = lat;
            this.lng = lng;
        }
    }

    public static class MapLayerExtension
    {
        public static void AddChild(this MapLayer layer, UIElement img, GeoCoordinate pos, PositionOrigin origin)
        {
            MapLayer.SetPosition(img, new Location(pos.lat, pos.lng));
            layer.Children.Add(img);
        }
    }
}
