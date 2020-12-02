using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using LuaScriptingEngine.CustomControls;
using System.IO;
using System.Text;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SLaB.Utilities.Xaml.Serializer.UI;
using System.Windows.Markup;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Controls;
#endif

namespace ScriptingEngine.LuaUI
{
    //Her position için template seçecek
    internal class LuaTemplateSelector : LuaScriptingEngine.CustomControls.DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            AdapterItem ai = (AdapterItem)item;
#if WINDOWS_PHONE
            SLaB.Utilities.Xaml.Serializer.UI.
            UiXamlSerializer uxs = new UiXamlSerializer();
            String xaml = uxs.Serialize(ai.view.GetView());
            DataTemplate datatemplate = (DataTemplate)XamlReader.Load(xaml);
            return datatemplate;
#else
            ai.view.GetView();
            return null;
#endif
        }
    }

    [LuaClass("LGListView")]
    public class LGListView : LGView
    {
        public LGAdapterView root;

        public LGListView(LuaContext context)
            : base(context)
        {
        }

        public LGListView(LuaContext context, String luaId)
            : base(context, luaId)
        {
        }

        public override void Setup()
        {
            view = new ListBox();
            ((ListBox)view).SelectionChanged += new SelectionChangedEventHandler(LGListView_SelectionChanged);
        }

        void LGListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (root != null)
            {
                if (root.GetItemChanged() != null)    //Detail View
                    root.GetItemChanged().CallIn(this,         null, ((ListBox)view).SelectedIndex, ((ListBox)view).SelectedItem);
            }
            ((ListBox)view).SelectedItem = null;
            //((ListBox)view).SelectedItems.Clear();
        }

        /**
         * Gets the LGAdapterView of listview
         */
        [LuaFunction(false)]
        public LGAdapterView GetAdapter()
        {
            return root;
        }

        /**
         * Sets the LGAdapterView of listview
         * @param adapter
         */
        [LuaFunction(typeof(LGAdapterView))]
        public void SetAdapter(LGAdapterView adapter)
        {
            root = adapter;
            adapter.par = this;
            ListBox lb = ((ListBox)view);
#if NETFX_CORE
            System.Windows.
#endif
            Dispatcher.BeginInvoke(() =>
            {
                lb.ItemsSource = adapter;
            });
        }
    }
}
