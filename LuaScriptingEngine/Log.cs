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
using LoggerNamespace;

namespace LoggerNamespace
{
    public class Log
    {
        public static void e(String tag, String message)
        {
            Logger.Log(LogType.CONSOLE, LogLevel.ERROR, tag + ":" + message);
        }

        public static void i(String tag, String message)
        {
            Logger.Log(LogType.CONSOLE, LogLevel.INFORM, tag + ":" + message);
        }

        public static void d(String tag, String message)
        {
            Logger.Log(LogType.CONSOLE, LogLevel.DEBUG, tag + ":" + message);
        }
    }
}
