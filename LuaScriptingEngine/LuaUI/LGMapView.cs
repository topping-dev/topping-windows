using System;
using System.Net;
using System.Windows;
using LuaScriptingEngine.CustomControls;
using System.IO;
using LoggerNamespace;
using System.Windows.Input;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
#else
using Bing.Maps;
using Windows.UI.Xaml.Media.Imaging;
#endif
#if WINDOWS_PHONE
using Microsoft.Phone.Controls.Maps;
#endif

namespace ScriptingEngine.LuaUI
{
    [LuaClass("LGMapView")]
    public class LGMapView : LGView
    {
        public LGMapView(LuaContext context)
            : base(context)
        {
        }

        public LGMapView(LuaContext context, String luaId)
            : base(context, luaId)
        {
        }

        public override void Setup()
        {
            view = new Map();
        }

        public LuaMapCircle AddCircle(LuaPoint geoLoc, double radius, LuaColor strokeColor, LuaColor fillColor)
        {
            Map map = ((Map)view);

            MapCircle mc = new MapCircle();
            mc.setRadius(radius);
            mc.setStrokeColor(strokeColor.GetColorObject());
            mc.setFillColor(fillColor.GetColorObject());
            mc.setCenter(geoLoc.ToPoint());
            LuaMapCircle lmc = new LuaMapCircle(mc);
            return lmc;
        }

        public LuaMapImage AddImage(LuaPoint geoPoint, String path, String icon, float width)
        {
            MapImage mi = null;
            if (icon != null)
            {
                LuaStream ls = LuaResource.GetResource(path, icon);
                if (ls != null && ls.GetStream() != null)
                {
                    BitmapImage bi = new BitmapImage();
                    bi.SetSource(
#if NETFX_CORE
                        new MemoryRandomAccessStream((Stream)ls.GetStream())
#else
                        (Stream)ls.GetStream()
#endif
);
                    mi = new MapImage(bi);
                    mi.setDimensions(width);
                    mi.setPosition(geoPoint.ToPoint());
                }
            }
            else
                Log.e("LGMapView", "Image is needed for LuaMapImage");

            if (mi != null)
            {
                LuaMapImage lmi = new LuaMapImage(mi);
                return lmi;
            }
            return null;
        }
    }
}
