using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Xna.Framework.Media;
using System.Device.Location;
#else
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Bing.Maps;
#endif
#if WINDOWS_PHONE
using Microsoft.Phone.Controls.Maps;
#endif

namespace LuaScriptingEngine.CustomControls
{
    public class MapImage
    {
        CompositeTransform ObjComposite;
        BitmapImage bi;
        Image image;
        MapLayer imageLayer;
        private Point ImagePosition;
        private double X;
        private double Y;

        public MapImage(BitmapImage img)
        {
            bi = img;
            image = new Image();
            image.Source = bi;
            imageLayer = new MapLayer();
            ObjComposite = new CompositeTransform();
        }

        public void setBearing(float bearing)
        {
            double radian = (bearing / 180.0) * Math.PI;

            double bx = bi.PixelWidth * Math.Cos(radian) + bi.PixelHeight * Math.Sin(radian);
            double by = bi.PixelWidth * Math.Sin(radian) + bi.PixelHeight * Math.Cos(radian);
            var translationDelta = new Point((image.ActualWidth - bx) / 2.0, (image.ActualHeight - by) / 2.0);
            UpdateImagePosition(translationDelta);
            ApplyPosition();
            image.RenderTransform = ObjComposite;
        }

        // This code update the ImageControl position on the canvas
        public void UpdateImagePosition(Point delta)
        {
            var newPosition = new Point(ImagePosition.X + delta.X, ImagePosition.Y + delta.Y);
            ImagePosition = newPosition;            
        }

        //This apply the new position to make the ImageControl rotate from center
        public void ApplyPosition()
        {
            ObjComposite.TranslateX = ImagePosition.X;
            ObjComposite.TranslateY = ImagePosition.Y;
        }

        public void setDimensions(float width)
        {
#if !NETFX_CORE
            WriteableBitmap wb = new WriteableBitmap(bi);
            MemoryStream ms = new MemoryStream();
            int orgHeight = bi.PixelHeight;
            int orgWidth = bi.PixelWidth;
            float ratio = width / orgWidth;
            int height = (int)ratio * orgWidth;
            wb.SaveJpeg(ms, (int)width, height, 0, 80);
            bi = new BitmapImage();
            bi.SetSource(ms);
            image.Source = bi;
#else
            
#endif
            
        }

        public void setDimensions(float width, float height)
        {
#if !NETFX_CORE
            WriteableBitmap wb = new WriteableBitmap(bi);
            MemoryStream ms = new MemoryStream();
            wb.SaveJpeg(ms, (int)width, (int)height, 0, 80);
            bi = new BitmapImage();
            bi.SetSource(ms);
            image.Source = bi;
#endif
        }

        public void setPosition(Point point)
        {
            imageLayer.Children.Remove(image);
            imageLayer.AddChild(image, new GeoCoordinate(point.X, point.Y), PositionOrigin.Center);
            X = point.X;
            Y = point.Y;
        }

        public void setTransparency(float transperency)
        {
            
        }

        public void setVisible(bool value)
        {
            if (value)
                imageLayer.AddChild(image, new GeoCoordinate(X, Y), PositionOrigin.Center);
            else
                imageLayer.Children.Remove(image);
        }

        public void setZIndex(float index)
        {
            Canvas.SetZIndex(imageLayer, (int)index);
        }
    }
}
