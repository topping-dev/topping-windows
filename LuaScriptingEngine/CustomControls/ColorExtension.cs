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
using Windows.UI;
#endif

namespace LuaScriptingEngine.CustomControls
{
    public static class ColorExtension
    {
        /**
         * (Ignore)
         */
        public static int ToColorInt(this Color c)
        {
            return (c.A << 24) | (c.R << 16) | (c.G << 8) | c.B;
        }

        /**
         * (Ignore)
         */
        public static Color FromColorInt(int value)
        {
            uint valueU = (uint)value;
            Color c = Color.FromArgb((byte)((value >> 0x18) & 0xff),
                          (byte)((value >> 0x10) & 0xff),
                          (byte)((value >> 8) & 0xff),
                          (byte)(value & 0xff));
            return c;
        }
    }
}
