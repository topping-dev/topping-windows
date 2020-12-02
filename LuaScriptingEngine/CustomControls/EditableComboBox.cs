using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
#if WINDOWS_PHONE
using Microsoft.Phone.Controls;
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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
#endif

namespace LuaScriptingEngine.CustomControls
{
    public class EditableComboBox : 
#if WINDOWS_PHONE
        PhoneTextBox
#else
        ComboBox
#endif
    {
#if WINDOWS_PHONE
        private ListBox lb;
        CustomMessageBox cmb;
        List<object> list;
#endif
        ResourceDictionary resourceDictionary;
        public bool ShowCustom { get; set; }
        public bool ShowCancel { get; set; }
        public String CustomString { get; set; }
        public String CancelString { get; set; }
        private ComboData selectedData;
        public ComboData SelectedData
        {
            get
            {
                return selectedData;
            }
        }

#if WINDOWS_PHONE
        // delegate declaration
        public delegate void OnSelectionChange(object sender, ComboData cd);

        // event declaration
        public event OnSelectionChange OnSelectionChanged;
#endif

        public struct ComboData
        {
            public String name { get; set; }
            public Object tag { get; set; }

            public override string ToString()
            {
                return name;
            }
        };

        public EditableComboBox()
        {
#if WINDOWS_PHONE
            ActionIcon = ResourceHelper.GetBitmap("CustomControls/Toolkit.Content/ApplicationBar.Select.png");
            ActionIconTapped += new EventHandler(EditableComboBox_ActionIconTapped);
            Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(EditableComboBox_Tap);
            IsReadOnly = true;
            list = new List<object>();
#else

#endif
            ShowCustom = false;
            ShowCancel = false;
            CustomString = "Delete";
            CancelString = "Cancel";

            resourceDictionary = Defines.GetGenericResourceDictionary();
        }

#if WINDOWS_PHONE
        void EditableComboBox_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (IsReadOnly)
            {
                lb = new ListBox();
                foreach (object o in list)
                    lb.Items.Add(o);
                lb.SelectionChanged += new SelectionChangedEventHandler(lb_SelectionChanged);
                lb.ItemTemplate = (DataTemplate)resourceDictionary["EditableComboBoxTemplate"];
                lb.ItemContainerStyle = (Style)resourceDictionary["EditableComboBoxListBoxStyle"];
                cmb = new CustomMessageBox();
                cmb.Content = lb;
                if (ShowCancel)
                    cmb.LeftButtonContent = CancelString;
                if (ShowCustom)
                    cmb.RightButtonContent = CustomString;
                cmb.IsFullScreen = true;
                cmb.Show();
            }
        }

        void EditableComboBox_ActionIconTapped(object sender, EventArgs e)
        {
            lb = new ListBox();
            foreach (object o in list)
                lb.Items.Add(o);
            lb.SelectionChanged += new SelectionChangedEventHandler(lb_SelectionChanged);
            lb.ItemTemplate = (DataTemplate)resourceDictionary["EditableComboBoxTemplate"];
            lb.ItemContainerStyle = (Style)resourceDictionary["EditableComboBoxListBoxStyle"];
            cmb = new CustomMessageBox();
            cmb.Content = lb;
            if (ShowCancel)
                cmb.LeftButtonContent = CancelString;
            if (ShowCustom)
                cmb.RightButtonContent = CustomString;
            cmb.IsFullScreen = true;
            cmb.Show();
        }

        void lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Text = lb.SelectedItem.ToString();
            selectedData = (ComboData)lb.SelectedItem;
            if(OnSelectionChanged != null)
                OnSelectionChanged(this, (ComboData)lb.SelectedItem);
            cmb.Dismiss();
            lb = null;
            cmb = null;
        }
#else
#endif

        public void AddItem(ComboData cd)
        {
#if WINDOWS_PHONE
            list.Add(cd);
#else
            this.Items.Add(cd);
#endif
            
        }

        public void SetSelected(Int32 index)
        {
#if WINDOWS_PHONE
            if (index < 0 || index >= list.Count)
                return;
            ComboData cd = (ComboData)list[index];
            Text = cd.ToString();
#else
            if(index < 0 || index > Items.Count)
                return;
            ComboData cd = (ComboData)Items[index];
#endif
            selectedData = cd;
#if WINDOWS_PHONE
            if (OnSelectionChanged != null)
                OnSelectionChanged(this, cd);
#else
            SelectedIndex = index;
#endif
        }

        public void AddItem(String name, Object tag)
        {
            ComboData cd = new ComboData();
            cd.name = name;
            cd.tag = tag;
#if WINDOWS_PHONE
            list.Add(cd);
#else
            Items.Add(cd);
#endif
        }

        public void DeleteItem(ComboData cd)
        {
#if WINDOWS_PHONE
            list.Remove(cd);
#else
            Items.Remove(cd);
#endif
        }

        public void DeleteItem(String name)
        {
            ComboData? cdToRemove = null;
            bool found = false;
            foreach (ComboData cd in 
#if WINDOWS_PHONE
                lb.Items
#else
                Items
#endif
                )
            {
                if (cd.name == name)
                {
                    cdToRemove = cd;
                    found = true;
                    break;
                }
            }
            if (found)
#if WINDOWS_PHONE
                list.Remove(cdToRemove);
#else
                Items.Remove(cdToRemove);
#endif
        }

#if !WINDOWS_PHONE
        public bool IsReadOnly
        {
            get
            {
                return IsEnabled;
            }
            set
            {
                IsEnabled = value;
            }
        }
#endif
    }
}
