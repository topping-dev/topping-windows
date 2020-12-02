using System;
using System.Net;
using System.Windows;
using LuaScriptingEngine.CustomControls;
using Microsoft.Phone.Controls;
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

namespace ScriptingEngine.LuaUI
{
    [LuaClass("LGComboBox")]
    public class LGComboBox : LGEditText
    {
        /**
	     * Creates LGComboBox Object From Lua.
	     * @param lc
	     * @return LGComboBox
	     */
	    [LuaFunction(typeof(LuaContext))]
	    public static LGComboBox Create(LuaContext lc)
	    {
		    return new LGComboBox(lc);
	    }

        public LGComboBox(LuaContext context)
            : base(context)
        {
        }

        public LGComboBox(LuaContext context, String luaId)
            : base(context, luaId)
        {
        }

        public override void Setup()
        {
            view = new EditableComboBox();
        }

        /**
         * Add combo item to combobox
         * @param id of combobox
         * @param tag
         */
        [LuaFunction(typeof(String), typeof(Object))]
        public void AddComboItem(String id, Object tag)
        {
            ((EditableComboBox)view).AddItem(id, tag);
        }
    
        /**
         * Show custom button
         * @param value
         */
        [LuaFunction(typeof(Int32))]
        public void ShowCustom(Int32 value)
        {
            ((EditableComboBox)view).ShowCustom = ((value == 1) ? true : false);
        }
    
        /**
         * Show delete button
         * @param value
         */
        [LuaFunction(typeof(Int32))]
        public void ShowDelete(Int32 value)
        {
            ((EditableComboBox)view).ShowCancel = ((value == 1) ? true : false);
        }
    
        /**
         * Set combobox editable
         * @param value (1 or 0)
         */
        [LuaFunction(typeof(Int32))]
        public void SetEditable(Int32 value)
        {
            ((EditableComboBox)view).IsReadOnly = ((value == 1) ? true : false);
        }

        /**
         * Set combobox value
         * @param index
         */
        [LuaFunction(typeof(Int32))]
        public void SetSelected(Int32 index)
        {
            ((EditableComboBox)view).SetSelected(index);
        }
    
        /**
         * Gets the selected name
         * @return name value
         */
        [LuaFunction(false)]
        public String GetSelectedName()
        {
            EditableComboBox.ComboData selectedData = ((EditableComboBox)view).SelectedData;
            return selectedData.name;
        }
    
        /**
         * Gets the selected tag
         * @return tag value
         */
        [LuaFunction(false)]
        public Object GetSelectedTag()
        {
            EditableComboBox.ComboData selectedData = ((EditableComboBox)view).SelectedData;
            return selectedData.tag;
        }

        public override void RegisterEventFunction(string var, LuaTranslator lt)
        {
            if (var == "Changed")
            {
#if WINDOWS_PHONE
                ((EditableComboBox)view).SelectionChanged += new RoutedEventHandler(delegate(object sender, RoutedEventArgs e)
#else
                ((EditableComboBox)view).SelectionChanged += new SelectionChangedEventHandler(delegate(object sender, SelectionChangedEventArgs e)
#endif
                {
                    LuaScriptingEngine.CustomControls.EditableComboBox.ComboData cd = ((EditableComboBox)sender).SelectedData;
                    lt.CallIn(cd.name, cd.tag);
                });
            }
            else
                base.RegisterEventFunction(var, lt);
        }
    }
}
