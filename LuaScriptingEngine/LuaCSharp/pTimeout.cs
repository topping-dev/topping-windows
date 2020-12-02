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
using Windows.UI.Xaml.Controls;
#endif

namespace LuaCSharp
{
    public class pTimeout
    {
        public double block;          /* maximum time for blocking calls */
        public double total;          /* total number of miliseconds for operation */
        public double start;          /* time of start of operation */
    }
}
