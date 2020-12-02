using System;
using System.Net;
using System.Windows;
using ScriptingEngine.LuaUI;
using System.Collections.Generic;
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
#else
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
#endif

namespace ScriptingEngine
{
    [LuaClass("LuaTabForm")]
    public class LuaTabForm : LuaInterface
    {
        LuaContext luaContext;
        String luaId;
#if !NETFX_CORE
        List<FormPivotItem> mTabList;

        /**
	     * Creates LuaTabForm Object From Lua.
	     * @param lc
	     * @param luaId
	     * @return LuaTabForm
	     */
	    [LuaFunction(typeof(LuaContext), typeof(String))]
	    public static Object Create(LuaContext lc, String luaId)
	    {
		    LuaTabForm ltf = new LuaTabForm();
            ltf.mTabList = new List<FormPivotItem>();
		    ltf.luaId = luaId;
		    ltf.luaContext = lc;
		    return ltf;
	    }
	
	    /**
	     * Add tab to tabform
	     * @param form tabform value
	     * @param title title of the tab
	     * @param image image of the tab
	     * @param ui xml file of tab
	     */
	    [LuaFunction(typeof(Object), typeof(String), typeof(LuaStream), typeof(String))]
	    public void AddTab(Object form, String title, LuaStream image, String ui)
	    {
            FormPivotItem fpi = new FormPivotItem((LuaForm)form, ui);
            fpi.Title = title;
            mTabList.Add(fpi);
	    }
	
	    /**
	     * 
	     * @param form
	     * @param title
	     * @param image
	     * @param ui
	     */
	    [LuaFunction(typeof(Object), typeof(String), typeof(LuaStream), typeof(LGView))]
	    public void AddTabStream(Object form, String title, LuaStream image, LGView ui)
	    {
		    FormPivotItem fpi = new FormPivotItem((LuaForm)form, ui);
            fpi.Title = title;
            mTabList.Add(fpi);
	    }
	
	    [LuaFunction(typeof(Object), typeof(String), typeof(String), typeof(String), typeof(String))]
	    public void AddTabSrc(Object form, String title, String path, String image, String ui)
	    {
		    FormPivotItem fpi = new FormPivotItem((LuaForm)form, ui);
            fpi.Title = title;
            mTabList.Add(fpi);
	    }
	
	    [LuaFunction(typeof(Object), typeof(String), typeof(String), typeof(String), typeof(LGView))] 
	    public void AddTabSrcStream(Object form, String title, String path, String image, LGView ui)
	    {
		    FormPivotItem fpi = new FormPivotItem((LuaForm)form, ui);
            fpi.Title = title;
            mTabList.Add(fpi);
	    }
	
	    [LuaFunction(typeof(Object))]
	    public void Setup(Object form)
	    {
#if NETFX_CORE
            ((LuaForm)form).
                Controller.Visibility = Visibility.Visible;
#endif
            foreach(FormPivotItem fpi in mTabList)
                ((LuaForm)form).TabController.Items.Add(fpi);
	    }
	
	    [LuaFunction(false)]
	    public void Free()
	    {
		
	    }

#endif
        #region LuaInterface Members

        public void RegisterEventFunction(string var, LuaTranslator lt)
        {
        }

        public string GetId()
        {
            if (luaId != null)
                return luaId;
            return "LuaTabForm";
        }

        #endregion
    }

}
