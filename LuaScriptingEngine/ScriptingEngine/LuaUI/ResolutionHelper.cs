using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
#if NETFX_CORE
using Windows.Graphics.Display;
#endif

namespace ScriptingEngine.LuaUI
{
    public enum Resolution { WVGA, WXGA, HD720p };

    public static class ResolutionHelper
    {
        private static bool IsWvga
        {
            get
            {
#if WINDOWS_PHONE
#if WP7
                return true;
#else
                return Application.Current.Host.Content.ScaleFactor == 100;
#endif
#elif NETFX_CORE
                return DisplayInformation.GetForCurrentView().LogicalDpi <= 100;
#else
                return false;
#endif
            }
        }

        private static bool IsWxga
        {
            get
            {
#if WINDOWS_PHONE
#if WP7
                return false;
#else
                return Application.Current.Host.Content.ScaleFactor == 160;
#endif
#elif NETFX_CORE
                return DisplayInformation.GetForCurrentView().LogicalDpi > 100 && DisplayInformation.GetForCurrentView().LogicalDpi <= 160;
#else
                return false;
#endif
            }
        }

        private static bool Is720p
        {
            get
            {
#if WINDOWS_PHONE
#if WP7
                return false;
#else
                return Application.Current.Host.Content.ScaleFactor == 150;
#endif
#elif NETFX_CORE
                return DisplayInformation.GetForCurrentView().LogicalDpi > 160;
#else
                return true;
#endif
            }
        }

        public static Resolution CurrentResolution
        {
            get
            {
                if (IsWvga) return Resolution.WVGA;
                else if (IsWxga) return Resolution.WXGA;
                else if (Is720p) return Resolution.HD720p;
                else throw new InvalidOperationException("Unknown resolution");
            }
        }

        public static PageOrientation CurrentOrientation
        {
            get
            {
#if WINDOWS_PHONE
                return ((PhoneApplicationFrame)Application.Current.RootVisual).Orientation;
#elif NETFX_CORE
                if (DisplayInformation.GetForCurrentView().CurrentOrientation == DisplayOrientations.Landscape
                    || DisplayInformation.GetForCurrentView().CurrentOrientation == DisplayOrientations.LandscapeFlipped)
                    return PageOrientation.Landscape;
                else
                    return PageOrientation.Portrait;
#else
                return PageOrientation.LANDSCAPE;
#endif
            }
        }
    }
}
