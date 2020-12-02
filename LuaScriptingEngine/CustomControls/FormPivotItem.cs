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
using MyToolkit.Controls;
#endif
using Microsoft.Phone.Controls;
using ScriptingEngine;
using ScriptingEngine.LuaUI;

namespace LuaScriptingEngine.CustomControls
{
    public class FormPivotItem : PivotItem
    {
        LuaForm form;
        LGView view;

        public FormPivotItem(LuaForm form, LGView ui)
        {
            this.form = form;
            this.view = ui;
            this.form.Content = ui.GetView();
        }

        public FormPivotItem(LuaForm form, String ui)
        {
            this.form = form;
            LuaViewInflator lvi = new LuaViewInflator(form.GetContext());
            view = lvi.ParseFile(ui, null);
            this.form.Content = view.GetView();
        }

        public String Title 
        {
            get
            {
                return (String)Header;
            }
            set
            {
                Header = value;
            }
        }
    }
}