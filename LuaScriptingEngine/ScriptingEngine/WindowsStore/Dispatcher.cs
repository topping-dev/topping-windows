#if NETFX_CORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace System.Windows
{
    class Dispatcher
    {
        public static void BeginInvoke(DispatchedHandler func)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, func);
        }

        /*public static void HandleAsync()
        {
             volatile bool asyncOperationCompleted = false;

	         Task<void>( AsyncTask ).then([&asyncOperationCompleted] (Task<void> previousTask) { 
		         try{
			        previousTask.get(); 
			        asyncOperationCompleted = true;
		         } catch(Exception e) { 
			         asyncOperationCompleted = true;
		         });

	        while( false == asyncOperationCompleted ) 
		        CoreWindow::GetForCurrentThread()->Dispatcher->ProcessEvents(CoreProcessEventsOption::ProcessOneIfPresent);
        }*/
    }
}

#endif
