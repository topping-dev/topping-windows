using System;
using System.Net;
using System.Windows;
using Microsoft.Phone.Controls;
using System.Windows.Input;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Shell;
using System.Windows.Controls.Primitives;
#else
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#endif

namespace LuaScriptingEngine.CustomControls
{
    public class ProgressIndicator : ContentControl
    {       
        private Rectangle backgroundRect;
        private StackPanel stackPanel;
        private ProgressBar progressBar;
        private TextBlock textBlockStatus;

        private ProgressTypes progressType;
        private bool currentSystemTrayState;       
        private static string defaultText = "Loading...";
        private bool showLabel;
        private string labelText;
        private bool cancellable;
        private TextBlock textBlockTitle;
        private Button butCancel;
        private string labelTitle;
        private int progress = -1;
        private int max = -1;
        private string labelCancelText;
        
        
        public ProgressIndicator()
        {
            this.DefaultStyleKey = typeof(ProgressIndicator);
            labelCancelText = "Cancel";
        }

#if WINDOWS_PHONE
        public
#else
        protected
#endif 
            override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.backgroundRect = this.GetTemplateChild("backgroundRect") as Rectangle;
            this.stackPanel = this.GetTemplateChild("stackPanel") as StackPanel;
            this.progressBar = this.GetTemplateChild("progressBar") as ProgressBar;
            this.textBlockTitle = this.GetTemplateChild("textBlockTitle") as TextBlock;
            this.textBlockStatus = this.GetTemplateChild("textBlockStatus") as TextBlock;
            this.butCancel = this.GetTemplateChild("butCancel") as Button;
#if WINDOWS_PHONE
            butCancel.Tap += butCancel_Tap;
#else
            butCancel.Tapped += butCancel_Tap;
#endif

            this.Text = labelText;
            this.Title = labelTitle;
            butCancel.Content = labelCancelText;
            Cancellable = true;
            
            InitializeProgressType();

            if (progress != -1)
                progressBar.Value = progress;
            else if (max != -1)
                progressBar.Maximum = max;
        }

        void butCancel_Tap(object sender, 
#if WINDOWS_PHONE
            System.Windows.Input.GestureEventArgs e)
#else
            TappedRoutedEventArgs e)
#endif
        {
            if (Cancellable)
            {
                this.Hide();
            }
        }

#if WINDOWS_PHONE
        protected override void OnTap(System.Windows.Input.GestureEventArgs e)
#else
        protected override void OnTapped(TappedRoutedEventArgs e)
#endif
        {
            if (Cancellable)
            {
                this.Hide();
            }
        }

        internal Popup ChildWindowPopup
        {
            get;
            private set;
        }

#if WINDOWS_PHONE
        private static PhoneApplicationFrame RootVisual
        {
            get
            {
                return Application.Current == null ? null : Application.Current.RootVisual as PhoneApplicationFrame;
            }
        }
#else
        private static Window RootVisual
        {
            get
            {
                return Window.Current == null ? null : Window.Current;
            }
        }
#endif

        public bool Cancellable
        {
            get
            {
                return cancellable;
            }
            set
            {
                this.cancellable = value;
                if (cancellable)
                    butCancel.Visibility = Visibility.Visible;
                else
                    butCancel.Visibility = Visibility.Collapsed;
            }
        }

        public ProgressTypes ProgressType
        {
            get
            {
                return this.progressType;
            }
            set
            {
                progressType = value;
            }
        }

        public bool ShowLabel
        {
            get
            {
                return this.showLabel;
            }
            set
            {
                this.showLabel = value;
            }
        }

        public string Title
        {
            get
            {
                return labelTitle;
            }
            set
            {
                labelTitle = value;
                if (textBlockTitle != null)
                {
                    if (labelTitle == null)
                        textBlockTitle.Visibility = Visibility.Collapsed;
                    else
                    {
                        textBlockTitle.Visibility = Visibility.Visible;
                        textBlockTitle.Text = value;
                    }
                }
            }
        }

        public string Text
        {
            get
            {
                return labelText;                
            }
            set
            {
                this.labelText = value;
                if (this.textBlockStatus != null)
                {
                    this.textBlockStatus.Text = value;
                }
            }
        }

        public string CancelText
        {
            get
            {
                return labelCancelText;
            }
            set
            {
                this.labelCancelText = value;
                if (this.butCancel != null)
                {
                    butCancel.Content = value;
                }
            }
        }

        public int Progress
        {
            get
            {
                if (this.progressBar == null)
                    return progress;
                else
                    return (int)this.progressBar.Value;
            }
            set
            {
                if (this.progressBar == null)
                    progress = value;
                else
                    this.progressBar.Value = value;
            }
        }

        public int Maximum
        {
            get
            {
                if (this.progressBar == null)
                    return max;
                else
                    return (int)this.progressBar.Maximum;
            }
            set
            {
                if (this.progressBar == null)
                    max = value;
                else
                    this.progressBar.Maximum = value;
            }
        }

        public ProgressBar ProgressBar
        {
            get
            {
                return this.progressBar;
            }
        }

        public new double Opacity
        {
            get
            {
                return this.backgroundRect.Opacity;
            }
            set
            {
                this.backgroundRect.Opacity = value;
            }
        }

        public void Hide()
        {
            // Restore system tray
#if WINDOWS_PHONE
            SystemTray.IsVisible = currentSystemTrayState;
#endif
            this.progressBar.IsIndeterminate = false;
            this.ChildWindowPopup.IsOpen = false;

        }

        public void Show()
        {
            if (this.ChildWindowPopup == null)
            {
                this.ChildWindowPopup = new Popup();

                try
                {
                    this.ChildWindowPopup.Child = this;
                }
                catch (ArgumentException)
                {
                    throw new InvalidOperationException("The control is already shown.");
                }
            }


            if (this.ChildWindowPopup != null && 
#if WINDOWS_PHONE
                Application.Current.RootVisual
#else
                Window.Current
#endif
                != null)
            {
                // Configure accordingly to the type
                InitializeProgressType();

                // Show popup
                this.ChildWindowPopup.IsOpen = true;
            }
        }
      

        private void HideSystemTray()
        {
            // Capture current state of the system tray
#if WINDOWS_PHONE
            this.currentSystemTrayState = SystemTray.IsVisible;
            // Hide it
            SystemTray.IsVisible = false;
#endif
        }

        private void InitializeProgressType()
        {
            this.HideSystemTray();
            if (this.progressBar == null)
                return;

            this.progressBar.Value = 0;


            switch (this.progressType)
            {
                case ProgressTypes.WaitCursor:
                    this.Opacity = 0.7;
                    this.backgroundRect.Visibility = Visibility.Visible;
                    this.stackPanel.VerticalAlignment = VerticalAlignment.Center;
                    this.progressBar.Foreground = (Brush)Application.Current.Resources["PhoneForegroundBrush"];
                    this.textBlockStatus.Text = defaultText;
                    this.textBlockStatus.HorizontalAlignment = HorizontalAlignment.Center;
                    this.textBlockStatus.Visibility = Visibility.Visible;
                    this.textBlockStatus.Margin = new Thickness();
                    this.Height = 800;
                    this.progressBar.IsIndeterminate = true;
                    break;
                case ProgressTypes.DeterminateMiddle:
                    this.Opacity = 0.7;
                    this.backgroundRect.Visibility = Visibility.Visible;
                    this.stackPanel.VerticalAlignment = VerticalAlignment.Center;
                    this.progressBar.Foreground = (Brush)Application.Current.Resources["PhoneAccentBrush"];
                    if (showLabel)
                    {
                        this.textBlockStatus.HorizontalAlignment = HorizontalAlignment.Center;
                        this.textBlockStatus.Visibility = Visibility.Visible;
                        this.textBlockStatus.Margin = new Thickness();
                    }
                    else
                    {
                        this.textBlockStatus.Margin = new Thickness();
                        this.textBlockStatus.Visibility = Visibility.Collapsed;
                    }
                    this.Height = 800;
                    break;
                case ProgressTypes.DeterminateTop:
                    this.Opacity = 0.8;
                    this.backgroundRect.Visibility = Visibility.Visible;
                    this.stackPanel.VerticalAlignment = VerticalAlignment.Top;
                    this.progressBar.Foreground = (Brush)Application.Current.Resources["PhoneAccentBrush"];
                    if (showLabel)
                    {
                        this.textBlockStatus.Visibility = Visibility.Visible;
                        this.textBlockStatus.HorizontalAlignment = HorizontalAlignment.Left;
                        this.textBlockStatus.Margin = new Thickness(18, -5, 0, 0);
                        this.Height = 30;
                    }
                    else
                    {
                        this.textBlockStatus.Visibility = Visibility.Collapsed;
                        this.Height = 4;
                    }

                    break;
                case ProgressTypes.IndeterminateTop:
                    this.Opacity = 0.8;
                    this.backgroundRect.Visibility = Visibility.Visible;
                    this.stackPanel.VerticalAlignment = VerticalAlignment.Top;
                    this.progressBar.Foreground = (Brush)Application.Current.Resources["PhoneAccentBrush"];
                    if (showLabel)
                    {
                        this.textBlockStatus.Visibility = Visibility.Visible;
                        this.textBlockStatus.HorizontalAlignment = HorizontalAlignment.Left;
                        this.textBlockStatus.Margin = new Thickness(18, -5, 0, 0);
                        this.Height = 30;
                    }
                    else
                    {
                        this.textBlockStatus.Visibility = Visibility.Collapsed;
                        this.Height = 4;
                    }
                    this.progressBar.IsIndeterminate = true;
                    break;
                case ProgressTypes.CustomTop:
                    this.stackPanel.VerticalAlignment = VerticalAlignment.Center;
                    this.textBlockStatus.Visibility = Visibility.Visible;
                    if (showLabel)
                    {
                        this.textBlockStatus.Visibility = Visibility.Visible;
                        this.textBlockStatus.HorizontalAlignment = HorizontalAlignment.Left;
                        this.textBlockStatus.Margin = new Thickness(18, -5, 0, 0);
                        this.Height = 30;
                    }
                    else
                    {
                        this.textBlockStatus.Visibility = Visibility.Collapsed;
                        this.Height = 4;
                    }
                    break;
                case ProgressTypes.CustomMiddle:
                    this.stackPanel.VerticalAlignment = VerticalAlignment.Center;
                    if (showLabel)
                    {
                        this.textBlockStatus.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        this.textBlockStatus.Visibility = Visibility.Collapsed;
                    }
                    this.Height = 800;
                    break;
            }
        }

    }
}
