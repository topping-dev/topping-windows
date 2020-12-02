using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using LuaCSharp;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
#else
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;
using WinRTXamlToolkit.Imaging;
using Windows.Storage;
using Windows.ApplicationModel;
using LuaScriptingEngine;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
#endif

namespace ScriptingEngine.LuaUI
{
    public class LGDrawableReturn
    {
        public ImageSource img;
        public Color? color;
        public String state;        
    }

    public class LGDrawableParser : Singleton<LGDrawableParser>, LuaInterface
    {
        private List<DynamicResource> directoryList;
        public void Initialize(Lua.lua_State L)
        {
#if WINDOWS_PHONE
            String drawableDirectoriesString = "";
            Lua.lua_getglobal(L, "WP7DrawableDirectories");
            if (Lua.lua_isstring(L, -1) != 0)
                drawableDirectoriesString = Lua.lua_tostring(L, -1).toString();
            Lua.lua_pop(L, 1);

            String[] drawableDirectories = drawableDirectoriesString.Split(new char[] { ',' });
#else
            String[] drawableDirectories = LuaResource.GetResourceDirectories(LuaEngine.LUA_DRAWABLE_FOLDER);
#endif
            directoryList = new List<DynamicResource>();
            LGParser.Instance.Tester(drawableDirectories, LuaEngine.LUA_DRAWABLE_FOLDER, ref directoryList);
            directoryList.Sort(new Comparison<DynamicResource>((a, b) => 
            {
                String aData = (String)a.data;
                String bData = (String)b.data;
                if(aData == bData)
                    return 0;
                else if(aData.Length > bData.Length)
                    return -1;
                else
                    return 1;
            }));
        }

        public LGDrawableReturn ParseDrawable(String drawable)
        {
            String[] arr = drawable.Split('/');
            ImageSource retVal = null;
            if (arr[0].Contains("drawable"))
            {
                if (arr.Length > 1)
                {
                    String name = arr[1];
                    foreach(DynamicResource dr in directoryList)
                    {
                        String path = System.IO.Path.Combine(LuaEngine.LUA_RESOURCE_FOLDER, LuaEngine.Instance.GetUIRoot(), (String)dr.data);
                        Stream str = (Stream)LuaResource.GetResource(path, name + ".png").GetStream();
                        if (str == null)
                        {
                            str = (Stream)LuaResource.GetResource(path, name + ".jpg").GetStream();
                            if (str == null)
                            {
                                str = (Stream)LuaResource.GetResource(path, name + ".gif").GetStream();
                                if (retVal == null)
                                {
                                    retVal = ParseXML(name).img;
                                }
                            }
                        }
                        if (str != null)
                        {
                            retVal = new BitmapImage();
                            ((BitmapImage)retVal).SetSource(str
#if NETFX_CORE
                            .AsRandomAccessStream()
#endif
                            );
                            break;
                        }
                    }                    
                }
            }
            LGDrawableReturn ldr = new LGDrawableReturn();
            ldr.img = retVal;
            return ldr;
        }

        LGDrawableReturn ParseXML(String filename)
        {
            LuaStream dat = LuaResource.GetResource(LuaEngine.LUA_DRAWABLE_FOLDER, filename + ".xml");
            XDocument parse;
		    try {
                parse = XDocument.Load((Stream)dat.GetStream());
                String name = parse.Root.Name.LocalName;

                XElement root = parse.Root;
                 if(name == "bitmap"
                   || name == "nine-patch")
                {
                    return ParseBitmap(root);
                }
                else if(name == "layer-list")
                {
                    return ParseLayer(root);
                }
                else if(name == "selector")
                {
                    return ParseStateList(root);
                }
                else if(name == "shape")
                {
                    return ParseShape(root);
                }
            }
            catch(Exception ex)
            {
                LGDrawableReturn ret = new LGDrawableReturn();
                return ret;
            }
   
            return null;
        }

        LGDrawableReturn ParseBitmap(XElement root)
        {   
            String imgPath = "";
            String tileMode = null;
   
            //[UIColor colorWithPatternImage: [UIImage imageNamed:@"gingham.png"]];
            foreach(XAttribute node in root.Attributes())
            {
                if(node.Name.LocalName == "src")
                {
                    imgPath = node.Value;
                }
                if(node.Name.LocalName == "tileMode")
                {
                    tileMode = node.Value;
                }
            }
   
            Color? color = null;
            ImageSource img = null;
            if(tileMode != null)
            {
                if(tileMode == "repeat")
                    //color = [UIColor colorWithPatternImage: [UIImage imageNamed:imgPath]];
                    color = Colors.Black; //TODO:Fix
                else
                    img = LGDrawableParser.Instance.ParseDrawable(imgPath).img;
            }
            else
                img = LGDrawableParser.Instance.ParseDrawable(imgPath).img;
   
            if(img != null || color != null)
            {
                LGDrawableReturn ldr = new LGDrawableReturn();
                ldr.img = img;
                ldr.color = color;
                return ldr;
            }
            return null;
        }

        LGDrawableReturn ParseLayer(XElement root)
        {
            Canvas canvas = new Canvas();
   
            int totalWidth = 0;
            int totalHeight = 0;
            foreach(XElement child in root.Elements())
            {
                int left = 0, top = 0, right = 0, bottom = 0;
                LGDrawableReturn ldr = null;
                String name = child.Name.LocalName;
                if(name == "item")
                {
                    int childCount = 0;
                    XElement firstChild = null;
                    foreach(XElement x in child.Elements())
                    {
                        if(childCount == 0)
                            firstChild = x;
                        childCount++;
                    }
                    if(childCount > 0)
                    {
                        ldr = ParseBitmap(firstChild);
                    }
                    foreach(XAttribute node in child.Attributes())
                    {
                        String attr = node.Name.LocalName;
                        if(attr == "drawable")
                        {
                            ldr = ParseDrawable(node.Value);
                        }
                        else if(attr == "id")
                        {
                        }
                        else if(attr == "top")
                        {
                            top = LGDimensionParser.Instance.GetDimension(node.Value);
                        }
                        else if(attr == "right")
                        {
                            right = LGDimensionParser.Instance.GetDimension(node.Value);
                        }
                        else if(attr == "bottom")
                        {
                            bottom = LGDimensionParser.Instance.GetDimension(node.Value);
                        }
                        else if(attr == "left")
                        {
                            left = LGDimensionParser.Instance.GetDimension(node.Value);
                        }
                    }

                    if (ldr == null)
                        continue;
                    BitmapSource bs = (BitmapSource)ldr.img;
                    Image i = new Image();
                    i.Source = ldr.img;
                    Canvas.SetLeft(i, left - right);
                    Canvas.SetTop(i, top - bottom);
                    canvas.Children.Add(i);
                    int height = (top - bottom) + bs.PixelHeight;
                    int width = (left - right) + bs.PixelWidth;
                    
                    if(width > totalWidth)
                        totalWidth = width;
                    if(height > totalHeight)
                        totalHeight = height;
                }
            }

            canvas.Width = totalWidth;
            canvas.Height = totalHeight;

#if !NETFX_CORE
            WriteableBitmap wb = BitmapFactory.New(totalWidth, totalHeight);
            wb.Render(canvas, null);
            wb.Invalidate();
            BitmapImage img;
            using (MemoryStream ms = new MemoryStream())
            {
                wb.SaveJpeg(ms, totalWidth, totalHeight, 0, 100);
                img = new BitmapImage();
                img.SetSource(ms);
            }
            LGDrawableReturn ret = new LGDrawableReturn();
            ret.img = img;
            return ret;
#else
            LGDrawableReturn ret = new LGDrawableReturn();
            RenderTargetBitmap rtb = new RenderTargetBitmap();
            rtb.RenderAsync(canvas, totalWidth, totalHeight).Completed = (info, status) =>
            {
                ret.img = rtb;
            };
            return ret;
#endif
        }

        public enum StateType
        {
            StateSelected,
            StateHighlighted,
            StateCheckable,
            StateChecked,
            StateNormal,
            StateActivated,
            StateWindowFocused
        }

        Dictionary<String, Dictionary<StateType, ImageSource>> stateListDictionary = new Dictionary<String, Dictionary<StateType, ImageSource>>();

        LGDrawableReturn ParseStateList(XElement root)
        {
            Dictionary<StateType, ImageSource> stateList = new Dictionary<StateType, ImageSource>();
            foreach(XElement child in root.Elements())
            {
                LGDrawableReturn ldr = null;
                String name = child.Name.LocalName;
                if(name == "item")
                {
                    int childCount = 0;
                    XElement firstChild = null;
                    foreach(XElement x in child.Elements())
                    {
                        if(childCount == 0)
                            firstChild = x;
                        childCount++;
                    }
                    if(childCount > 0)
                    {
                        ldr = ParseBitmap(firstChild);
                    }
                    
                    foreach(XAttribute node in child.Attributes())
                    {
                        String attr = node.Name.LocalName;
                        if(attr == "drawable")
                        {
                            ldr = ParseDrawable(node.Value);
                        }
                        else if(attr == "state_pressed")
                        {
                            stateList.Add(StateType.StateSelected, ldr.img);
                        }
                        else if(attr == "state_focused")
                        {
                            stateList.Add(StateType.StateHighlighted, ldr.img);
                        }
                        else if(attr == "state_hovered")
                        {
                            stateList.Add(StateType.StateHighlighted, ldr.img);
                        }
                        else if(attr == "state_selected")
                        {
                            stateList.Add(StateType.StateSelected, ldr.img);
                        }
                        else if(attr == "state_checkable")
                        {
                            stateList.Add(StateType.StateCheckable, ldr.img);
                        }
                        else if(attr == "state_checked")
                        {
                            stateList.Add(StateType.StateChecked, ldr.img);
                        }
                        else if(attr == "state_enabled")
                        {
                            stateList.Add(StateType.StateNormal, ldr.img);
                        }
                        else if(attr == "state_activated")
                        {
                            stateList.Add(StateType.StateActivated, ldr.img);
                        }
                        else if(attr == "state_window_focused")
                        {
                            stateList.Add(StateType.StateWindowFocused, ldr.img);
                        }
                    }
                }
            }
   
            stateListDictionary.Add(root.Name.LocalName, stateList);
   
            LGDrawableReturn ret = new LGDrawableReturn();
            ret.img = null;
            ret.state = root.Name.LocalName;
            return ret;
        }

        LGDrawableReturn ParseShape(XElement root)
        {
            String type = @"rectangle";
#if !NETFX_CORE
            int height = (int)Application.Current.Host.Content.ActualHeight, width = (int)Application.Current.Host.Content.ActualWidth;
#else
            int height = (int)Window.Current.Bounds.Height, width = (int)Window.Current.Bounds.Width;
#endif
            //int height = 2000, width = 2000;
            int radius = 0;
            int thickness = 0;
            int cornerTopLeftRadius = 0;
            int cornerTopRightRadius = 0;
            int cornerBottomLeftRadius = 0;
            int cornerBottomRightRadius = 0;
            bool hasGradient = false;
            bool hasCorner = false;
            int gradientAngle = 0;
            float gradientCenterX = 0;
            float gradientCenterY = 0;
            Color? gradientCenterColor = null;
            Color? gradientEndColor = null;
            int gradientRadius = 0;
            Color? gradientStartColor = null;
            String gradientType = "linear";
            int paddingLeft = 0;
            int paddingTop = 0;
            int paddingRight = 0;
            int paddingBottom = 0;
            Color? fillColor = null;
            int strokeWidth = 1;
            Color? strokeColor = null;
            int dashGap = 0;
            int dashWidth = 0;
   
            foreach(XAttribute attr in root.Attributes())
            {
                String attrName = attr.Name.LocalName;
                if(attrName == "shape")
                {
                    String attrVal = attr.Value;
                    type = attrVal;
                }
                else if(attrName == "innerRadius")
                {
                    radius = LGDimensionParser.Instance.GetDimension(attr.Value);
                }
                else if(attrName == "innerRadiusRatio")
                {
           
                }
                else if(attrName == "thickness")
                {
                    thickness = LGDimensionParser.Instance.GetDimension(attr.Value);
                }
                else if(attrName == "thicknessRatio")
                {
           
                }
            }

            foreach(XElement child in root.Elements())
            {
                String name = child.Name.LocalName;
                if(name == "corners")
                {
                    hasCorner = true;
                    foreach(XAttribute childAttr in child.Attributes())
                    {
                        String childAttrName = childAttr.Name.LocalName;
                        if(childAttrName == "radius")
                        {
                            int cornerRadius = LGDimensionParser.Instance.GetDimension(childAttr.Value);
                            cornerTopLeftRadius = cornerRadius;
                            cornerTopRightRadius = cornerRadius;
                            cornerBottomLeftRadius = cornerRadius;
                            cornerBottomRightRadius = cornerRadius;
                        }
                        else if(childAttrName == "topLeftRadius")
                        {
                            cornerTopLeftRadius = LGDimensionParser.Instance.GetDimension(childAttr.Value);
                        }
                        else if(childAttrName == "topRightRadius")
                        {
                            cornerTopRightRadius = LGDimensionParser.Instance.GetDimension(childAttr.Value);
                        }
                        else if(childAttrName == "bottomLeftRadius")
                        {
                            cornerBottomLeftRadius = LGDimensionParser.Instance.GetDimension(childAttr.Value);
                        }
                        else if(childAttrName == "bottomRightRadius")
                        {
                            cornerBottomRightRadius = LGDimensionParser.Instance.GetDimension(childAttr.Value);
                        }
                    }
                }
                else if(name == "gradient")
                {
                    hasGradient = true;
                    foreach(XAttribute childAttr in child.Attributes())
                    {
                        String childAttrName = childAttr.Name.LocalName;
                        if(childAttrName == "angle")
                        {
                            gradientAngle = Convert.ToInt32(childAttr.Value);
                        }
                        else if(childAttrName == "centerX")
                        {
                            gradientCenterX = Convert.ToSingle(childAttr.Value);
                        }
                        else if(childAttrName == "centerY")
                        {
                            gradientCenterY = Convert.ToSingle(childAttr.Value);
                        }
                        else if(childAttrName == "centerColor")
                        {
                            gradientCenterColor = LGColorParser.Instance.ParseColor(childAttr.Value);
                        }
                        else if(childAttrName == "endColor")
                        {
                            gradientEndColor = LGColorParser.Instance.ParseColor(childAttr.Value);
                        }
                        else if(childAttrName == "gradientRadius")
                        {
                            gradientRadius = LGDimensionParser.Instance.GetDimension(childAttr.Value);
                        }
                        else if(childAttrName == "startColor")
                        {
                            gradientStartColor = LGColorParser.Instance.ParseColor(childAttr.Value);
                        }
                        else if(childAttrName == "type")
                        {
                            gradientType = childAttr.Value;
                        }
                    }
                }
                else if(name == "padding")
                {
                    foreach(XAttribute childAttr in child.Attributes())
                    {
                        String childAttrName = childAttr.Name.LocalName;
                        if(childAttrName == "left")
                        {
                            paddingLeft = LGDimensionParser.Instance.GetDimension(childAttr.Value);
                        }
                        else if(childAttrName == "top")
                        {
                            paddingTop = LGDimensionParser.Instance.GetDimension(childAttr.Value);
                        }
                        else if(childAttrName == "right")
                        {
                            paddingRight = LGDimensionParser.Instance.GetDimension(childAttr.Value);
                        }
                        else if(childAttrName == "bottom")
                        {
                            paddingBottom = LGDimensionParser.Instance.GetDimension(childAttr.Value);
                        }
                    }
                }
                else if(name == "size")
                {
                    foreach(XAttribute childAttr in child.Attributes())
                    {
                        String childAttrName = childAttr.Name.LocalName;
                        if(childAttrName == "height")
                        {
                            height = LGDimensionParser.Instance.GetDimension(childAttr.Value);
                        }
                        else if(childAttrName == "width")
                        {
                            width = LGDimensionParser.Instance.GetDimension(childAttr.Value);
                        }
                    }
                }
                else if(name == "solid")
                {
                    foreach(XAttribute childAttr in child.Attributes())
                    {
                        String childAttrName = childAttr.Name.LocalName;
                        if(childAttrName == "color")
                        {
                            fillColor = LGColorParser.Instance.ParseColor(childAttr.Value);
                        }
                    }
                }
                else if(name == "stroke")
                {
                    foreach(XAttribute childAttr in child.Attributes())
                    {
                        String childAttrName = childAttr.Name.LocalName;
                        if(childAttrName == "width")
                        {
                            strokeWidth = LGDimensionParser.Instance.GetDimension(childAttr.Value);
                        }
                        else if(childAttrName == "color")
                        {
                            strokeColor = LGColorParser.Instance.ParseColor(childAttr.Value);
                        }
                        else if(childAttrName == "dashGap")
                        {
                            dashGap = LGDimensionParser.Instance.GetDimension(childAttr.Value);
                        }
                        else if(childAttrName == "dashWidth")
                        {
                            dashWidth = LGDimensionParser.Instance.GetDimension(childAttr.Value);
                        }
                    }
                }
            }

            Canvas canvas = new Canvas();
            GeometryGroup group = new GeometryGroup();
            if(type == "ring")
            {
                canvas.Width = radius;
                canvas.Height = radius;
                if(fillColor != null)
                {
                    
                }
            }
            else
            {
                canvas.Width = width;
                canvas.Height = height;
            }

            if(type == "rectangle")
            {
                RectangleGeometry rect = new RectangleGeometry();
                rect.Rect = new Rect(paddingLeft, paddingTop, width - paddingRight, height - paddingBottom);
                group.Children.Add(rect);
                if (hasCorner)
                {
                    int radiusX = (Math.Abs(cornerTopLeftRadius - cornerTopRightRadius) + Math.Abs(cornerBottomLeftRadius - cornerBottomRightRadius)) / 2;
                    int radiusY = (Math.Abs(cornerTopLeftRadius - cornerBottomLeftRadius) + Math.Abs(cornerTopRightRadius - cornerBottomRightRadius)) / 2;
#if !NETFX_CORE
                    rect.RadiusX = radiusX;
                    rect.RadiusY = radiusY;
#endif
                }
            }
            else if(type == "ring")
            {
                EllipseGeometry eg = new EllipseGeometry();
                eg.Center = new Point(paddingLeft - paddingRight, paddingTop - paddingBottom);
                eg.RadiusX = radius;
                eg.RadiusY = radius;
                group.Children.Add(eg);
            }
            else if(type == "oval")
            {
                EllipseGeometry eg = new EllipseGeometry();
                eg.Center = new Point(paddingLeft - paddingRight, paddingTop - paddingBottom);
                eg.RadiusX = width;
                eg.RadiusY = height;
                group.Children.Add(eg);
            }
            else if(type == "line")
            {
                LineGeometry lg = new LineGeometry();
                lg.StartPoint = new Point(0, 0);
                lg.EndPoint = new Point(width, height);
                group.Children.Add(lg);
            }

#if WINDOWS_PHONE
            System.Windows.Shapes.
#elif NETFX_CORE
            Windows.UI.Xaml.Shapes.
#endif
            Path path = new
#if WINDOWS_PHONE
            System.Windows.Shapes.
#elif NETFX_CORE
            Windows.UI.Xaml.Shapes.
#endif
            Path();
            if(fillColor != null || hasGradient)
            {
                if(hasGradient)
                {
                    GradientBrush gradient = null;
                    if (gradientType == "linear")
                        gradient = new LinearGradientBrush();
                    else if (gradientType == "radial")
#if WINDOWS_PHONE
                        gradient = new RadialGradientBrush();
#elif NETFX_CORE
                        gradient = new LinearGradientBrush();
#endif
                    else
                        gradient = new LinearGradientBrush();
                    if(gradientStartColor != null)
                    {
                        GradientStop gs = new GradientStop();
                        gs.Color = (Color)gradientStartColor;
                        gs.Offset = 0.0;
                        gradient.GradientStops.Add(gs);
                    }
                    if(gradientCenterColor != null)
                    {
                        GradientStop gs = new GradientStop();
                        gs.Color = (Color)gradientCenterColor;
                        if (gradientEndColor == null)
                            gs.Offset = 0.5;
                        else
                            gs.Offset = 0.33;
                        gradient.GradientStops.Add(gs);
                    }
                    if(gradientEndColor != null)
                    {
                        GradientStop gs = new GradientStop();
                        gs.Color = (Color)gradientEndColor;
                        if (gradientCenterColor == null)
                            gs.Offset = 0.5;
                        else
                            gs.Offset = 0.66;
                        gradient.GradientStops.Add(gs);
                    }
                    if(type == "radial")
                    {
#if !NETFX_CORE
                        ((RadialGradientBrush)gradient).Center = new Point(gradientCenterX, gradientCenterY);
                        ((RadialGradientBrush)gradient).RadiusX = gradientRadius;
                        ((RadialGradientBrush)gradient).RadiusY = gradientRadius;
#else
                        //TODO:Radial brush on windows runtime
#endif
                    }
                    else
                    {
                        ((LinearGradientBrush)gradient).StartPoint = new Point(0,0.5);
                        ((LinearGradientBrush)gradient).EndPoint = new Point(1,0.5); 
                    }
                    RotateTransform rtra = new RotateTransform();
                    rtra.Angle = gradientAngle;
                    rtra.CenterX = 0.5;
                    rtra.CenterY = 0.5;
                    gradient.RelativeTransform = rtra;                   

                    path.Fill = gradient;
                }
                else
                {
                    path.Fill = new SolidColorBrush((Color)fillColor);
                }
            }
            if(strokeColor != null)
            {
                path.Stroke = new SolidColorBrush((Color)strokeColor);
                path.StrokeThickness = strokeWidth;
                if(dashGap != 0)
                {
                    DoubleCollection dc = new DoubleCollection();
                    dc.Add(dashWidth);
                    dc.Add(dashGap);
                    path.StrokeDashArray = dc;
                }
            }
            path.Width = width;
            path.Height = height;
            path.Data = group;
            canvas.Children.Add(path);

#if !NETFX_CORE
            WriteableBitmap wb = BitmapFactory.New(width, height);

            wb.Render(canvas, null);
            wb.Invalidate();
            BitmapImage img;
            using (MemoryStream ms = new MemoryStream())
            {
                wb.SaveJpeg(ms, width, height, 0, 100);
                img = new BitmapImage();
                img.SetSource(ms);
            }
            LGDrawableReturn ret = new LGDrawableReturn();
            ret.img = img;
            return ret;
#else
            LGDrawableReturn ret = new LGDrawableReturn();
            RenderTargetBitmap rtb = new RenderTargetBitmap();
            rtb.RenderAsync(canvas, width, height).Completed = (info, status) =>
            {
                ret.img = rtb;
            };
            return ret;
#endif
        }

        #region LuaInterface Members

        public void RegisterEventFunction(string var, LuaTranslator lt)
        {
            
        }

        public string GetId()
        {
            return "LGDrawableParser";
        }

        #endregion
    }
}
