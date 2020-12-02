using System;
using System.Net;
using System.Windows;
using ScriptingEngine.LuaUI;
using Microsoft.Phone.Controls;
using LuaScriptingEngine.CustomControls;
using System.Windows.Input;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
#else
using Windows.UI.Xaml.Controls;
using Windows.UI.Popups;
#endif

namespace ScriptingEngine
{
    /**
     * Lua dialog class.
     * This class is used to create dialogs and manupilate it from lua.
     * There are five types of dialogs.
     * DIALOG_TYPE_NORMAL
     * DIALOG_TYPE_PROGRESS
     * DIALOG_TYPE_PROGRESS_INDETERMINATE
     * DIALOG_TYPE_DATEPICKER
     * DIALOG_TYPE_TIMEPICKER
     */
    [LuaClass("LuaDialog")]
    [LuaGlobalInt(
            new string[]
            {
                "DIALOG_TYPE_NORMAL",
			    "DIALOG_TYPE_PROGRESS",
			    "DIALOG_TYPE_PROGRESS_INDETERMINATE",
			    "DIALOG_TYPE_DATEPICKER",
			    "DIALOG_TYPE_TIMEPICKER"
            },
            new int[]
            {
                1,
			    2,
			    6,
			    8,
			    16
            })]
    public class LuaDialog : LuaInterface
    {
#if WINDOWS_PHONE
        Control dialog = null;
#elif NETFX_CORE
        MessageDialog dialog;
#endif
        private LuaTranslator OnPositiveButton;
        private LuaTranslator OnNegativeButton;
        private int dialogType = DIALOG_TYPE_NORMAL;
        private const int DIALOG_TYPE_NORMAL = 0x01;
	    private const int DIALOG_TYPE_PROGRESS = 0x02;
	    private const int DIALOG_TYPE_PROGRESS_INDETERMINATE = DIALOG_TYPE_PROGRESS | 0x04;
	    private const int DIALOG_TYPE_DATEPICKER = 0x08;
	    private const int DIALOG_TYPE_TIMEPICKER = 0x10;
        /**
	     * Shows the LuaDialog
	     * @param context lua context value
	     * @param title title text
	     * @param content content text
	     * @param flags MESSAGEBOX_ flags
	     */
	    [LuaFunction(typeof(LuaContext), typeof(String), typeof(String))]
	    public static void MessageBox(LuaContext context, String title, String content)
	    {
#if WINDOWS_PHONE
            Deployment.Current.
#endif
            Dispatcher.BeginInvoke(() =>
            {
#if WINDOWS_PHONE
                System.Windows.MessageBox.Show(content, title, MessageBoxButton.OK);
#elif NETFX_CORE
                MessageDialog md = new MessageDialog(content, title);
                md.Commands.Add(new UICommand("OK", new UICommandInvokedHandler((cmd) => { })));
                md.ShowAsync();
#endif
            });
	    }
	
	    /**
	     * Creates LuaDialog for build
	     * @param context
	     * @return LuaDialog
	     */
	    [LuaFunction(typeof(LuaContext), typeof(int))]
	    public static LuaDialog Create(LuaContext context, int dialogType)
	    {
		    LuaDialog ld = new LuaDialog();
            ld.dialogType = dialogType;
            switch (dialogType)
            {
                case DIALOG_TYPE_NORMAL:
                    {
#if WINDOWS_PHONE
                        ld.dialog = new CustomMessageBox()
                        {
                            Caption = "",
                            Message = "",
                            LeftButtonContent = null,
                            RightButtonContent = null,
                        };
                        ((CustomMessageBox)ld.dialog).Dismissed += ld.LuaDialog_Dismissed;
#else
                        ld.dialog = new MessageDialog("");
                        ld.dialog.Title = "";
                        ld.dialog.Commands.Add(new UICommand("OK", new UICommandInvokedHandler((cmd) => { ld.LuaDialog_Dismissied(0); })));
                        ld.dialog.DefaultCommandIndex = 0;
                        ld.dialog.CancelCommandIndex = 0;
#endif
                    } break;
                case DIALOG_TYPE_PROGRESS:
                    {
#if WINDOWS_PHONE
                        ld.dialog = new ProgressIndicator();
                        ((ProgressIndicator)ld.dialog).ProgressType = ProgressTypes.DeterminateMiddle;
#endif
                    } break;
                case DIALOG_TYPE_PROGRESS_INDETERMINATE:
                    {
#if WINDOWS_PHONE
                        ld.dialog = new ProgressIndicator();
                        ((ProgressIndicator)ld.dialog).ProgressType = ProgressTypes.WaitCursor;
#endif
                    } break;
                case DIALOG_TYPE_DATEPICKER:
                    {
#if WINDOWS_PHONE
                        ld.dialog = new CustomMessageBox()
                        {
                            Caption = "",
                            Message = "",
                            LeftButtonContent = "Select",
                            RightButtonContent = "Cancel",
                            Content = new DatePicker()
                        };
                        ((CustomMessageBox)ld.dialog).Dismissed += ld.LuaDialog_Dismissed;
#endif
                    } break;
                case DIALOG_TYPE_TIMEPICKER:
                    {
#if WINDOWS_PHONE
                        ld.dialog = new CustomMessageBox()
                        {
                            Caption = "",
                            Message = "",
                            LeftButtonContent = "Select",
                            RightButtonContent = "Cancel",
                            Content = new TimePicker()
                        };
                        ((CustomMessageBox)ld.dialog).Dismissed += ld.LuaDialog_Dismissed;
#endif
                    } break;
            }
		    return ld;
	    }

#if WINDOWS_PHONE
        public void LuaDialog_Dismissed(object sender, DismissedEventArgs e)
        {
            if (e.Result == CustomMessageBoxResult.LeftButton) //Positive
            {
                if (OnPositiveButton != null)
                    OnPositiveButton.CallIn(this);
            }
            else if (e.Result == CustomMessageBoxResult.RightButton //Cancel
                || e.Result == CustomMessageBoxResult.None)
            {
                if (OnNegativeButton != null)
                    OnNegativeButton.CallIn(this);
            }
        }
#elif NETFX_CORE
        public void LuaDialog_Dismissied(int result)
        {
            if (result == 0)
            {
            }
        }
#endif
	
	    /**
	     * Sets the positive button of LuaDialog
	     * @param title title of the button
	     * @param action action to do when button is pressed
	     */
	    [LuaFunction(typeof(String), typeof(LuaTranslator))]
	    public void SetPositiveButton(String title, LuaTranslator action)
	    {
#if WINDOWS_PHONE
            ((CustomMessageBox)dialog).LeftButtonContent = title;
#elif NETFX_CORE
            if (dialog.DefaultCommandIndex == 0 && dialog.CancelCommandIndex == 0)
                dialog.Commands.Clear();
            dialog.Commands.Add(new UICommand(title, new UICommandInvokedHandler((cmd) => { OnPositiveButton.CallIn(cmd); })));
            dialog.DefaultCommandIndex = (uint)dialog.Commands.Count;
#endif
            OnPositiveButton = action;
	    }
	
	    /**
	     * Sets the negative button of LuaDialog
	     * @param title title of the button
	     * @param action action to do when button is pressed
	     */
	    [LuaFunction(typeof(String), typeof(LuaTranslator))]
	    public void SetNegativeButton(String title, LuaTranslator action)
	    {
#if WINDOWS_PHONE
            ((CustomMessageBox)dialog).RightButtonContent = title;
#elif NETFX_CORE
            if (dialog.DefaultCommandIndex == 0 && dialog.CancelCommandIndex == 0)
                dialog.Commands.Clear();
            dialog.Commands.Add(new UICommand(title, new UICommandInvokedHandler((cmd) => { OnPositiveButton.CallIn(cmd); })));
            dialog.CancelCommandIndex = (uint)dialog.Commands.Count;
#endif
		    OnNegativeButton = action;
	    }
	
	    /**
	     * Sets the title of the LuaDialog
	     * @param title
	     */
	    [LuaFunction(typeof(String))]
	    public void SetTitle(String title)
	    {
            switch (dialogType)
            {
                case DIALOG_TYPE_NORMAL:
                case DIALOG_TYPE_DATEPICKER:
                case DIALOG_TYPE_TIMEPICKER:
                    {
#if WINDOWS_PHONE
                        ((CustomMessageBox)dialog).Title = title;
#elif NETFX_CORE
                        dialog.Title = title;
#endif
                    } break;
                case DIALOG_TYPE_PROGRESS:
                case DIALOG_TYPE_PROGRESS_INDETERMINATE:
                    {
#if WINDOWS_PHONE
                        ((ProgressIndicator)dialog).ShowLabel = true;
                        ((ProgressIndicator)dialog).Text = title;
#endif
                    } break;
            }
	    }
	
	    /**
	     * Sets the message of the LuaDialog
	     * @param message
	     */
	    [LuaFunction(typeof(String))]
	    public void SetMessage(String message)
	    {
            switch (dialogType)
            {
                case DIALOG_TYPE_NORMAL:
                case DIALOG_TYPE_DATEPICKER:
                case DIALOG_TYPE_TIMEPICKER:
                    {
#if WINDOWS_PHONE
                        ((CustomMessageBox)dialog).Message = message;
#elif NETFX_CORE
                        dialog.Content = message;
#endif
                    } break;
                case DIALOG_TYPE_PROGRESS:
                case DIALOG_TYPE_PROGRESS_INDETERMINATE:
                    {
                        
                    } break;
            }
	    }

	    /**
	     * Sets the value of the progress bar
	     * (progress bar is needed otherwise it wont effect anything)
	     * @param intvalue
	     */
	    [LuaFunction(typeof(int))]
	    public void SetProgress(int value)
	    {
#if WINDOWS_PHONE
            if ((dialogType & DIALOG_TYPE_PROGRESS) > 0)
                ((ProgressIndicator)dialog).Progress = value;
#endif
	    }
	
	    /**
	     * Sets the maximum value of the progress bar
	     * (progress bar is needed otherwise it wont effect anything)
	     * @param intvalue
	     */
	    [LuaFunction(typeof(int))]
	    public void SetMax(int value)
	    {
#if WINDOWS_PHONE
            if ((dialogType & DIALOG_TYPE_PROGRESS) > 0)
                ((ProgressIndicator)dialog).Maximum = value;                
#endif
	    }
	
	    /**
	     * Sets the date of the date picker
	     * (date picker dialog is needed otherwise it wort effect anything)
	     * @param date
	     */
	    [LuaFunction(typeof(LuaDate))]
	    public void SetDate(LuaDate date)
	    {
#if WINDOWS_PHONE
            if (dialogType == DIALOG_TYPE_DATEPICKER)
                ((DatePicker)(((CustomMessageBox)dialog).Content)).Value = new DateTime(date.GetYear(), date.GetMonth(), date.GetDay(), 0, 0, 0, 0);
#endif
	    }
	
	    /**
	     * Sets the date of the date picker
	     * (date picker dialog is needed otherwise it wort effect anything)
	     * @param day
	     * @param month
	     * @param year
	     */
	    [LuaFunction(typeof(int), typeof(int), typeof(int))]
	    public void SetDateManual(int day, int month, int year)
	    {
#if WINDOWS_PHONE
		    if(dialogType == DIALOG_TYPE_DATEPICKER)
                ((DatePicker)(((CustomMessageBox)dialog).Content)).Value = new DateTime(year, month, day, 0, 0, 0, 0);
#endif
	    }

	    /**
	     * Sets the time of the time picker
	     * (time picker dialog is needed otherwise it wort effect anything)
	     * @param date
	     */
	    [LuaFunction(typeof(LuaDate))]
	    public void SetTime(LuaDate date)
	    {
#if WINDOWS_PHONE
            if (dialogType == DIALOG_TYPE_TIMEPICKER)
                ((TimePicker)(((CustomMessageBox)dialog).Content)).Value = new DateTime(0, 0, 0, date.GetHour(), date.GetMinute(), 0, 0);
#endif
	    }
	
	    /**
	     * Sets the time of the time picker
	     * (time picker dialog is needed otherwise it wort effect anything)
	     * @param hour
	     * @param minute
	     */
	    [LuaFunction(typeof(int), typeof(int))]
	    public void SetTimeManual(int hour, int minute)
	    {
#if WINDOWS_PHONE
            if (dialogType == DIALOG_TYPE_TIMEPICKER)
            {
                DateTime dtNow = DateTime.Now;
                ((TimePicker)(((CustomMessageBox)dialog).Content)).Value = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, hour, minute, 0, 0);
            }
#endif
	    }
	
	    /**
	     * Shows the created dialog of LuaDialog
	     */
	    [LuaFunction(false)]
	    public void Show()
	    {
#if WINDOWS_PHONE
            Deployment.Current.
#endif
            Dispatcher.BeginInvoke(() =>
            {
                switch (dialogType)
                {
                    case DIALOG_TYPE_NORMAL:
                    case DIALOG_TYPE_DATEPICKER:
                    case DIALOG_TYPE_TIMEPICKER:
                        {
#if WINDOWS_PHONE
                            ((CustomMessageBox)dialog).Show();
#elif NETFX_CORE
                            dialog.ShowAsync();
#endif
                        } break;
                    case DIALOG_TYPE_PROGRESS:
                    case DIALOG_TYPE_PROGRESS_INDETERMINATE:
                        {
#if WINDOWS_PHONE
                            ((ProgressIndicator)dialog).Show();
#elif NETFX_CORE
                            dialog.ShowAsync();
#endif
                        } break;
                }
            });
	    }

        /**
	     * Dismiss the created dialog
	     */
	    [LuaFunction(false)]
	    public void Dismiss()
	    {
#if WINDOWS_PHONE
            Deployment.Current.
#endif
            Dispatcher.BeginInvoke(() =>
            {
		        switch (dialogType)
                {
                    case DIALOG_TYPE_NORMAL:
                    case DIALOG_TYPE_DATEPICKER:
                    case DIALOG_TYPE_TIMEPICKER:
                        {
#if WINDOWS_PHONE
                            ((CustomMessageBox)dialog).Dismiss();
#elif NETFX_CORE
#endif
                        } break;
                    case DIALOG_TYPE_PROGRESS:
                    case DIALOG_TYPE_PROGRESS_INDETERMINATE:
                        {
#if WINDOWS_PHONE
                            ((ProgressIndicator)dialog).Hide();
#elif NETFX_CORE
#endif
                        } break;
                }
            });
	    }
	
	    /**
	     * Frees LuaDialog.
	     */
	    [LuaFunction(false)]
	    public void Free()
	    {
            dialog = null;
	    }
    
        #region LuaInterface Members

        public void  RegisterEventFunction(string var, LuaTranslator lt)
        {
 	        
        }

        public string  GetId()
        {
            return "LuaDialog";
        }

        #endregion
    }
}
