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
using Windows.UI.Xaml;
#endif

namespace LuaScriptingEngine
{
    /// <summary>
    /// Details about a Windows Phone 7.0 theme
    /// </summary>
    public class Theme
    {
        /// <summary>
        /// Cache of the current theme.
        /// </summary>
        /// <remarks>
        /// The current theme can be cached, as long as Windows Phone does not support multitasking. In the current version
        /// when the application is reactivated after changing the theme the cached Theme instance is lost and will be
        /// recreated when accessing Theme.Current the next time.
        /// </remarks>
        private static Theme _currentTheme;
        /// <summary>
        /// Reference color for the background color of the light theme
        /// </summary>
        private static readonly Color _lightThemeBackground = Color.FromArgb(255, 255, 255, 255);
        /// <summary>
        /// Reference color for the background color of the dark theme
        /// </summary>
        private static readonly Color _darkThemeBackground = Color.FromArgb(255, 0, 0, 0);


        /// <summary>
        /// Background (as set in System - Theme - Background)
        /// </summary>
        public ThemeBackground Background { get; private set; }
        /// <summary>
        /// Accent color (as set in System - Theme - Accent color)
        /// </summary>
        public Color AccentColor { get; private set; }
        /// <summary>
        /// Resource suffix for the current theme that can be used to retrieve a resource based on the currently active theme
        /// </summary>
        /// <example>
        /// itemListBox.ItemTemplate = itemListBox.Resources["itemTemplate" + Utilities.Theme.Current.ResourceSuffix] as DataTemplate;
        /// </example>
        public string ResourceSuffix { get { return Background.ToString() + "Theme"; } }
        /// <summary>
        /// Currently active theme
        /// </summary>
        public static Theme Current
        {
            get
            {
                if (_currentTheme == null)
                {
                    _currentTheme = DetectCurrentTheme();
                }

                return _currentTheme;
            }
        }


        private Theme()
        {

        }


        /// <summary>
        /// Detects the current theme
        /// </summary>
        /// <returns>Theme currently active</returns>
        private static Theme DetectCurrentTheme()
        {
            Theme currentTheme = new Theme();

            // Detect background
            Color backgroundBrush = (Color)Application.Current.Resources["PhoneBackgroundColor"];

            if (backgroundBrush == _lightThemeBackground)
            {
                currentTheme.Background = ThemeBackground.Light;
            }
            else if (backgroundBrush == _darkThemeBackground)
            {
                currentTheme.Background = ThemeBackground.Dark;
            }
            else
            {
                throw new Exception("Unsupported theme");
            }

            // Detect accent color
            currentTheme.AccentColor = (Color)Application.Current.Resources["PhoneAccentColor"];

            return currentTheme;
        }
    }


    /// <summary>
    /// Enumerator for the available theme backgrounds
    /// </summary>
    public enum ThemeBackground
    {
        Dark,
        Light
    }
}
