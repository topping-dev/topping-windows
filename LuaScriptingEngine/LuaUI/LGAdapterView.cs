using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using LuaScriptingEngine.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using LuaScriptingEngine;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public struct AdapterItem
    {
        public Int32 id;
        public Object data;
        public LGView view;
    }

    [LuaClass("LGAdapterView")]
    public class LGAdapterView : INotifyCollectionChanged, INotifyPropertyChanged, IList<Object>, LuaInterface
    {
        //Section is GroupHeaderTemplate
        //Item is ItemTemplate
        //The view that shown after clicking section is GroupItemTemplate
        private LuaContext mLc;
        private String id;
        private LuaTranslator ltItemChanged;
        public LGListView par;
        public String Key;
        LGView lastCreatedUI = null;
        private int lastCreatedIndex;
        /*List<LongListGroup<String, AdapterItem>> sectionValues = new List<LongListGroup<String, AdapterItem>>();
        Dictionary<String, List<AdapterItem>> sections = new Dictionary<String, List<AdapterItem>>();*/
        Dictionary<String, LGAdapterView> sections = new Dictionary<string, LGAdapterView>();
        List<AdapterItem> values = new List<AdapterItem>();

        /**
	     * Creates LGAdapterView Object From Lua.
	     * @return LGAdapterView
	     */
        [LuaFunction(typeof(LuaContext), typeof(String))]
        public static LGAdapterView Create(LuaContext lc, String id)
        {
    	    LGAdapterView lgav = new LGAdapterView(lc, id);
    	    return lgav;
        }

        /**
	     * (Ignore)
	     */
        public LGAdapterView(LuaContext lc, String id)
        {
            mLc = lc;
            this.id = id;
        }

        /**
	     * (Ignore)
	     */
        public LuaTranslator GetItemChanged()
        {
            return ltItemChanged;
        }

        /**
        * Add section
        * @param header of section
        * @param id of LGAdapterView
        */
	    [LuaFunction(typeof(String), typeof(String))]
	    public LGAdapterView AddSection(String header, String id)
	    {
		    LGAdapterView lgav = new LGAdapterView(mLc, id);
		    //sectionsValues.Add(header);
		    sections.Add(header, lgav);
		    return lgav;
	    }
	
	    /**
	    * Remove section
	    * @param header value
	    */
	    [LuaFunction(typeof(String))]
	    public void RemoveSection(String header)
	    {
		    //sectionsValues.Remove(header);
		    sections.Remove(header);
	    }
	
	    /**
	    * Add Value to adapter
	    * @param id of value
	    * @param value
	    */
	    [LuaFunction(typeof(Int32), typeof(Object))]
	    public void AddValue(Int32 id, Object value)
	    {
            AdapterItem ai = new AdapterItem();
            ai.id = id;
            ai.data = value;
            values.Add(ai);
            if(CollectionChanged != null)
                CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, id));
	    }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }
	
	    /**
	    * Remove Value from adapter
	    * @param id of value
	    */
	    [LuaFunction(typeof(Int32))]
	    public void RemoveValue(Int32 id)
	    {
            AdapterItem? aiToRemove = null;
            foreach (AdapterItem ai in values)
            {
                if (ai.id == id)
                {
                    aiToRemove = ai;
                    break;
                }
            }
            if(aiToRemove != null)
                values.Remove(((AdapterItem)aiToRemove));
            if (CollectionChanged != null)
                CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, aiToRemove, id));
	    }

        [LuaFunction(false)]
        public void RemoveAllValues()
        {
            values.Clear();
            if (CollectionChanged != null)
                CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    
        #region LuaInterface Members

        [LuaFunction(typeof(String), typeof(LuaTranslator))]
        public void  RegisterEventFunction(string var, LuaTranslator lt)
        {
 	        if(var == "ItemSelected")
            {
                ltItemChanged = lt;
            }
        }

        public string  GetId()
        {
 	        if(id == null)
                return "LGAdapterView";
            return id;
        }

        #endregion

        #region IList<object> Members

        public int IndexOf(object item)
        {
            int count = 0;
            foreach(AdapterItem ai in values)
            {
                if (ai.data == item)
                    return count;

                ++count;
            }
            return -1;
        }

        public void Insert(int index, object item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public object this[int index]
        {
            get
            {
                /*AdapterItem ai = values[index];
                LGView gui = (LGView)LuaEngine.Instance.OnGuiEvent(this, LuaEngine.GuiEvents.GUI_EVENT_ADAPTERVIEW, par, index, ai.data, lastCreatedUI, mLc);
                ai.view = gui;
#if !NETFX_CORE //We don't need to clone ui on winrt
                lastCreatedUI = gui.Clone();
#endif
                return ai.view.GetView();*/

                AdapterItem ai = values[index];
#if !NETFX_CORE
                if (lastCreatedUI != null && lastCreatedIndex == index && ai.view != null)
                {
                    return ai.view.GetView();
                }
#endif
                LGView gui = (LGView)LuaEngine.Instance.OnGuiEvent(this, LuaEngine.GuiEvents.GUI_EVENT_ADAPTERVIEW, par, index, ai.data, lastCreatedUI, mLc);
                ai.view = gui;
                values[index] = ai;
#if !NETFX_CORE
                lastCreatedUI = gui.Clone();
                lastCreatedIndex = index;
#endif
                return ai.view.GetView();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<object> Members

        public void Add(object item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(object item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return values.Count; }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(object item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<object> Members

        public IEnumerator<object> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        #endregion

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
