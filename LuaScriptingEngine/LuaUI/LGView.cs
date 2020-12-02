using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using LuaScriptingEngine.CustomControls;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization;
using LuaScriptingEngine;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
#endif

namespace ScriptingEngine.LuaUI
{
    [LuaClass("LGView")]
    [DataContract]
    public class LGView : FrameworkElement, LuaInterface
    {
        private bool loaded = false;
        String luaId = null;
        public LuaContext lc;
        [DataMember]
        protected FrameworkElement view;
        public List<LGView> subviews = new List<LGView>();

        /**
	    * Creates LGView Object From Lua.
	    * @param lc
	    * @return LGView
	    */
	    [LuaFunction(typeof(LuaContext))]
	    public static LGView Create(LuaContext lc)
	    {
		    return new LGView(lc);
	    }

        public LGView()
        {
        }

        public LGView(LuaContext context)
        {
            this.lc = context;
            Setup();
            AfterSetup();
        }

        public LGView(LuaContext context, String luaId)
        {
            this.luaId = luaId;
            this.lc = context;
            Setup();
            AfterSetup();
        }

        public virtual void Setup()
        {
            view = new BaseView();
        }

        public virtual void AfterSetup()
        {
            if (!(this is LGScrollView || this is LGLinearLayout))
            {
                if (!loaded)
                {
                    LuaEngine.Instance.OnGuiEvent(this, LuaEngine.GuiEvents.GUI_EVENT_CREATE, lc);
                    loaded = true;
                }
            }
        }

        public virtual void Populate(LGView parent)
        {
            if (parent is LGScrollView)
            {
                ((ScrollViewer)parent.view).Content = view;
                if (!parent.IsLoaded)
                {
                    LuaEngine.Instance.OnGuiEvent(parent, LuaEngine.GuiEvents.GUI_EVENT_CREATE, lc);
                    parent.IsLoaded = true;
                }
            }
            else if(parent is LGLinearLayout)
            {
                ((LinearLayout)parent.view).Add(view);
                if (!parent.IsLoaded)
                {
                    LuaEngine.Instance.OnGuiEvent(parent, LuaEngine.GuiEvents.GUI_EVENT_CREATE, lc);
                    parent.IsLoaded = true;
                }
            }
        }

        public FrameworkElement GetView() 
        {
            return view;
        }

        public void SetView(FrameworkElement fe)
        {
            view = fe;
        }

        public bool IsLoaded 
        { 
            get { return loaded; }
            set { loaded = value; }
        }

        public LGView Clone()
        {
            LGView ret = (LGView)MemberwiseClone();
            ret.lc = lc;
            ret.luaId = luaId;
            ret.subviews = new List<LGView>();
            foreach (LGView v in subviews)
            {
                ret.subviews.Add(v.Clone());
            }

            ret.Setup();
            ret.view = (FrameworkElement)LuaViewInflator.Clone(ret.view, view);
            if(ret.view.GetType() == typeof(LinearLayout))
            {
                LinearLayout ll = (LinearLayout)ret.view;
                foreach (LGView cloned in ret.subviews)
                {
                    ll.Add(cloned.view);
                }
            }
            return ret;
        }

        [LuaFunction(typeof(String))]
        public LGView GetViewById(String lId)
	    {
		    if(GetId() != null && GetId() == lId)
			    return this;
		    else
		    {
			    foreach(LGView w in subviews)
			    {
				    LGView wFound = w.GetViewById(lId);
				    if(wFound != null)
					    return wFound;
			    }
		    }
		    return null;
	    }
    
        [LuaFunction(typeof(Int32))]
        public void SetEnabled(Int32 value)
        {
            if(view.GetType() == typeof(Control))
                ((Control)view).IsEnabled = Convert.ToBoolean(value);
        }
    
        [LuaFunction(typeof(Int32))]
        public void SetFocusable(Int32 value)
        {
    	    //view.setEnabled(value.intValue() == 1 ? true : false);
            //view.
        }

        #region LuaInterface Members

        [LuaFunction(typeof(String), typeof(LuaTranslator))]
        public virtual void RegisterEventFunction(string var, LuaTranslator lt)
        {
            if (var == "Click")
            {
#if WINDOWS_PHONE
                view.Tap += new EventHandler<GestureEventArgs>(delegate(object sender, GestureEventArgs e)
                    {
                        lt.CallIn(sender);
                    });
#elif NETFX_CORE
                view.Tapped += new TappedEventHandler(delegate(object sender, TappedRoutedEventArgs e)
                    {
                        lt.CallIn(sender);
                    });
#endif
            }
        }

        public string GetId()
        {
            if (luaId != null)
                return luaId;
            return "LGView";
        }

        #endregion
    }
}
