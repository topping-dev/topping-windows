using System;

using System.Collections.Generic;
using System.Text;

namespace ScriptingEngine
{
    public class LuaObject<T>
    {
        private T m_obj;
        public T obj
        {
            get { return m_obj; }
            set { m_obj = value; }
        }

        public void PushObject(T ptr)
        {
            obj = ptr;
        }
    }    
}
