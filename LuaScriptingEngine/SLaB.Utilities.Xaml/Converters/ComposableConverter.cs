#region Using Directives

using System;
using System.Globalization;
#if !NETFX_CORE
using System.Windows.Data;
using System.Windows.Markup;
#else
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
#endif

#endregion

namespace SLaB.Utilities.Xaml.Converters
{
    /// <summary>
    ///   Creates a DependencyObject that wraps an IValueConverter.
    /// </summary>
    [ContentProperty(
#if NETFX_CORE
        Name=
#endif
        "Converter")]
    public class ComposableConverter : IValueConverter
    {

        /// <summary>
        ///   Gets or sets the Converter for this ComposableConverter.
        /// </summary>
        public IValueConverter Converter { get; set; }




        #region IValueConverter Members

        /// <summary>
        ///   Proxies the call into the Converter.
        /// </summary>
        /// <param name = "value"></param>
        /// <param name = "targetType"></param>
        /// <param name = "parameter"></param>
        /// <param name = "culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
#if !NETFX_CORE
            return this.Converter.Convert(value, targetType, parameter, culture);
#else
            return this.Converter.Convert(value, targetType, parameter, culture.ToString());
#endif
        }

        /// <summary>
        ///   Proxies the call into the Converter.
        /// </summary>
        /// <param name = "value"></param>
        /// <param name = "targetType"></param>
        /// <param name = "parameter"></param>
        /// <param name = "culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
#if !NETFX_CORE
            return this.Converter.ConvertBack(value, targetType, parameter, culture);
#else
            return this.Converter.ConvertBack(value, targetType, parameter, culture.ToString());
#endif
        }

        #endregion
    }
}