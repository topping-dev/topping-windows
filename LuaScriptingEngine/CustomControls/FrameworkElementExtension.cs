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
using Windows.UI.Xaml;
#endif

namespace LuaScriptingEngine.CustomControls
{
    public class FrameworkElementExtension
    {
        public float Weight;
        public float TotalWeight;
        public Object Tag;
        public static float DefaultWeight = 0.0f;

        public FrameworkElementExtension()
        {
            Weight = DefaultWeight;
            TotalWeight = DefaultWeight;
            Tag = null;
        }
    }

    public static class FrameworkElementExtensionFunctions
    {
        public static FrameworkElementExtension GetInstance(FrameworkElement obj)
        {
            if (obj.Tag == null)
                obj.Tag = new FrameworkElementExtension();

            return (FrameworkElementExtension)obj.Tag;
        }

        public static void SetWeight(this FrameworkElement obj, float value)
        {
            GetInstance(obj).Weight = value;
        }

        public static float GetWeight(this FrameworkElement obj)
        {
            return GetInstance(obj).Weight;
        }

        public static void IncreaseTotalWeight(this FrameworkElement obj, float value)
        {
            GetInstance(obj).TotalWeight += value;
        }

        public static void SetTotalWeight(this FrameworkElement obj, float value)
        {
            GetInstance(obj).TotalWeight = value;
        }

        public static float GetTotalWeight(this FrameworkElement obj)
        {
            return GetInstance(obj).TotalWeight;
        }

        public static void SetTag(this FrameworkElement obj, Object value)
        {
            GetInstance(obj).Tag = value;
        }

        public static Object GetTag(this FrameworkElement obj)
        {
            return GetInstance(obj).Tag;
        }
    }
}
