using System;
using System.Net;
using System.Windows;
using ScriptingEngine.LuaUI;
#if WINDOWS_PHONE
using Coding4Fun.Phone.Controls;
#else
using Coding4Fun.Toolkit.Controls;
#endif
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
using Windows.UI.Notifications;
#endif

namespace ScriptingEngine
{
    [LuaClass("LuaToast")]
    [LuaGlobalInt(
		new String[] { "TOAST_SHORT", "TOAST_LONG" },
        new Int32[] { 2000, 3500 }
	)]
    public class LuaToast
    {
        /**
	     * Show the toast
	     * @param context
	     * @param text text to show
	     * @param duration duration as milliseconds or TOAST_SHORT or TOAST_LONG
	     */
	    [LuaFunction(typeof(LuaContext), typeof(String), typeof(Int32))]
	    public static void Show(LuaContext context, String text, Int32 duration)
	    {
#if WINDOWS_PHONE
            ToastPrompt toast = new ToastPrompt();
            toast.Title = "";
            toast.Message = text;
            toast.TextOrientation = System.Windows.Controls.Orientation.Vertical;
            toast.MillisecondsUntilHidden = duration;

            toast.Show();
#else
            // Toasts use a predefined set of standard templates to display their content.
            // The updates happen by sending a XML fragment to the Toast Notification Manager.
            // To make things easier, we will get the template for a toast iwth text as a base, and modify it from there
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);

            // Find the 'text' element in the template's XML, and insert the text "A sample toast" into it.
            var message = toastXml.GetElementById("1");
            message.NodeValue = text;

            // Create a ToastNotification from our XML, and send it to the Toast Notification Manager
            var toast = new ToastNotification(toastXml);
            toast.ExpirationTime = new DateTimeOffset(DateTime.Now, new TimeSpan(duration * 10000));
            ToastNotificationManager.CreateToastNotifier().Show(toast);
#endif
	    }
    }
}
