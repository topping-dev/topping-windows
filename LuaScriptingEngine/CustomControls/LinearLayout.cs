//#define STACKPANEL
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
    public enum LayoutSystemOrientation
    {
        VERTICAL,
        HORIZONTAL
    }

    public class LinearLayout : 
#if STACKPANEL
        StackPanel
#else
        Grid
#endif
    {
        public LinearLayout()
        {
            Orientation = LayoutSystemOrientation.HORIZONTAL;
            ColumnDefinitions.Clear();
            RowDefinitions.Clear();
        }

#if !STACKPANEL
        private int LastElementCount = 0;
#endif
        public void Add(FrameworkElement child)
        {
            Children.Add(child);
#if !STACKPANEL
            if (Orientation == LayoutSystemOrientation.HORIZONTAL)
            {
                Grid.SetColumn(child, LastElementCount++);
                Grid.SetRow(child, 0);
            }
            else
            {
                Grid.SetColumn(child, 0);
                Grid.SetRow(child, LastElementCount++);
            }
            Setup(child);
#endif
        }

#if !STACKPANEL
        public void Setup(FrameworkElement child)
        {
            if (Orientation == LayoutSystemOrientation.HORIZONTAL)
            {
                if (Double.IsNaN(child.Width))
                {
                    if (child.HorizontalAlignment == HorizontalAlignment.Stretch)
                    {
                        ColumnDefinition cd = new ColumnDefinition();
                        if (child.GetWeight() == FrameworkElementExtension.DefaultWeight)
                            cd.Width = new GridLength(1, GridUnitType.Star);
                        else
                            cd.Width = new GridLength(/*(this.GetTotalWeight() - */child.GetWeight()/*)*/ / this.GetTotalWeight(), GridUnitType.Star);
                        ColumnDefinitions.Add(cd);
                    }
                    else
                    {
                        ColumnDefinition cd = new ColumnDefinition();
                        cd.Width = new GridLength(1, GridUnitType.Auto);
                        ColumnDefinitions.Add(cd);
                    }
                }
                else
                {
                    ColumnDefinition cd = new ColumnDefinition();
                    cd.Width = new GridLength(child.Width, GridUnitType.Auto);
                    ColumnDefinitions.Add(cd);
                }
            }
            else
            {
                if (Double.IsNaN(child.Height))
                {
                    if (child.VerticalAlignment == VerticalAlignment.Stretch)
                    {
                        RowDefinition rd = new RowDefinition();
                        if (child.GetWeight() == FrameworkElementExtension.DefaultWeight)
                            rd.Height = new GridLength(1, GridUnitType.Star);
                        else
                            rd.Height = new GridLength(/*(this.GetTotalWeight() - */child.GetWeight()/*)*/ / this.GetTotalWeight(), GridUnitType.Star);
                        RowDefinitions.Add(rd);
                    }
                    else
                    {
                        RowDefinition rd = new RowDefinition();
                        rd.Height = new GridLength(1, GridUnitType.Auto);
                        RowDefinitions.Add(rd);
                    }
                }
                else
                {
                    RowDefinition rd = new RowDefinition();
                    rd.Height = new GridLength(child.Height, GridUnitType.Auto);
                    RowDefinitions.Add(rd);
                }
            }
        }

        public void SetupOrientation()
        {
            ColumnDefinitions.Clear();
            RowDefinitions.Clear();
            if (Orientation == LayoutSystemOrientation.HORIZONTAL)
            {
                RowDefinition rd = new RowDefinition();
                if(HorizontalAlignment == HorizontalAlignment.Stretch)
                    rd.Height = new GridLength(1, GridUnitType.Star);
                else
                    rd.Height = new GridLength(1, GridUnitType.Auto);
                RowDefinitions.Add(rd);
            }
            else
            {
                ColumnDefinition cd = new ColumnDefinition();
                if (VerticalAlignment == VerticalAlignment.Stretch)
                    cd.Width = new GridLength(1, GridUnitType.Star);
                else
                    cd.Width = new GridLength(1, GridUnitType.Auto);
                ColumnDefinitions.Add(cd);
            }
        }
#endif

        private LayoutSystemOrientation _Orientation;
        public 
#if STACKPANEL
        new 
#endif
        LayoutSystemOrientation Orientation 
        {
            get
            {
                return _Orientation;
            }
            set
            {
                _Orientation = value;
#if STACKPANEL
                if (Orientation == LayoutSystemOrientation.VERTICAL)
                    base.Orientation = System.Windows.Controls.Orientation.Vertical;
                else
                    base.Orientation = System.Windows.Controls.Orientation.Horizontal;
#else
                SetupOrientation();
#endif
            }
        }
    }
}
