using System;
using System.Net;
using System.Windows;
using System.IO;
using LuaScriptingEngine;
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
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
#endif

namespace ScriptingEngine.LuaUI
{
    [LuaClass("LGImageView")]
    public class LGImageView : LGView
    {
        /**
	     * Creates LGImageView Object From Lua.
	     * @param lc
	     * @param tag String tag
	     * @return LGImageView
	     */
	    [LuaFunction(typeof(LuaContext), typeof(String))]
	    public static LGImageView Create(LuaContext lc, String tag)
	    {
		    LGImageView iv = new LGImageView(lc);
		    iv.Tag = tag;
		    return iv;		
	    }

        public LGImageView(LuaContext context)
            : base(context)
        {
        }

        public LGImageView(LuaContext context, String luaId)
            : base(context, luaId)
        {
        }

        public override void Setup()
        {
            view = new Image();
        }

        /**
            * Sets the image view from LuaStream stream
            * @param stream
            */
        [LuaFunction(typeof(LuaStream))]
        void SetImage(LuaStream stream)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                Defines.CopyStream((Stream)stream.GetStream(), memoryStream);
                BitmapImage imageSource = new BitmapImage();
                
#if NETFX_CORE
                //TODO:Is there any other elegant solution for this.
                imageSource.SetSource(memoryStream.AsRandomAccessStream());
#else
                imageSource.SetSource(memoryStream);
#endif

                // Assign the Source property of your image
                ((Image)view).Source = imageSource;
            }
#if !NETFX_CORE
            ((Stream)stream.GetStream()).Close();
#endif
        }
    }
}
