using System;
using System.Net;
using System.Windows;
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
#endif

namespace System.Windows.Markup
{
    public interface IXamlTypeResolver
    {
        Type Resolve(string qualifiedTypeName);
    }
}
