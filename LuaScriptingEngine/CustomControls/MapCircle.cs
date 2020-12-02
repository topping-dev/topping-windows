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
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Bing.Maps;
#endif

namespace LuaScriptingEngine.CustomControls
{
    public class MapCircle
    {
        MapLayer circleLayer;
        Ellipse circle;

        public MapCircle()
        {
            circle = new Ellipse();
            circleLayer = new MapLayer();
        }

        public void setCenter(Point latLng)
        {
            circleLayer.Children.Remove(circle);
            circleLayer.AddChild(circle, new GeoCoordinate(latLng.X, latLng.Y), PositionOrigin.Center);
        }

        public void setRadius(double radius)
        {
            circle.Width = radius;
            circle.Height = radius;
        }

        public void setStrokeColor(int p)
        {
            circle.Stroke = new SolidColorBrush(ColorExtension.FromColorInt(p));
        }

        public void setStrokeColor(Color p)
        {
            circle.Stroke = new SolidColorBrush(p);
        }

        public void setStrokeWidth(float p)
        {
            circle.StrokeThickness = p;
        }

        public void setFillColor(int p)
        {
            circle.Fill = new SolidColorBrush(ColorExtension.FromColorInt(p));
        }

        public void setFillColor(Color p)
        {
            circle.Fill = new SolidColorBrush(p);
        }

        public void setZIndex(float p)
        {
            Canvas.SetZIndex(circleLayer, (int)p);
        }
    }
}
